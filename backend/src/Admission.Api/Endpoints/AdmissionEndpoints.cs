using Admission.Api.Models;
using Admission.Api.Services;

namespace Admission.Api.Endpoints;

public static class AdmissionEndpoints
{
    public static IEndpointRouteBuilder MapAdmissionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/classes", async (CreateClassRequest? body, ClassService classes, CancellationToken ct) =>
        {
            var (name, seats) = Validation.ValidateNewClass(body);
            var created = await classes.CreateAsync(name, seats, ct);
            return Results.Json(created, statusCode: StatusCodes.Status201Created);
        });

        app.MapGet("/classes", async (ClassService classes, CancellationToken ct) =>
            Results.Ok(await classes.ListAsync(ct)));

        app.MapGet("/classes/{id}", async (string id, ClassService classes, CancellationToken ct) =>
            Results.Ok(await classes.GetAsync(id, ct)));

        app.MapPost("/classes/{id}/holds", async (string id, HttpRequest req, HoldService holds, CancellationToken ct) =>
        {
            var studentId = Validation.RequireStudentId(req);
            var hold = await holds.ApplyAsync(id, studentId, ct);
            return Results.Json(hold, statusCode: StatusCodes.Status201Created);
        });

        app.MapGet("/holds/{id}", async (string id, HoldService holds, CancellationToken ct) =>
            Results.Ok(await holds.GetAsync(id, ct)));

        app.MapPost("/holds/{id}/confirm", async (string id, HttpRequest req, HoldService holds, CancellationToken ct) =>
        {
            var studentId = Validation.RequireStudentId(req);
            return Results.Ok(await holds.ConfirmAsync(id, studentId, ct));
        });

        app.MapDelete("/holds/{id}", async (string id, HttpRequest req, HoldService holds, CancellationToken ct) =>
        {
            var studentId = Validation.RequireStudentId(req);
            await holds.CancelAsync(id, studentId, ct);
            return Results.NoContent();
        });

        app.MapGet("/me/holds", async (HttpRequest req, HoldService holds, CancellationToken ct) =>
        {
            var studentId = Validation.RequireStudentId(req);
            return Results.Ok(await holds.ListMineAsync(studentId, ct));
        });

        return app;
    }
}
