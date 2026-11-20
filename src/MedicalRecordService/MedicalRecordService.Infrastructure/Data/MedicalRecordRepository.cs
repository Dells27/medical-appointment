using MedicalRecordService.Application.Interfaces;
using MedicalRecordService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicalRecordService.Infrastructure.Data;

public class MedicalRecordRepository : IMedicalRecordRepository
{
    private readonly MedicalRecordDbContext _context;

    public MedicalRecordRepository(MedicalRecordDbContext context)
    {
        _context = context;
    }

    public async Task<MedicalRecord?> GetByIdAsync(Guid id)
    {
        return await _context.MedicalRecords.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<MedicalRecord?> GetByAppointmentIdAsync(Guid appointmentId)
    {
        return await _context.MedicalRecords.FirstOrDefaultAsync(m => m.appointmentId == appointmentId);
    }

    public async Task<List<MedicalRecord>> GetByPatientIdAsync(Guid patientId)
    {
        return await _context.MedicalRecords
            .Where(m => m.patientId == patientId)
            .OrderByDescending(m => m.appointmentDate)
            .ToListAsync();
    }

    public async Task<List<MedicalRecord>> GetByDoctorIdAsync(Guid doctorId)
    {
        return await _context.MedicalRecords
            .Where(m => m.doctorId == doctorId)
            .OrderByDescending(m => m.appointmentDate)
            .ToListAsync();
    }

    public async Task AddAsync(MedicalRecord record)
    {
        await _context.MedicalRecords.AddAsync(record);
    }

    public async Task<MedicalBackground?> GetBackgroundAsync(Guid patientId)
    {
        return await _context.MedicalBackgrounds.FirstOrDefaultAsync(b => b.patientId == patientId);
    }

    public async Task AddBackgroundAsync(MedicalBackground background)
    {
        await _context.MedicalBackgrounds.AddAsync(background);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
