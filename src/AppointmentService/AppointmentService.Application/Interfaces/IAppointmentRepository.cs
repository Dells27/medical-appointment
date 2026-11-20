using AppointmentService.Domain.Entities;

namespace AppointmentService.Application.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<Appointment?> GetByIdAsync(Guid id);
        Task<List<Appointment>> GetByPatientIdAsync(Guid patientId);
        Task<List<Appointment>> GetByDoctorIdAsync(Guid doctorId);
        Task<List<Appointment>> GetByDoctorAndPatientAsync(Guid doctorId, Guid patientId);
        Task<List<Appointment>> GetByDoctorAndDateAsync(Guid doctorId, DateTime date);
        Task<bool> IsSlotTakenAsync(Guid doctorId, DateTime date, TimeOnly time, Guid? excludeAppointmentId = null);
        Task<bool> HasRelationshipAsync(Guid doctorId, Guid patientId);
        Task AddAsync(Appointment appointment);
        Task SaveChangesAsync();
    }
}
