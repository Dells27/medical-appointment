using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using PatientService.Application.Interfaces;

namespace PatientService.Infrastructure.Services;

// Copies the incoming Authorization header to service-to-service calls,
// so the Appointment Service authorizes the request as the same doctor.
public class ForwardAuthorizationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ForwardAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authorization = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        if (!string.IsNullOrEmpty(authorization) && request.Headers.Authorization is null)
            request.Headers.TryAddWithoutValidation("Authorization", authorization);

        return base.SendAsync(request, cancellationToken);
    }
}

public class AppointmentServiceClient : IAppointmentServiceClient
{
    private readonly HttpClient _http;

    public AppointmentServiceClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> HasRelationshipAsync(Guid patientUserId, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync($"api/appointments/relationship/{patientUserId}", ct);

        // 401/403 means the caller is not a doctor — no access
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            return false;

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<RelationshipDto>(cancellationToken: ct);
        return body?.HasRelationship ?? false;
    }

    private sealed record RelationshipDto(bool HasRelationship);
}
