using Microsoft.AspNetCore.Http;

namespace AppointmentService.Infrastructure.Services;

// Copies the incoming request's Authorization header to outgoing service-to-service calls,
// so the called service authorizes the request as the same user.
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
