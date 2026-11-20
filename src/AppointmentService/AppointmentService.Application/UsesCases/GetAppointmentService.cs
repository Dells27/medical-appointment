using AppointmentService.Application.DTOs;
using AppointmentService.Application.Interfaces;
using AppointmentService.Application.UseCases.CreateAppointment;

namespace AppointmentService.Application.UsesCases.GetAppointment;

public class GetAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAppointmentService (IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<List<AppointmentResponse>> HandleByPatient (Guid patientId)
    {
        var appointments = await _appointmentRepository.GetByPatientIdAsync (patientId);
        return appointments.Select(CreateAppointmentHandler.MapToResponse).ToList();
    }

    public async Task<List<AppointmentResponse>> HandleByDoctor (Guid doctorId)
    {
        var appointments = await _appointmentRepository.GetByDoctorIdAsync (doctorId);
        return appointments.Select(CreateAppointmentHandler.MapToResponse).ToList ();
    }

    // Only the patient or the doctor of the appointment can read it
    public async Task<AppointmentResponse> HandleById(Guid id, Guid userId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync (id)
            ?? throw new KeyNotFoundException("Appointment not found");

        if (!appointment.BelongsTo(userId))
            throw new UnauthorizedAccessException("You cannot access this appointment");

        return CreateAppointmentHandler.MapToResponse(appointment);
    }

}
