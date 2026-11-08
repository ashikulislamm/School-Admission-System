using Admission.Api.Configuration;
using Npgsql;

namespace Admission.Tests;

public class ConnectionStringHelperTests
{
    [Fact]
    public void Url_form_is_converted_and_cloud_host_requires_ssl()
    {
        var result = ConnectionStringHelper.Normalize(
            "postgres://user1:p%40ss@ep-cool-123.us-east-2.aws.neon.tech/neondb");
        var b = new NpgsqlConnectionStringBuilder(result);

        Assert.Equal("ep-cool-123.us-east-2.aws.neon.tech", b.Host);
        Assert.Equal(5432, b.Port);
        Assert.Equal("neondb", b.Database);
        Assert.Equal("user1", b.Username);
        Assert.Equal("p@ss", b.Password);
        Assert.Equal(SslMode.Require, b.SslMode);
    }

    [Fact]
    public void Url_sslmode_query_is_respected()
    {
        var b = new NpgsqlConnectionStringBuilder(
            ConnectionStringHelper.Normalize("postgresql://u:p@db.example.com:6543/app?sslmode=disable"));

        Assert.Equal(6543, b.Port);
        Assert.Equal(SslMode.Disable, b.SslMode);
    }

    [Theory]
    [InlineData("Host=db;Port=5432;Database=admission;Username=a;Password=b")]
    [InlineData("Host=localhost;Port=5433;Database=admission;Username=a;Password=b")]
    public void Local_and_compose_hosts_do_not_force_ssl(string raw)
    {
        var b = new NpgsqlConnectionStringBuilder(ConnectionStringHelper.Normalize(raw));

        Assert.NotEqual(SslMode.Require, b.SslMode);
    }

    [Fact]
    public void Explicit_ssl_mode_in_key_value_string_is_respected()
    {
        var b = new NpgsqlConnectionStringBuilder(ConnectionStringHelper.Normalize(
            "Host=db.abc.supabase.co;Database=postgres;Username=u;Password=p;SSL Mode=Disable"));

        Assert.Equal(SslMode.Disable, b.SslMode);
    }

    [Fact]
    public void Key_value_cloud_host_requires_ssl()
    {
        var b = new NpgsqlConnectionStringBuilder(ConnectionStringHelper.Normalize(
            "Host=db.abc.supabase.co;Port=5432;Database=postgres;Username=u;Password=p"));

        Assert.Equal(SslMode.Require, b.SslMode);
    }

    [Fact]
    public void Empty_value_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => ConnectionStringHelper.Normalize("  "));
    }
}
