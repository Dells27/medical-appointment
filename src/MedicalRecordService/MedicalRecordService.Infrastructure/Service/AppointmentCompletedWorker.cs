using MedicalRecordService.Application.DTOs;
using MedicalRecordService.Application.UseCases.CreateEmptyRecord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace MedicalRecordService.Infrastructure.Workers;

public class AppointmentCompletedWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnection _connection;
    private readonly ILogger<AppointmentCompletedWorker> _logger;

    public AppointmentCompletedWorker(
        IServiceScopeFactory scopeFactory,
        IConnection connection,
        ILogger<AppointmentCompletedWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _connection = connection;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: "medic_events",
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: "medical_record_service_queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: "medical_record_service_queue",
            exchange: "medic_events",
            routingKey: "appointment.completed",
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var routingKey = ea.RoutingKey;
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            _logger.LogInformation("Evento recibido: {RoutingKey}. JSON: {Json}", routingKey, json);

            try
            {
                using var scope = _scopeFactory.CreateScope();

                if (routingKey == "appointment.completed")
                {
                    var appointmentEvent = JsonSerializer.Deserialize<AppointmentCompletedEvent>(json);
                    var handler = scope.ServiceProvider.GetRequiredService<CreateEmptyRecordHandler>();
                    await handler.Handle(appointmentEvent!);
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando evento {RoutingKey}", routingKey);
                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
            }
        };

        await channel.BasicConsumeAsync(
            queue: "medical_record_service_queue",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}