using AppointmentService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AppointmentService.Application.UsesCases.Scheduling;

// Fetches the doctor's name and email for event payloads. A failure here must not
// undo an appointment that was already saved, so it returns null and logs instead.
public class DoctorLookup
{
    private readonly IDoctorServiceClient _client;
    private readonly ILogger<DoctorLookup> _logger;

    public DoctorLookup(IDoctorServiceClient client, ILogger<DoctorLookup> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<DoctorInfo?> TryGetAsync(Guid doctorUserId)
    {
        try
        {
            return await _client.GetByUserIdAsync(doctorUserId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load doctor {DoctorId} for the notification payload", doctorUserId);
            return null;
        }
    }
}
