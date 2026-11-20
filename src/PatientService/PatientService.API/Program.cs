using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PatientService.Application.Interfaces;
using PatientService.Application.UseCases;
using PatientService.Application.UseCases.UploadDocument;
using PatientService.Infrastructure.Data;
using PatientService.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. BASE DE DATOS
// ============================================================
builder.Services.AddDbContext<PatientDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================
// 2. AWS S3
// ============================================================
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = new AmazonS3Config
    {
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(
            builder.Configuration["AWS:Region"]!)
    };

    return new AmazonS3Client(
        builder.Configuration["AWS:AccessKey"],
        builder.Configuration["AWS:SecretKey"],
        config);
});

// ============================================================
// 3. INYECCIÓN DE DEPENDENCIAS
// ============================================================
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IS3Service, S3Service>();
builder.Services.AddScoped<CreatePatientHandler>();
builder.Services.AddScoped<UploadDocumentHandler>();

// Service-to-service HTTP: the doctor's JWT is forwarded to the Appointment Service
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ForwardAuthorizationHandler>();
builder.Services.AddHttpClient<IAppointmentServiceClient, AppointmentServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AppointmentService"]
        ?? throw new InvalidOperationException("Missing configuration: Services:AppointmentService"));
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddHttpMessageHandler<ForwardAuthorizationHandler>();

// ============================================================
// 4. JWT
// ============================================================
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var jwtSecret = builder.Configuration["Jwt:Secret"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();

// ============================================================
// 5. CONTROLLERS Y SWAGGER
// ============================================================
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddExceptionHandler<PatientService.API.Middleware.ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Patient Service API",
        Version = "v1",
        Description = "Servicio de pacientes del Sistema de Citas Médicas"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresá el token así: Bearer {tu token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================================
// 6. CORS
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

// ============================================================
// 7. MIGRACIONES AUTOMÁTICAS
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PatientDbContext>();
    db.Database.Migrate();
}

// ============================================================
// 8. MIDDLEWARE PIPELINE
// ============================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();