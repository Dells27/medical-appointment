using AppointmentService.Application.DTOs;
using AppointmentService.Application.Interfaces;
using AppointmentService.Application.UseCases.CreateAppointment;
using AppointmentService.Application.UsesCases.Scheduling;

namespace AppointmentService.Application.UsesCases.CancelAppointment;

public class CancelAppointmentHandler
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICacheService _cacheService;
    private readonly IEventPublisher _eventPublisher;
    private readonly DoctorLookup _doctorLookup;

    public CancelAppointmentHandler (IAppointmentRepository appointmentRepository, ICacheService cacheService,
        IEventPublisher eventPublisher, DoctorLookup doctorLookup)
    {
        _appointmentRepository = appointmentRepository;
        _cacheService = cacheService;
        _eventPublisher = eventPublisher;
        _doctorLookup = doctorLookup;
    }


    // Only the patient or the doctor of the appointment can cancel it
    public async Task<AppointmentResponse> Handle (Guid appointmentId, string reason, Guid userId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync (appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found");

        if (!appointment.BelongsTo(userId))
            throw new UnauthorizedAccessException("You cannot cancel this appointment");

        var byRole = appointment.IsDoctor(userId) ? "Doctor" : "Patient";
        appointment.Cancel(reason, byRole);
        await _appointmentRepository.SaveChangesAsync();

        var cacheKey = CreateAppointmentHandler.SlotCacheKey(appointment.doctorId, appointment.appointmentDate, appointment.appointmentTime);
        await _cacheService.RemoveAsync (cacheKey);

        // The Notification Service tells the other party
        var doctor = await _doctorLookup.TryGetAsync(appointment.doctorId);
        await _eventPublisher.PublishAsync("appointment.cancelled", new AppointmentCancelledEvent
        {
            appointmentId = appointment.Id,
            patientId = appointment.patientId,
            doctorId = appointment.doctorId,
            appointmentDate = appointment.appointmentDate,
            appointmentTime = appointment.appointmentTime,
            reason = appointment.cancellationReason ?? string.Empty,
            cancelledByRole = byRole,
            patientEmail = appointment.patientEmail ?? string.Empty,
            patientName = appointment.patientName ?? string.Empty,
            doctorEmail = doctor?.Email ?? string.Empty,
            doctorName = doctor?.Name ?? string.Empty
        });

        return CreateAppointmentHandler.MapToResponse (appointment);
    }
}
