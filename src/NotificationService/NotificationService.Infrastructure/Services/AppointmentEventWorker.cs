using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.UsesCases.HandleAppointmentCreated;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotificationService.Infrastructure.Workers;

// BackgroundService corre en segundo plano durante toda la vida de la aplicación
// A diferencia de un Controller, no espera requests HTTP — escucha eventos continuamente
public class AppointmentEventsWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnection _connection;
    private readonly ILogger<AppointmentEventsWorker> _logger;

    public AppointmentEventsWorker(
        IServiceScopeFactory scopeFactory,
        IConnection connection,
        ILogger<AppointmentEventsWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _connection = connection;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Declaramos el mismo exchange que usa Appointment Service
        await channel.ExchangeDeclareAsync(
            exchange: "medic_events",
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        // Declaramos una cola exclusiva para este servicio
        var queueDeclareResult = await channel.QueueDeclareAsync(
            queue: "notification_service_queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // Nos suscribimos a los eventos que nos interesan
        await channel.QueueBindAsync(
            queue: "notification_service_queue",
            exchange: "medic_events",
            routingKey: "appointment.created",
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: "notification_service_queue",
            exchange: "medic_events",
            routingKey: "appointment.cancelled",
            cancellationToken: stoppingToken);

        // El consumer procesa cada mensaje que llega
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var routingKey = ea.RoutingKey;
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            _logger.LogInformation(
                "Evento recibido: {RoutingKey}",
                routingKey);
            try
            {
                // Cada request necesita su propio scope porque los handlers son Scoped
                using var scope = _scopeFactory.CreateScope();

                if (routingKey == "appointment.created")
                {
                    var appointmentEvent = JsonSerializer.Deserialize<AppointmentCreatedEvent>(json);
                    var handler = scope.ServiceProvider.GetRequiredService<HandleAppointmentCreatedHandler>();
                    await handler.Handle(appointmentEvent!);
                }
                // Aquí se agregarían más "else if" para otros tipos de eventos

                // Confirmamos que procesamos el mensaje exitosamente
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando evento {RoutingKey}", routingKey);
                // Rechazamos el mensaje — RabbitMQ puede reintentarlo
                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
            }
        };

        await channel.BasicConsumeAsync(
            queue: "notification_service_queue",
            autoAck: false, // Confirmamos manualmente después de procesar
            consumer: consumer,
            cancellationToken: stoppingToken);

        // Mantiene el worker corriendo hasta que la app se detenga
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}