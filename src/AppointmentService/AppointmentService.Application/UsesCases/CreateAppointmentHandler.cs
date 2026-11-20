using AppointmentService.Application.DTOs;
using AppointmentService.Application.Interfaces;
using AppointmentService.Application.UsesCases.Scheduling;
using AppointmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Application.UseCases.CreateAppointment;

public class CreateAppointmentHandler
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICacheService _cacheService;
    private readonly IEventPublisher _eventPublisher;
    private readonly SlotAvailabilityService _slots;
    private readonly DoctorLookup _doctorLookup;

    public CreateAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        ICacheService cacheService,
        IEventPublisher eventPublisher,
        SlotAvailabilityService slots,
        DoctorLookup doctorLookup)
    {
        _appointmentRepository = appointmentRepository;
        _cacheService = cacheService;
        _eventPublisher = eventPublisher;
        _slots = slots;
        _doctorLookup = doctorLookup;
    }

    public static string SlotCacheKey(Guid doctorId, DateTime date, TimeOnly time) =>
        $"slot:{doctorId}:{date:yyyy-MM-dd}:{time}";

    public async Task<AppointmentResponse> Handle(CreateAppointmentRequest request)
    {
        var appointmentDate = DateTime.SpecifyKind(request.appointmentDate.Date, DateTimeKind.Utc);

        // 1. Quick rejection — Redis remembers recently taken slots
        var cacheKey = SlotCacheKey(request.doctorId, appointmentDate, request.appointmentTime);
        var isTakenInCache = await _cacheService.GetAsync<bool?>(cacheKey);

        if (isTakenInCache == true)
            throw new InvalidOperationException("This time slot is no longer available");

        // 2. The slot must be inside the doctor's working hours, not blocked, not taken and not in the past
        await _slots.EnsureSlotIsBookableAsync(request.doctorId, appointmentDate, request.appointmentTime);

        // 3. Create the appointment
        var appointment = Appointment.Create(
            request.PatientID,
            request.doctorId,
            appointmentDate,
            request.appointmentTime,
            request.reason,
            request.PatientName,
            request.PatientEmail);

        await _appointmentRepository.AddAsync(appointment);
        try
        {
            // 4. The PostgreSQL unique index is the real guarantee against race conditions
            await _appointmentRepository.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            await _cacheService.SetAsync(cacheKey, true, TimeSpan.FromMinutes(30));
            throw new InvalidOperationException("This time slot was just taken by another patient. Please choose another time.");
        }

        // 5. Mark the slot as taken in Redis
        await _cacheService.SetAsync(cacheKey, true, TimeSpan.FromDays(1));

        // 6. Publish the event — the Notification Service emails the patient and the doctor
        var doctor = await _doctorLookup.TryGetAsync(appointment.doctorId);
        await _eventPublisher.PublishAsync("appointment.created", new AppointmentCreatedEvent
        {
            appointmentId = appointment.Id,
            patientId = appointment.patientId,
            doctorId = appointment.doctorId,
            appointmentDate = appointment.appointmentDate,
            appointmentTime = appointment.appointmentTime,
            reason = appointment.reason,
            PatientEmail = appointment.patientEmail ?? string.Empty,
            patientName = appointment.patientName ?? string.Empty,
            doctorEmail = doctor?.Email ?? string.Empty,
            doctorName = doctor?.Name ?? string.Empty
        });

        return MapToResponse(appointment);
    }

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
            patientName = appointment.patientName,
            patientEmail = appointment.patientEmail,
            appointmentDate = appointment.appointmentDate,
            appointmentTime = appointment.appointmentTime,
            status = appointment.status.ToString(),
            reason = appointment.reason,
            cancellationReason = appointment.cancellationReason,
            cancelledByRole = appointment.cancelledByRole,
            rescheduledAt = appointment.rescheduledAt,
            createdAt = appointment.createdAt
        };
    }
}
