namespace Admission.Api.Configuration;

/// <summary>Validated application settings. Loading fails fast with a clear message.</summary>
public sealed record AppSettings(
    string ConnectionString,
    int HoldDurationSeconds,
    string[] CorsOrigins,
    bool SweeperEnabled = true)
{
    public const int DefaultHoldDurationSeconds = 600;
    private const string DefaultCorsOrigins = "http://localhost:3000,http://localhost:5173";

    public static AppSettings Load(IConfiguration config)
    {
        var rawConnection = config["ConnectionStrings:Default"];
        if (string.IsNullOrWhiteSpace(rawConnection))
        {
            rawConnection = config["DATABASE_URL"];
        }

        if (string.IsNullOrWhiteSpace(rawConnection))
        {
            throw new InvalidOperationException(
                "Database connection is not configured. Set ConnectionStrings__Default " +
                "(or DATABASE_URL) as an environment variable.");
        }

        var holdSeconds = DefaultHoldDurationSeconds;
        var rawHold = config["HOLD_DURATION_SECONDS"];
        if (!string.IsNullOrWhiteSpace(rawHold))
        {
            if (!int.TryParse(rawHold, out holdSeconds) || holdSeconds <= 0)
            {
                throw new InvalidOperationException(
                    $"HOLD_DURATION_SECONDS must be a positive integer, got '{rawHold}'.");
            }
        }

        var origins = (config["CORS_ORIGINS"] ?? DefaultCorsOrigins)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(o => o.TrimEnd('/'))
            .ToArray();

        return new AppSettings(
            ConnectionStringHelper.Normalize(rawConnection),
            holdSeconds,
            origins,
            !string.Equals(config["SWEEPER_ENABLED"], "false", StringComparison.OrdinalIgnoreCase));
    }
}
