using MedicalRecordService.Domain.Entities;

namespace MedicalRecordService.Application.Interfaces
{
    public interface IMedicalRecordRepository
    {
        Task<MedicalRecord?> GetByIdAsync(Guid Id);
        Task<MedicalRecord?> GetByAppointmentIdAsync(Guid id);
        Task<List<MedicalRecord>> GetByPatientIdAsync(Guid patientId);
        Task<List<MedicalRecord>> GetByDoctorIdAsync(Guid doctorId);
        Task AddAsync(MedicalRecord record);

        Task<MedicalBackground?> GetBackgroundAsync(Guid patientId);
        Task AddBackgroundAsync(MedicalBackground background);

        Task SaveChangesAsync();
    }

    // Asks the Appointment Service whether the current doctor (JWT forwarded)
    // has a non-cancelled appointment with the patient
    public interface IAppointmentServiceClient
    {
        Task<bool> HasRelationshipAsync(Guid patientUserId, CancellationToken ct = default);
    }
}
