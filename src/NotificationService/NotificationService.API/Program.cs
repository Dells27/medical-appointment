using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NotificationService.Application.Interfaces;
using NotificationService.Application.UsesCases.HandleAppointmentCreated;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Data.Repositories;
using NotificationService.Infrastructure.Services;
using NotificationService.Infrastructure.Workers;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. BASE DE DATOS
// ============================================================
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================
// 2. RABBITMQ
// ============================================================
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:HostName"]!,
        UserName = builder.Configuration["RabbitMQ:UserName"]!,
        Password = builder.Configuration["RabbitMQ:Password"]!
    };

    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

// ============================================================
// 3. INYECCIÓN DE DEPENDENCIAS
// ============================================================
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<HandleAppointmentCreatedHandler>();

// ============================================================
// 4. EL BACKGROUND SERVICE — lo más importante de este servicio
// ============================================================
builder.Services.AddHostedService<AppointmentEventsWorker>();

// ============================================================
// 5. CONTROLLERS Y SWAGGER
// Los mantenemos por si en el futuro agregamos endpoints
// como "ver historial de notificaciones enviadas"
// ============================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notification Service API",
        Version = "v1",
        Description = "Servicio de notificaciones del Sistema de Citas Médicas"
    });
});

var app = builder.Build();

// ============================================================
// 6. MIGRACIONES AUTOMÁTICAS
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    db.Database.Migrate();
}

// ============================================================
// 7. MIDDLEWARE PIPELINE
// ============================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();