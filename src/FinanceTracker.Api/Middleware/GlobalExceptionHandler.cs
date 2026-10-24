using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
        Exception exception, CancellationToken ct)
    {
        var (status, title, detail) = exception switch
        {
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "User is not authenticated."),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                exception.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Server Error",
                "An unexpected error occurred on the server.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
        else
            logger.LogWarning(exception, "Request failed with status {Status}: {Message}",
                status, exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Detail = detail
        };

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, ct);

        return true;
    }
}
