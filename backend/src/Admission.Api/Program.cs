using Admission.Api.Configuration;
using Admission.Api.Data;
using Admission.Api.Endpoints;
using Admission.Api.Errors;
using Microsoft.EntityFrameworkCore;

DotEnv.LoadIfDevelopment();

var builder = WebApplication.CreateBuilder(args);

// Port: ASPNETCORE_URLS wins; otherwise PORT (Render sets it); otherwise 8080.
if (string.IsNullOrWhiteSpace(builder.Configuration["ASPNETCORE_URLS"]))
{
    var port = builder.Configuration["PORT"];
    builder.WebHost.UseUrls($"http://+:{(string.IsNullOrWhiteSpace(port) ? "8080" : port)}");
}

var settings = AppSettings.Load(builder.Configuration);
builder.Services.AddSingleton(settings);

builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(settings.ConnectionString));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(settings.CorsOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();

// Unmatched routes and other empty-bodied error statuses also get a JSON body.
app.UseStatusCodePages(async ctx =>
{
    var status = ctx.HttpContext.Response.StatusCode;
    var body = status switch
    {
        StatusCodes.Status404NotFound => new ErrorResponse(ErrorCodes.NotFound, "That resource was not found."),
        StatusCodes.Status405MethodNotAllowed => new ErrorResponse(ErrorCodes.MethodNotAllowed, "That method is not allowed here."),
        _ => new ErrorResponse(ErrorCodes.InvalidInput, "The request is not valid."),
    };
    await ctx.HttpContext.Response.WriteAsJsonAsync(body);
});

app.UseCors();

app.MapHealthEndpoints();

await DatabaseInitializer.MigrateAsync(app.Services);

app.Run();

public partial class Program { }
