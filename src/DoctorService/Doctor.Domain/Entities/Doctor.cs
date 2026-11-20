namespace DoctorService.Domain.Entities;

public class Doctor
{

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Specialty { get; private set; } = null!;
    public string LicenseNumber { get; private set; } = null!;
    public string? PhotoUrl { get; private set; }
    public string? Bio { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<AvailabilitySchedule> _schedules = new();
    public IReadOnlyCollection<AvailabilitySchedule> Schedules => _schedules.AsReadOnly();

    private readonly List<TimeOff> _timeOffs = new();
    public IReadOnlyCollection<TimeOff> TimeOffs => _timeOffs.AsReadOnly();

    private Doctor() { }

    public static Doctor Create(Guid userId, string name, string email, string specialty, string licenseNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");
        if (string.IsNullOrWhiteSpace(specialty))
            throw new ArgumentException("Specialty is required");
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("License number is required");

        return new Doctor {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Specialty = specialty.Trim(),
            LicenseNumber = licenseNumber.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
    }


    public void UpdateProfile (string name, string specialty, string? bio, string? photoURL)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");
        if (string.IsNullOrWhiteSpace(specialty))
            throw new ArgumentException("Specialty is required");

        Name = name.Trim();
        Specialty = specialty.Trim();
        Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        PhotoUrl = string.IsNullOrWhiteSpace(photoURL) ? null : photoURL.Trim();
    }

    // Replaces the whole weekly schedule. Blocks on the same day cannot overlap.
    public void ReplaceSchedules(IEnumerable<(DayOfWeek Day, TimeOnly Start, TimeOnly End)> blocks)
    {
        var newBlocks = blocks
            .Select(b => AvailabilitySchedule.Create(Id, b.Day, b.Start, b.End))
            .ToList();

        foreach (var day in newBlocks.GroupBy(b => b.DayOfWeek))
        {
            var ordered = day.OrderBy(b => b.StartTime).ToList();
            for (var i = 1; i < ordered.Count; i++)
            {
                if (ordered[i].StartTime < ordered[i - 1].EndTime)
                    throw new ArgumentException($"Working hours overlap on {day.Key}");
            }
        }

        _schedules.Clear();
        _schedules.AddRange(newBlocks);
    }

    public TimeOff AddTimeOff(TimeOffType type, DateOnly startDate, DateOnly endDate,
        TimeOnly? startTime, TimeOnly? endTime, string? reason)
    {
        var timeOff = TimeOff.Create(Id, type, startDate, endDate, startTime, endTime, reason);
        _timeOffs.Add(timeOff);
        return timeOff;
    }

    public void RemoveTimeOff(Guid timeOffId)
    {
        var timeOff = _timeOffs.FirstOrDefault(t => t.Id == timeOffId)
            ?? throw new KeyNotFoundException("Time off not found");
        _timeOffs.Remove(timeOff);
    }

    // Working windows for a given date: weekly hours minus any time off on that date
    public List<TimeWindow> GetWorkingWindows(DateOnly date)
    {
        var windows = _schedules
            .Where(s => s.IsActive && s.DayOfWeek == date.DayOfWeek)
            .OrderBy(s => s.StartTime)
            .Select(s => new TimeWindow(s.StartTime, s.EndTime))
            .ToList();

        foreach (var timeOff in _timeOffs.Where(t => t.Covers(date)))
        {
            if (timeOff.IsFullDay)
                return new List<TimeWindow>();

            windows = windows
                .SelectMany(w => w.Subtract(timeOff.StartTime!.Value, timeOff.EndTime!.Value))
                .ToList();
        }

        return windows;
    }
}
