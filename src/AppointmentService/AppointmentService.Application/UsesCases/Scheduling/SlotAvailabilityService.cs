using AppointmentService.Application.Interfaces;

namespace AppointmentService.Application.UsesCases.Scheduling;

public class SchedulingOptions
{
    public int SlotMinutes { get; set; } = 30;
}

// Builds bookable slots from the doctor's working windows (Doctor Service)
// minus the slots already taken (this service) and times already in the past.
public class SlotAvailabilityService
{
    private readonly IDoctorServiceClient _doctorClient;
    private readonly IAppointmentRepository _repository;
    private readonly SchedulingOptions _options;

    public SlotAvailabilityService(IDoctorServiceClient doctorClient, IAppointmentRepository repository, SchedulingOptions options)
    {
        _doctorClient = doctorClient;
        _repository = repository;
        _options = options;
    }

    public int SlotMinutes => _options.SlotMinutes;

    public async Task<List<TimeOnly>> GetAvailableSlotsAsync(Guid doctorUserId, DateOnly date, Guid? excludeAppointmentId = null)
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        if (date < today)
            return new List<TimeOnly>();

        var windows = await _doctorClient.GetWorkingWindowsAsync(doctorUserId, date);

        var taken = (await _repository.GetByDoctorAndDateAsync(doctorUserId, ToStorageDate(date)))
            .Where(a => excludeAppointmentId is null || a.Id != excludeAppointmentId)
            .Select(a => a.appointmentTime)
            .ToHashSet();

        var slots = new List<TimeOnly>();
        foreach (var window in windows)
        {
            var slot = window.Start;
            while (true)
            {
                var slotEnd = slot.AddMinutes(_options.SlotMinutes);

                // TimeOnly wraps at midnight: slotEnd <= slot means we passed 00:00
                if (slotEnd <= slot || slotEnd > window.End)
                    break;

                var inThePast = date == today && slot <= TimeOnly.FromDateTime(now);
                if (!taken.Contains(slot) && !inThePast)
                    slots.Add(slot);

                slot = slotEnd;
            }
        }

        return slots.Distinct().OrderBy(s => s).ToList();
    }

    public async Task EnsureSlotIsBookableAsync(Guid doctorUserId, DateTime date, TimeOnly time, Guid? excludeAppointmentId = null)
    {
        var slots = await GetAvailableSlotsAsync(doctorUserId, DateOnly.FromDateTime(date), excludeAppointmentId);
        if (!slots.Contains(time))
            throw new InvalidOperationException("This time slot is not available. Please choose another one.");
    }

    // Appointments store the date as midnight UTC
    public static DateTime ToStorageDate(DateOnly date) =>
        DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
}
