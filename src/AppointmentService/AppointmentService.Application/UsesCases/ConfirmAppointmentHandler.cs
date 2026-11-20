using AppointmentService.Application.DTOs;
using AppointmentService.Application.Interfaces;
using AppointmentService.Application.UseCases.CreateAppointment;
using AppointmentService.Application.UsesCases.Scheduling;

namespace AppointmentService.Application.UsesCases.ConfirmAppointment;

public class ConfirmAppointmentHandler
{
    private readonly IAppointmentRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly DoctorLookup _doctorLookup;

    public ConfirmAppointmentHandler(IAppointmentRepository repository, IEventPublisher eventPublisher, DoctorLookup doctorLookup)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _doctorLookup = doctorLookup;
    }

    // Only the doctor of the appointment can confirm it; the patient gets an email
    public async Task<AppointmentResponse> Handle(Guid appointmentId, Guid doctorUserId)
    {
        var appointment = await _repository.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found");

        if (!appointment.IsDoctor(doctorUserId))
            throw new UnauthorizedAccessException("You cannot confirm this appointment");

        appointment.Confirm();
        await _repository.SaveChangesAsync();

        var doctor = await _doctorLookup.TryGetAsync(appointment.doctorId);
        await _eventPublisher.PublishAsync("appointment.confirmed", new AppointmentConfirmedEvent
        {
            appointmentId = appointment.Id,
            patientId = appointment.patientId,
            doctorId = appointment.doctorId,
            appointmentDate = appointment.appointmentDate,
            appointmentTime = appointment.appointmentTime,
            patientEmail = appointment.patientEmail ?? string.Empty,
            patientName = appointment.patientName ?? string.Empty,
            doctorName = doctor?.Name ?? string.Empty
        });

        return CreateAppointmentHandler.MapToResponse(appointment);
    }
}
