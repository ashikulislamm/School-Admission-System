using Admission.Api.Configuration;
using Microsoft.Extensions.Configuration;

namespace Admission.Tests;

public class AppSettingsTests
{
    private static IConfiguration Config(params (string Key, string? Value)[] items) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(items.Select(i => new KeyValuePair<string, string?>(i.Key, i.Value)))
            .Build();

    [Fact]
    public void Defaults_are_applied()
    {
        var s = AppSettings.Load(Config(("ConnectionStrings:Default", "Host=db;Database=x;Username=a;Password=b")));

        Assert.Equal(600, s.HoldDurationSeconds);
        Assert.Contains("http://localhost:3000", s.CorsOrigins);
    }

    [Fact]
    public void Hold_duration_and_cors_are_read_from_config()
    {
        var s = AppSettings.Load(Config(
            ("ConnectionStrings:Default", "Host=db;Database=x;Username=a;Password=b"),
            ("HOLD_DURATION_SECONDS", "20"),
            ("CORS_ORIGINS", "https://app.vercel.app/, http://localhost:3000")));

        Assert.Equal(20, s.HoldDurationSeconds);
        Assert.Equal(["https://app.vercel.app", "http://localhost:3000"], s.CorsOrigins);
    }

    [Fact]
    public void Database_url_is_used_when_connection_string_missing()
    {
        var s = AppSettings.Load(Config(("DATABASE_URL", "postgres://u:p@host.example.com/app")));

        Assert.Contains("host.example.com", s.ConnectionString);
    }

    [Fact]
    public void Missing_connection_fails_fast()
    {
        Assert.Throws<InvalidOperationException>(() => AppSettings.Load(Config()));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("abc")]
    public void Invalid_hold_duration_fails_fast(string value)
    {
        Assert.Throws<InvalidOperationException>(() => AppSettings.Load(Config(
            ("ConnectionStrings:Default", "Host=db;Database=x;Username=a;Password=b"),
            ("HOLD_DURATION_SECONDS", value))));
    }
}
