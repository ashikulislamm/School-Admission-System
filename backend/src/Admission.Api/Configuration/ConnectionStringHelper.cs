using System.Data.Common;
using System.Web;
using Npgsql;

namespace Admission.Api.Configuration;

/// <summary>
/// Accepts either an Npgsql key-value string or a postgres:// URL (as given by Neon,
/// Supabase and Render) and returns a normalized Npgsql connection string.
/// Cloud hosts get SSL required unless the input explicitly chose an SSL mode.
/// </summary>
public static class ConnectionStringHelper
{
    public static string Normalize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("Connection string is empty.", nameof(raw));
        }

        var trimmed = raw.Trim();
        var isUrl = trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
                    || trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);

        NpgsqlConnectionStringBuilder builder;
        bool sslChosen;
        if (isUrl)
        {
            builder = FromUrl(trimmed, out sslChosen);
        }
        else
        {
            builder = new NpgsqlConnectionStringBuilder(trimmed);
            // The typed builder reports every keyword as present, so inspect the raw text instead.
            var generic = new DbConnectionStringBuilder { ConnectionString = trimmed };
            sslChosen = generic.ContainsKey("SSL Mode") || generic.ContainsKey("SslMode");
        }

        // Cloud hosts have a dotted hostname; "localhost" and compose service names ("db") do not.
        var isCloudHost = builder.Host is { } host && host.Contains('.') && !IsLoopback(host);
        if (isCloudHost && !sslChosen)
        {
            builder.SslMode = SslMode.Require;
        }

        return builder.ConnectionString;
    }

    private static NpgsqlConnectionStringBuilder FromUrl(string url, out bool sslChosen)
    {
        var uri = new Uri(url);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/')),
        };

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            var parts = uri.UserInfo.Split(':', 2);
            builder.Username = Uri.UnescapeDataString(parts[0]);
            if (parts.Length > 1)
            {
                builder.Password = Uri.UnescapeDataString(parts[1]);
            }
        }

        sslChosen = false;
        var sslMode = HttpUtility.ParseQueryString(uri.Query)["sslmode"];
        if (!string.IsNullOrEmpty(sslMode)
            && Enum.TryParse<SslMode>(sslMode.Replace("-", string.Empty), true, out var mode))
        {
            builder.SslMode = mode;
            sslChosen = true;
        }

        return builder;
    }

    private static bool IsLoopback(string host) =>
        host is "localhost" or "127.0.0.1" or "::1";
}
