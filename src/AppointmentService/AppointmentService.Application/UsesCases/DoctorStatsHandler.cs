using AppointmentService.Application.DTOs;
using AppointmentService.Application.Interfaces;
using AppointmentService.Domain.Entities;

namespace AppointmentService.Application.UsesCases.DoctorStats;

// Care statistics for the doctor dashboard (no revenue, no durations)
public class DoctorStatsHandler
{
    private readonly IAppointmentRepository _repository;

    public DoctorStatsHandler(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<DoctorStatsResponse> Handle(Guid doctorUserId, DateTime? from, DateTime? to)
    {
        var all = await _repository.GetByDoctorIdAsync(doctorUserId);
        // Appointment dates are calendar dates (wall clock), so compare with the local date
        var today = DateTime.Now.Date;

        // Optional range filter for the totals
        var inRange = all
            .Where(a => from is null || a.appointmentDate.Date >= from.Value.Date)
            .Where(a => to is null || a.appointmentDate.Date <= to.Value.Date)
            .ToList();

        var byStatus = Enum.GetValues<AppointmentStatus>()
            .ToDictionary(s => s.ToString(), s => inRange.Count(a => a.status == s));

        var cancelled = byStatus[nameof(AppointmentStatus.Cancelled)];

        // Last six calendar months, oldest first, always from the full history
        var firstMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-5);
        var months = Enumerable.Range(0, 6)
            .Select(i => firstMonth.AddMonths(i))
            .Select(m => new MonthlyCountResponse
            {
                month = m.ToString("yyyy-MM"),
                completed = all.Count(a => a.status == AppointmentStatus.Completed
                    && a.appointmentDate.Year == m.Year && a.appointmentDate.Month == m.Month),
                cancelled = all.Count(a => a.status == AppointmentStatus.Cancelled
                    && a.appointmentDate.Year == m.Year && a.appointmentDate.Month == m.Month)
            })
            .ToList();

        return new DoctorStatsResponse
        {
            totalAppointments = inRange.Count,
            byStatus = byStatus,
            patientsAttended = inRange
                .Where(a => a.status == AppointmentStatus.Completed)
                .Select(a => a.patientId)
                .Distinct()
                .Count(),
            upcoming = all.Count(a => a.IsActive && a.appointmentDate.Date >= today),
            today = all.Count(a => a.IsActive && a.appointmentDate.Date == today),
            pendingConfirmation = all.Count(a => a.status == AppointmentStatus.Scheduled && a.appointmentDate.Date >= today),
            cancellationRate = inRange.Count == 0 ? 0 : Math.Round((double)cancelled / inRange.Count, 4),
            lastSixMonths = months
        };
    }
}
