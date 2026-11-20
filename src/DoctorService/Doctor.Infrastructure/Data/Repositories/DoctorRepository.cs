using DoctorService.Application.Interfaces;
using DoctorService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoctorService.Infrastructure.Data.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {

        private readonly DoctorDbContext _context;

        public DoctorRepository(DoctorDbContext context)
        {
            _context = context;
        }

        public async Task<Doctor?> GetByIdAsync (Guid id)
        {
            return await _context.Doctors
                .Include(d => d.Schedules)
                .Include(d => d.TimeOffs)
                .FirstOrDefaultAsync (d => d.Id == id);
        }

        public async Task<Doctor?> GetByUserIdAsync (Guid userId)
        {
            return await _context.Doctors
                .Include(d => d.Schedules)
                .Include(d => d.TimeOffs)
                .FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public async Task<List<Doctor>> GetAllAsync()
        {
            return await _context.Doctors
                .Include(d => d.Schedules)
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        // Case-insensitive filter by specialty and/or name
        public async Task<List<Doctor>> SearchAsync(string? specialty, string? search)
        {
            var query = _context.Doctors
                .Include(d => d.Schedules)
                .Where(d => d.IsActive);

            if (!string.IsNullOrWhiteSpace(specialty))
            {
                var s = specialty.Trim().ToLower();
                query = query.Where(d => d.Specialty.ToLower() == s);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim()}%";
                query = query.Where(d =>
                    EF.Functions.ILike(d.Name, pattern) || EF.Functions.ILike(d.Specialty, pattern));
            }

            return await query.OrderBy(d => d.Name).ToListAsync();
        }


        public async Task<List<Doctor>> GetBySpecialtyAsync (string specialty)
        {
            return await SearchAsync(specialty, null);
        }

        public async Task<List<(string Specialty, int DoctorCount)>> GetSpecialtiesAsync()
        {
            var rows = await _context.Doctors
                .Where(d => d.IsActive)
                .GroupBy(d => d.Specialty)
                .Select(g => new { Specialty = g.Key, Count = g.Count() })
                .OrderBy(x => x.Specialty)
                .ToListAsync();

            return rows.Select(r => (r.Specialty, r.Count)).ToList();
        }

        public async Task<bool> ExistsAsync (Guid userId)
        {
            return await _context.Doctors
                .AnyAsync(d => d.UserId == userId && d.IsActive);
        }

        public async Task AddAsync (Doctor doctor)
        {
            await _context.Doctors.AddAsync(doctor);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
