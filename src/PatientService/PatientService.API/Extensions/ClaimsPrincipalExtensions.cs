using System.Security.Claims;

namespace PatientService.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("sub")?.Value
                    ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (value is null || !Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException("Invalid token");

        return userId;
    }

    public static string GetEmail(this ClaimsPrincipal user) =>
        user.FindFirst("email")?.Value ?? user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    public static string GetName(this ClaimsPrincipal user) =>
        user.FindFirst("name")?.Value ?? user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

    public static string GetRole(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("role")?.Value ?? string.Empty;
}
