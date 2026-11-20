using Microsoft.AspNetCore.Diagnostics;

namespace PatientService.API.Middleware;

// Maps exceptions thrown by handlers to HTTP responses with a { message } body:
//   KeyNotFoundException        -> 404
//   UnauthorizedAccessException -> 403 (the user is authenticated but not the owner)
//   InvalidOperationException / ArgumentException -> 400 (business rule violated)
//   anything else               -> 500 (details are logged, not returned)
public class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, message) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, exception.Message),
            InvalidOperationException or ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new { message }, cancellationToken);
        return true;
    }
}
