using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;

const string AuthCookieName = "medic_token";

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. YARP — routes and clusters come from appsettings.json.
//    The JWT lives in an httpOnly cookie; downstream services expect
//    an Authorization header, so we copy the cookie into it.
// ============================================================
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        context.AddRequestTransform(transform =>
        {
            var token = transform.HttpContext.Request.Cookies[AuthCookieName];
            if (!string.IsNullOrEmpty(token) && transform.ProxyRequest.Headers.Authorization is null)
            {
                transform.ProxyRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            return ValueTask.CompletedTask;
        });
    });

// ============================================================
// 2. RATE LIMITING — partitioned per client IP, so one client
//    cannot exhaust the quota for everyone else.
//    Each YARP route picks its policy via "RateLimiterPolicy".
// ============================================================
var rateLimits = builder.Configuration.GetSection("RateLimiting");
var globalPermitLimit = rateLimits.GetValue("GlobalPermitLimit", 100);
var authPermitLimit = rateLimits.GetValue("AuthPermitLimit", 10);

static string ClientKey(HttpContext context) =>
    context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("global", context =>
        RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = globalPermitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));

    // Stricter policy for auth endpoints — slows down brute-force login attempts
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = authPermitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            JsonSerializer.Serialize(new { message = "Too many requests. Please try again in a moment." }),
            token);
    };
});

// ============================================================
// 3. AUTHENTICATION — the gateway validates the JWT before proxying.
//    Routes opt in/out via "AuthorizationPolicy" ("default" / "anonymous").
// ============================================================
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!))
        };

        // Read the token from the httpOnly cookie when there is no Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token))
                    context.Token = context.Request.Cookies[AuthCookieName];
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// ============================================================
// 4. HEALTH CHECKS — one per proxied cluster, built from the
//    ReverseProxy config so no service URL is hard-coded here.
// ============================================================
var healthChecks = builder.Services.AddHealthChecks();
foreach (var cluster in builder.Configuration.GetSection("ReverseProxy:Clusters").GetChildren())
{
    var address = cluster.GetSection("Destinations").GetChildren()
        .Select(d => d["Address"])
        .FirstOrDefault(a => !string.IsNullOrWhiteSpace(a));

    if (address is null) continue;

    healthChecks.AddUrlGroup(
        new Uri(new Uri(address), "health"),
        name: cluster.Key.Replace("-cluster", "-service"),
        tags: new[] { "services" });
}

// ============================================================
// 5. CORS — the browser sends the auth cookie, so credentials are allowed
//    and origins must be explicit.
// ============================================================
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// The frontend proxies /api/* through Next.js, so the real client IP arrives in
// X-Forwarded-For. Only loopback proxies are trusted by default.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// ============================================================
// Health endpoint
// ============================================================
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            services = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                responseTime = e.Value.Duration.TotalMilliseconds
            })
        });

        await context.Response.WriteAsync(result);
    }
});

app.MapReverseProxy();

app.Run();
