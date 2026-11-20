using AppointmentService.Application.DTOs;
using AppointmentService.Application.Interfaces;
using AppointmentService.Application.UseCases.CreateAppointment;
using AppointmentService.Application.UsesCases.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Application.UsesCases.RescheduleAppointment;

public class RescheduleAppointmentHandler
{
    private readonly IAppointmentRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly IEventPublisher _eventPublisher;
    private readonly SlotAvailabilityService _slots;
    private readonly DoctorLookup _doctorLookup;

    public RescheduleAppointmentHandler(IAppointmentRepository repository, ICacheService cacheService,
        IEventPublisher eventPublisher, SlotAvailabilityService slots, DoctorLookup doctorLookup)
    {
        _repository = repository;
        _cacheService = cacheService;
        _eventPublisher = eventPublisher;
        _slots = slots;
        _doctorLookup = doctorLookup;
    }

    // Only the doctor of the appointment can move it; the patient gets an email
    public async Task<AppointmentResponse> Handle(Guid appointmentId, RescheduleAppointmentRequest request, Guid doctorUserId)
    {
        var appointment = await _repository.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found");

        if (!appointment.IsDoctor(doctorUserId))
            throw new UnauthorizedAccessException("You cannot reschedule this appointment");

        var newDate = DateTime.SpecifyKind(request.appointmentDate.Date, DateTimeKind.Utc);
        await _slots.EnsureSlotIsBookableAsync(appointment.doctorId, newDate, request.appointmentTime, appointment.Id);

        var previousDate = appointment.appointmentDate;
        var previousTime = appointment.appointmentTime;

        appointment.Reschedule(newDate, request.appointmentTime);
        try
        {
            await _repository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("This time slot was just taken. Please choose another time.");
        }

        // Free the old slot and mark the new one as taken
        await _cacheService.RemoveAsync(CreateAppointmentHandler.SlotCacheKey(appointment.doctorId, previousDate, previousTime));
        await _cacheService.SetAsync(
            CreateAppointmentHandler.SlotCacheKey(appointment.doctorId, newDate, request.appointmentTime),
            true, TimeSpan.FromDays(1));

        var doctor = await _doctorLookup.TryGetAsync(appointment.doctorId);
        await _eventPublisher.PublishAsync("appointment.rescheduled", new AppointmentRescheduledEvent
        {
            appointmentId = appointment.Id,
            patientId = appointment.patientId,
            doctorId = appointment.doctorId,
            previousDate = previousDate,
            previousTime = previousTime,
            appointmentDate = appointment.appointmentDate,
            appointmentTime = appointment.appointmentTime,
            patientEmail = appointment.patientEmail ?? string.Empty,
            patientName = appointment.patientName ?? string.Empty,
            doctorEmail = doctor?.Email ?? string.Empty,
            doctorName = doctor?.Name ?? string.Empty
        });

        return CreateAppointmentHandler.MapToResponse(appointment);
    }
}
