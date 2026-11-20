using System.Net;
using System.Net.Http.Json;
using AppointmentService.Application.Interfaces;

namespace AppointmentService.Infrastructure.Services;

public class DoctorServiceClient : IDoctorServiceClient
{
    private readonly HttpClient _http;

    public DoctorServiceClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<DoctorInfo?> GetByUserIdAsync(Guid doctorUserId, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync($"api/doctors/by-user/{doctorUserId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var doctor = await response.Content.ReadFromJsonAsync<DoctorDto>(cancellationToken: ct);
        return doctor is null ? null : new DoctorInfo(doctor.UserId, doctor.Name, doctor.Email, doctor.Specialty);
    }

    public async Task<List<WorkingWindow>> GetWorkingWindowsAsync(Guid doctorUserId, DateOnly date, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync(
            $"api/doctors/by-user/{doctorUserId}/availability?date={date:yyyy-MM-dd}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new KeyNotFoundException("Doctor not found");

        response.EnsureSuccessStatusCode();

        var availability = await response.Content.ReadFromJsonAsync<AvailabilityDto>(cancellationToken: ct);
        return availability?.Windows.Select(w => new WorkingWindow(w.Start, w.End)).ToList() ?? new();
    }

    // Shapes of the Doctor Service responses (only the fields we use)
    private sealed record DoctorDto(Guid UserId, string Name, string Email, string Specialty);
    private sealed record WindowDto(TimeOnly Start, TimeOnly End);
    private sealed record AvailabilityDto(List<WindowDto> Windows);
}
