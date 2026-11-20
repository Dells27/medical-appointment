namespace PatientService.Application.Interfaces;

// Asks the Appointment Service whether the current doctor (JWT forwarded)
// has a non-cancelled appointment with the patient
public interface IAppointmentServiceClient
{
    Task<bool> HasRelationshipAsync(Guid patientUserId, CancellationToken ct = default);
}
