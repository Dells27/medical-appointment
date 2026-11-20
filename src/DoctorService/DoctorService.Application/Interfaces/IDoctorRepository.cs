using DoctorService.Domain.Entities;

namespace DoctorService.Application.Interfaces
{
    public interface IDoctorRepository
    {
        Task<Doctor?> GetByIdAsync(Guid id);
        Task<Doctor?> GetByUserIdAsync(Guid userId);
        Task<List<Doctor>> GetAllAsync();
        Task<List<Doctor>> SearchAsync(string? specialty, string? search);
        Task<List<Doctor>> GetBySpecialtyAsync(string specialty);
        Task<List<(string Specialty, int DoctorCount)>> GetSpecialtiesAsync();
        Task<bool> ExistsAsync(Guid userId);
        Task AddAsync(Doctor doctor);
        Task SaveChangesAsync();
    }
}
