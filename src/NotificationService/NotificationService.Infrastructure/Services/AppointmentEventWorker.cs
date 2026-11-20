using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.UsesCases;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotificationService.Infrastructure.Workers;

// Runs for the whole lifetime of the app, listening to appointment events
public class AppointmentEventsWorker : BackgroundService
{
    private const string Exchange = "medic_events";
    private const string Queue = "notification_service_queue";

    private static readonly string[] RoutingKeys =
    {
        "appointment.created",
        "appointment.confirmed",
        "appointment.cancelled",
        "appointment.rescheduled"
    };

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

        await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(Queue, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: stoppingToken);

        foreach (var routingKey in RoutingKeys)
            await channel.QueueBindAsync(Queue, Exchange, routingKey, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var routingKey = ea.RoutingKey;
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());

            _logger.LogInformation("Event received: {RoutingKey}", routingKey);
            try
            {
                // Handlers are scoped (DbContext), so each message gets its own scope
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<AppointmentNotificationsHandler>();

                switch (routingKey)
                {
                    case "appointment.created":
                        await handler.HandleCreated(Deserialize<AppointmentCreatedEvent>(json));
                        break;
                    case "appointment.confirmed":
                        await handler.HandleConfirmed(Deserialize<AppointmentConfirmedEvent>(json));
                        break;
                    case "appointment.cancelled":
                        await handler.HandleCancelled(Deserialize<AppointmentCancelledEvent>(json));
                        break;
                    case "appointment.rescheduled":
                        await handler.HandleRescheduled(Deserialize<AppointmentRescheduledEvent>(json));
                        break;
                    default:
                        _logger.LogWarning("Unhandled routing key {RoutingKey}", routingKey);
                        break;
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {RoutingKey}", routingKey);
                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
            }
        };

        await channel.BasicConsumeAsync(Queue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException($"Empty {typeof(T).Name} payload");
}
