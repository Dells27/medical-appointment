using AppointmentService.Application.DTOs;
using AppointmentService.Application.Interfaces;
using AppointmentService.Application.UseCases.CreateAppointment;
using AppointmentService.Domain.Entities;

namespace AppointmentService.Application.UsesCases.DoctorPatients;

// The doctor's view of their patients, built from their appointments
public class DoctorPatientsHandler
{
    private readonly IAppointmentRepository _repository;

    public DoctorPatientsHandler(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<DoctorPatientResponse>> GetMyPatients(Guid doctorUserId)
    {
        var appointments = await _repository.GetByDoctorIdAsync(doctorUserId);
        var today = DateTime.Now.Date; // appointment dates are calendar dates (wall clock)

        return appointments
            .Where(a => a.status != AppointmentStatus.Cancelled)
            .GroupBy(a => a.patientId)
            .Select(g =>
            {
                // Latest known name/email (older appointments may not have them)
                var latest = g.OrderByDescending(a => a.createdAt).First();
                var completed = g.Where(a => a.status == AppointmentStatus.Completed).ToList();
                var next = g
                    .Where(a => a.IsActive && a.appointmentDate.Date >= today)
                    .OrderBy(a => a.appointmentDate).ThenBy(a => a.appointmentTime)
                    .FirstOrDefault();

                return new DoctorPatientResponse
                {
                    patientId = g.Key,
                    patientName = g.Select(a => a.patientName).FirstOrDefault(n => !string.IsNullOrEmpty(n)) ?? latest.patientName,
                    patientEmail = g.Select(a => a.patientEmail).FirstOrDefault(e => !string.IsNullOrEmpty(e)) ?? latest.patientEmail,
                    completedVisits = completed.Count,
                    totalAppointments = g.Count(),
                    lastVisit = completed.Count > 0 ? completed.Max(a => a.appointmentDate) : null,
                    nextAppointmentDate = next?.appointmentDate,
                    nextAppointmentTime = next?.appointmentTime
                };
            })
            .OrderBy(p => p.patientName ?? string.Empty)
            .ToList();
    }

    // All appointments between the doctor and one patient (403 if they never met)
    public async Task<List<AppointmentResponse>> GetAppointmentsWithPatient(Guid doctorUserId, Guid patientUserId)
    {
        await EnsureRelationship(doctorUserId, patientUserId);
        var appointments = await _repository.GetByDoctorAndPatientAsync(doctorUserId, patientUserId);
        return appointments.Select(CreateAppointmentHandler.MapToResponse).ToList();
    }

    public Task<bool> HasRelationship(Guid doctorUserId, Guid patientUserId) =>
        _repository.HasRelationshipAsync(doctorUserId, patientUserId);

    private async Task EnsureRelationship(Guid doctorUserId, Guid patientUserId)
    {
        if (!await _repository.HasRelationshipAsync(doctorUserId, patientUserId))
            throw new UnauthorizedAccessException("This patient has no appointments with you");
    }
}
