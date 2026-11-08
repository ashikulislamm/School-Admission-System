using Admission.Api.Data;
using Admission.Api.Errors;

namespace Admission.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", async (AppDbContext db, CancellationToken ct) =>
        {
            var canConnect = await db.Database.CanConnectAsync(ct);
            return canConnect
                ? Results.Ok(new { status = "ok" })
                : Results.Json(
                    new ErrorResponse(ErrorCodes.Unavailable, "The database is not reachable."),
                    statusCode: StatusCodes.Status503ServiceUnavailable);
        });

        return app;
    }
}
