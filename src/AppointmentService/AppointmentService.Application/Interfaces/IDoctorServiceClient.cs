namespace AppointmentService.Application.Interfaces;

public record DoctorInfo(Guid UserId, string Name, string Email, string Specialty);

public record WorkingWindow(TimeOnly Start, TimeOnly End);

// HTTP client to the Doctor Service. Calls are made on behalf of the current user
// (their JWT is forwarded), so the Doctor Service applies its own authorization.
public interface IDoctorServiceClient
{
    // Null when the doctor does not exist
    Task<DoctorInfo?> GetByUserIdAsync(Guid doctorUserId, CancellationToken ct = default);

    // Working windows on a date (weekly hours minus time off). Empty when not working.
    Task<List<WorkingWindow>> GetWorkingWindowsAsync(Guid doctorUserId, DateOnly date, CancellationToken ct = default);
}
