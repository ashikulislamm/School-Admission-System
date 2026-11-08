using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Npgsql;

namespace Admission.Api.Errors;

/// <summary>
/// Turns every exception into a JSON ErrorResponse. Input problems become 4xx;
/// only truly unexpected failures become a generic 500 (details are logged, never returned).
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, body) = Map(exception);

        if (status >= 500)
        {
            logger.LogError(exception, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
        }

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(body, cancellationToken);
        return true;
    }

    private static (int Status, ErrorResponse Body) Map(Exception exception) => exception switch
    {
        ApiException api => (api.StatusCode, new ErrorResponse(api.Code, api.Message)),

        BadHttpRequestException or JsonException or FormatException => (
            StatusCodes.Status400BadRequest,
            new ErrorResponse(ErrorCodes.InvalidInput, "The request is not valid. Check the values and try again.")),

        PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } => (
            StatusCodes.Status409Conflict,
            new ErrorResponse(ErrorCodes.AlreadyApplied, "You already have an application for this class.")),

        PostgresException { SqlState: PostgresErrorCodes.CheckViolation } => (
            StatusCodes.Status400BadRequest,
            new ErrorResponse(ErrorCodes.InvalidInput, "The request is not valid. Check the values and try again.")),

        NpgsqlException or TimeoutException => (
            StatusCodes.Status503ServiceUnavailable,
            new ErrorResponse(ErrorCodes.Unavailable, "The service is temporarily unavailable. Try again shortly.")),

        _ => (
            StatusCodes.Status500InternalServerError,
            new ErrorResponse(ErrorCodes.Internal, "Something went wrong on our side. Try again.")),
    };
}
