using DoctorService.Application.DTOs;
using DoctorService.Application.Interfaces;
using DoctorService.Application.UseCases.CreateDoctor;
using DoctorService.Domain.Entities;

namespace DoctorService.Application.UseCases.Profile;

// Everything the authenticated doctor manages about themselves:
// professional profile, weekly hours and time off
public class DoctorProfileHandler
{
    private readonly IDoctorRepository _repository;

    public DoctorProfileHandler(IDoctorRepository repository)
    {
        _repository = repository;
    }

    public async Task<DoctorResponse> GetMine(Guid userId)
    {
        var doctor = await GetDoctorOrThrow(userId);
        return CreateDoctorHanlder.MapToResponse(doctor);
    }

    public async Task<DoctorResponse> UpdateMine(Guid userId, UpdateDoctorRequest request)
    {
        var doctor = await GetDoctorOrThrow(userId);
        doctor.UpdateProfile(request.name, request.Specialty, request.Bio, request.PhotoUrl);
        await _repository.SaveChangesAsync();
        return CreateDoctorHanlder.MapToResponse(doctor);
    }

    public async Task<List<ScheduleResponse>> ReplaceSchedules(Guid userId, ReplaceSchedulesRequest request)
    {
        var doctor = await GetDoctorOrThrow(userId);
        doctor.ReplaceSchedules(request.Schedules.Select(s => (s.DayOfWeek, s.StartTime, s.EndTime)));
        await _repository.SaveChangesAsync();
        return CreateDoctorHanlder.MapSchedules(doctor);
    }

    public async Task<List<TimeOffResponse>> GetTimeOff(Guid userId)
    {
        var doctor = await GetDoctorOrThrow(userId);
        return doctor.TimeOffs
            .OrderBy(t => t.StartDate).ThenBy(t => t.StartTime)
            .Select(MapTimeOff)
            .ToList();
    }

    public async Task<TimeOffResponse> AddTimeOff(Guid userId, CreateTimeOffRequest request)
    {
        if (!Enum.TryParse<TimeOffType>(request.Type, ignoreCase: true, out var type))
            throw new ArgumentException("Invalid time off type");

        var doctor = await GetDoctorOrThrow(userId);
        var timeOff = doctor.AddTimeOff(type, request.StartDate, request.EndDate,
            request.StartTime, request.EndTime, request.Reason);

        await _repository.SaveChangesAsync();
        return MapTimeOff(timeOff);
    }

    public async Task RemoveTimeOff(Guid userId, Guid timeOffId)
    {
        var doctor = await GetDoctorOrThrow(userId);
        doctor.RemoveTimeOff(timeOffId);
        await _repository.SaveChangesAsync();
    }

    // Working windows of any doctor on a date — used by the Appointment Service to build slots
    public async Task<AvailabilityResponse> GetAvailability(Guid doctorUserId, DateOnly date)
    {
        var doctor = await _repository.GetByUserIdAsync(doctorUserId);
        if (doctor is null || !doctor.IsActive)
            throw new KeyNotFoundException("Doctor not found");

        return new AvailabilityResponse
        {
            DoctorUserId = doctorUserId,
            Date = date,
            Windows = doctor.GetWorkingWindows(date)
                .Select(w => new TimeWindowResponse { Start = w.Start, End = w.End })
                .ToList()
        };
    }

    private async Task<Doctor> GetDoctorOrThrow(Guid userId)
    {
        return await _repository.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Doctor profile not found");
    }

    private static TimeOffResponse MapTimeOff(TimeOff t) => new()
    {
        Id = t.Id,
        Type = t.Type.ToString(),
        StartDate = t.StartDate,
        EndDate = t.EndDate,
        StartTime = t.StartTime,
        EndTime = t.EndTime,
        Reason = t.Reason
    };
}
