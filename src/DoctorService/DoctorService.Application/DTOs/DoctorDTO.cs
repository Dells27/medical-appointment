using System.Text.Json.Serialization;

namespace DoctorService.Application.DTOs;

// Create the doctor profile
public class CreateDoctorRequest
{
    [JsonIgnore] // set from the JWT, never from the client
    public Guid userId { get; set; }
    public string name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialty {  get; set; } = string.Empty;
    public string LicenseNumber {  get; set; } = string.Empty;
}

// Update the professional profile
public class UpdateDoctorRequest
{
    public string name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string? Bio {  get; set; }
    public string? PhotoUrl { get; set; }
}

// One block of weekly working hours
public class AddScheduleRequest
{
    public DayOfWeek DayOfWeek {  get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

// Replaces the whole weekly schedule
public class ReplaceSchedulesRequest
{
    public List<AddScheduleRequest> Schedules { get; set; } = new();
}

public class CreateTimeOffRequest
{
    public string Type { get; set; } = "DayOff"; // Vacation | DayOff | BlockedHours
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Reason { get; set; }
}

public class DoctorResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string? PhotoUrl {get; set;}
    public string? Bio { get; set; }
    public List<ScheduleResponse> Schedules { get; set; } = new();
}

// What anonymous visitors see: no email or license number
public class PublicDoctorResponse
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string? Bio { get; set; }
    public List<ScheduleResponse> Schedules { get; set; } = new();
}

public class SpecialtyResponse
{
    public string Name { get; set; } = string.Empty;
    public int DoctorCount { get; set; }
}

public class ScheduleResponse
{
    public Guid Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

public class TimeOffResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Reason { get; set; }
}

public class TimeWindowResponse
{
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }
}

// Working windows of a doctor on a specific date (weekly hours minus time off)
public class AvailabilityResponse
{
    public Guid DoctorUserId { get; set; }
    public DateOnly Date { get; set; }
    public List<TimeWindowResponse> Windows { get; set; } = new();
}
