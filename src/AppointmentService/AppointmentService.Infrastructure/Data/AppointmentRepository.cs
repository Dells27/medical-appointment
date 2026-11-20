using AppointmentService.Domain.Entities;
using AppointmentService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Infrastructure.Data.Repositories;

public class AppointmentRepository: IAppointmentRepository
{
    private readonly AppointmentDbContext _context;

    public AppointmentRepository(AppointmentDbContext dbContext)
    {
        _context = dbContext;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id)
    {
        return await _context.Appointments.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Appointment>> GetByPatientIdAsync(Guid patientId)
    {
        return await _context.Appointments
            .Where(a => a.patientId == patientId)
            .OrderByDescending(a => a.appointmentDate)
            .ThenByDescending(a => a.appointmentTime)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetByDoctorIdAsync (Guid doctorId)
    {
        return await _context.Appointments
            .Where(x => x.doctorId == doctorId)
            .OrderByDescending(x => x.appointmentDate)
            .ThenByDescending(x => x.appointmentTime)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetByDoctorAndPatientAsync(Guid doctorId, Guid patientId)
    {
        return await _context.Appointments
            .Where(x => x.doctorId == doctorId && x.patientId == patientId)
            .OrderByDescending(x => x.appointmentDate)
            .ThenByDescending(x => x.appointmentTime)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetByDoctorAndDateAsync (Guid doctorId, DateTime date)
    {
        return await _context.Appointments
            .Where(x => x.doctorId == doctorId
                && x.appointmentDate == date
                && x.status != AppointmentStatus.Cancelled)
            .ToListAsync();
    }

    // Cancelled appointments free the slot (matches the filtered unique index)
    public async Task<bool> IsSlotTakenAsync(Guid doctorId, DateTime date, TimeOnly time, Guid? excludeAppointmentId = null)
    {
        return await _context.Appointments.AnyAsync(
            x => x.doctorId == doctorId
            && x.appointmentDate == date
            && x.appointmentTime == time
            && x.status != AppointmentStatus.Cancelled
            && (excludeAppointmentId == null || x.Id != excludeAppointmentId));
    }

    // A doctor may access a patient's clinical data only if they share a non-cancelled appointment
    public async Task<bool> HasRelationshipAsync(Guid doctorId, Guid patientId)
    {
        return await _context.Appointments.AnyAsync(
            x => x.doctorId == doctorId
            && x.patientId == patientId
            && x.status != AppointmentStatus.Cancelled);
    }

    public async Task AddAsync (Appointment appointment)
    {
         await _context.AddAsync(appointment);
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
