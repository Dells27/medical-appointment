using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using HealthChecks.Uris;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. YARP — lee la configuración de rutas y clusters del appsettings.json
// ============================================================
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ============================================================
// 2. RATE LIMITING
// ============================================================
builder.Services.AddRateLimiter(options =>
{
    // Política general — aplica a todo el tráfico
    options.AddFixedWindowLimiter("global", opt =>
    {
        opt.PermitLimit = 100;              // 100 requests
        opt.Window = TimeSpan.FromMinutes(1); // por minuto
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0; // sin cola — rechaza inmediatamente si se excede
    });

    // Política más estricta específica para auth — previene fuerza bruta en login
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1); // por minuto
        opt.QueueLimit = 0;
    });

    // Qué pasa cuando se excede el límite
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync(
            "Demasiadas solicitudes. Por favor intentá de nuevo en un momento.", token);
    };
});

// ============================================================
// 3. HEALTH CHECKS
// ============================================================
builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri("https://localhost:7248/swagger/v1/swagger.json"), name: "auth-service", tags: new[] { "services" })
    .AddUrlGroup(new Uri("https://localhost:7098/swagger/v1/swagger.json"), name: "doctor-service", tags: new[] { "services" })
    .AddUrlGroup(new Uri("https://localhost:7187/swagger/v1/swagger.json"), name: "patient-service", tags: new[] { "services" })
    .AddUrlGroup(new Uri("https://localhost:7283/swagger/v1/swagger.json"), name: "appointment-service", tags: new[] { "services" })
    .AddUrlGroup(new Uri("https://localhost:7280/swagger/v1/swagger.json"), name: "medical-record-service", tags: new[] { "services" });

// ============================================================
// 4. CORS
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseRateLimiter();

// ============================================================
// Endpoint de Health Check
// ============================================================
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var result = System.Text.Json.JsonSerializer.Serialize(new
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

// ============================================================
// Rutas con rate limiting aplicado
// ============================================================
app.MapReverseProxy().RequireRateLimiting("global");

app.Run();