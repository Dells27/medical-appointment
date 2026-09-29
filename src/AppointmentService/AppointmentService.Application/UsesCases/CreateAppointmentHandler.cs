using AppointmentService.Application.DTOs;
using AppointmentService.Application.Interfaces;
using AppointmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Application.UseCases.CreateAppointment;

public class CreateAppointmentHandler
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICacheService _cacheService;
    private readonly IEventPublisher _eventPublisher;

    public CreateAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        ICacheService cacheService,
        IEventPublisher eventPublisher)
    {
        _appointmentRepository = appointmentRepository;
        _cacheService = cacheService;
        _eventPublisher = eventPublisher;
    }

    public async Task<AppointmentResponse> Handle(CreateAppointmentRequest request)
    {
        // 1. Verificar disponibilidad — primero en Redis (rápido)
        var cacheKey = $"slot:{request.doctorId}:{request.appointmentDate:yyyy-MM-dd}:{request.appointmentTime}";
        var isTakenInCache = await _cacheService.GetAsync<bool?>(cacheKey);

        if (isTakenInCache == true)
            throw new Exception("Este horario ya no está disponible");

        // 2. Verificar en la base de datos (fuente de verdad)
        var isTaken = await _appointmentRepository.IsSlotTakenAsync(
            request.doctorId, request.appointmentDate, request.appointmentTime);

        if (isTaken)
        {
            // Actualizar cache para futuras consultas
            await _cacheService.SetAsync(cacheKey, true, TimeSpan.FromMinutes(30));
            throw new Exception("Este horario ya no está disponible");
        }

        // 3. Crear la cita
        var appointment = Appointment.Create(
            request.PatientID,
            request.doctorId,
            request.appointmentDate,
            request.appointmentTime,
            request.reason);

        await _appointmentRepository.AddAsync(appointment);
        try
        {
            // 4. Intentar guardar — el constraint único de PostgreSQL
            // es la garantía REAL contra race conditions
            await _appointmentRepository.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Si dos requests llegaron al mismo tiempo, PostgreSQL rechaza
            // el segundo automáticamente gracias al índice único
            await _cacheService.SetAsync(cacheKey, true, TimeSpan.FromMinutes(30));
            throw new Exception("Este horario acaba de ser tomado por otro paciente. Por favor elegí otro horario.");
        }

        // 5. Marcar el slot como ocupado en Redis
        await _cacheService.SetAsync(cacheKey, true, TimeSpan.FromDays(1));

        // 6. Publicar evento en RabbitMQ
        await _eventPublisher.PublishAsync("appointment.created", new AppointmentCreatedEvent
        {
            appointmentId = appointment.Id,
            patientId = appointment.patientId,
            doctorId = appointment.doctorId,
            appointmentDate = appointment.appointmentDate,
            appointmentTime = appointment.appointmentTime,
            PatientEmail = request.PatientEmail

        });

        return MapToResponse(appointment);
    }

    // Detecta si el error es específicamente por el constraint único
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("duplicate key") == true ||
               ex.InnerException?.Message.Contains("unique constraint") == true;
    }


    public static AppointmentResponse MapToResponse(Appointment appointment)
    {
        return new AppointmentResponse
        {
            Id = appointment.Id,
            patientId = appointment.patientId,
            doctorId = appointment.doctorId,
            appointmentDate = appointment.appointmentDate,
            appointmentTime = appointment.appointmentTime,
            status = appointment.status.ToString(),
            reason = appointment.reason,
            cancellationReason = appointment.cancellationReason,
            createdAt = appointment.createdAt
        };
    }
}