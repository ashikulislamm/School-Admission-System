using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Admission.Tests;

/// <summary>One real Postgres container shared by every integration test (never mocked).</summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public Api CreateApi(int holdSeconds = 600, bool sweeper = false) =>
        new(new AppFactory(ConnectionString, holdSeconds, sweeper));
}

[CollectionDefinition(Name)]
public sealed class DbCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "db";
}

public sealed class AppFactory(string connectionString, int holdSeconds, bool sweeper) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", connectionString);
        builder.UseSetting("HOLD_DURATION_SECONDS", holdSeconds.ToString());
        builder.UseSetting("SWEEPER_ENABLED", sweeper ? "true" : "false");
    }
}

public sealed record ClassDto(string Id, string Name, int TotalSeats, int HeldSeats, int ConfirmedSeats, int AvailableSeats);

public sealed record HoldDto(string HoldId, string ClassId, string StudentId, string Status, DateTime ExpiresAt);

public sealed record ErrorDto(string Code, string Message);

/// <summary>Thin typed wrapper over the in-memory test server.</summary>
public sealed class Api(AppFactory factory) : IDisposable
{
    private readonly HttpClient _http = factory.CreateClient();

    public static string NewStudent() => Random.Shared.NextInt64(10_000_000_000_000_000, 100_000_000_000_000_000).ToString();

    public async Task<ClassDto> CreateClassAsync(int seats, string name = "Class One")
    {
        var res = await _http.PostAsJsonAsync("/classes", new { name, totalSeats = seats });
        Assert.Equal(System.Net.HttpStatusCode.Created, res.StatusCode);
        return (await res.Content.ReadFromJsonAsync<ClassDto>())!;
    }

    public Task<HttpResponseMessage> ApplyAsync(string classId, string? student) =>
        Send(HttpMethod.Post, $"/classes/{classId}/holds", student);

    public async Task<HoldDto> ApplyOkAsync(string classId, string student)
    {
        var res = await ApplyAsync(classId, student);
        Assert.Equal(System.Net.HttpStatusCode.Created, res.StatusCode);
        return (await res.Content.ReadFromJsonAsync<HoldDto>())!;
    }

    public Task<HttpResponseMessage> ConfirmAsync(string holdId, string? student) =>
        Send(HttpMethod.Post, $"/holds/{holdId}/confirm", student);

    public Task<HttpResponseMessage> CancelAsync(string holdId, string? student) =>
        Send(HttpMethod.Delete, $"/holds/{holdId}", student);

    public Task<HttpResponseMessage> GetHoldAsync(string holdId) => Send(HttpMethod.Get, $"/holds/{holdId}", null);

    public Task<HttpResponseMessage> MyHoldsAsync(string? student) => Send(HttpMethod.Get, "/me/holds", student);

    public async Task<ClassDto> GetClassAsync(string id) =>
        (await _http.GetFromJsonAsync<ClassDto>($"/classes/{id}"))!;

    public Task<HttpResponseMessage> RawAsync(HttpMethod method, string url, string? student = null, HttpContent? body = null) =>
        Send(method, url, student, body);

    public async Task<string> StatusOfAsync(string holdId) =>
        (await (await GetHoldAsync(holdId)).Content.ReadFromJsonAsync<HoldDto>())!.Status;

    private Task<HttpResponseMessage> Send(HttpMethod method, string url, string? student, HttpContent? body = null)
    {
        var req = new HttpRequestMessage(method, url) { Content = body };
        if (student is not null)
        {
            req.Headers.Add("X-Student-Id", student);
        }

        return _http.SendAsync(req);
    }

    public void Dispose()
    {
        _http.Dispose();
        factory.Dispose();
    }
}

public static class ResponseExtensions
{
    public static async Task<ErrorDto> ErrorAsync(this HttpResponseMessage res) =>
        (await res.Content.ReadFromJsonAsync<ErrorDto>())!;
}
