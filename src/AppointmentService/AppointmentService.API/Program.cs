using AppointmentService.Application.Interfaces;
using AppointmentService.Application.UseCases.CreateAppointment;
using AppointmentService.Application.UsesCases.GetAppointment;
using AppointmentService.Application.UsesCases.CancelAppointment;
using AppointmentService.Application.UsesCases.CompleteAppointmentHandler;
using AppointmentService.Infrastructure.Data;
using AppointmentService.Infrastructure.Data.Repositories;
using AppointmentService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. BASE DE DATOS
// ============================================================
builder.Services.AddDbContext<AppointmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================
// 2. REDIS
// ============================================================
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration["Redis:ConnectionString"]!;
    return ConnectionMultiplexer.Connect(connectionString);
});

// ============================================================
// 3. RABBITMQ
// ============================================================
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:HostName"]!,
        UserName = builder.Configuration["RabbitMQ:UserName"]!,
        Password = builder.Configuration["RabbitMQ:Password"]!
    };

    // CreateConnectionAsync es async — lo esperamos de forma síncrona solo al arrancar
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

// ============================================================
// 4. INYECCIÓN DE DEPENDENCIAS
// ============================================================
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
builder.Services.AddScoped<CreateAppointmentHandler>();
builder.Services.AddScoped<CancelAppointmentHandler>();
builder.Services.AddScoped<GetAppointmentService>();
builder.Services.AddScoped<CompleteAppointmentHandler>();

// ============================================================
// 5. JWT
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
// 6. CONTROLLERS Y SWAGGER
// ============================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Appointment Service API",
        Version = "v1",
        Description = "Servicio de citas del Sistema de Citas Médicas"
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
// 7. CORS
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
// 8. MIGRACIONES AUTOMÁTICAS
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
    db.Database.Migrate();
}

// ============================================================
// 9. MIDDLEWARE PIPELINE
// ============================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();