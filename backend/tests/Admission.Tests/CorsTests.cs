namespace Admission.Tests;

[Collection(DbCollection.Name)]
public sealed class CorsTests(PostgresFixture db)
{
    [Fact]
    public async Task AllowedOrigin_GetsCorsHeaders_AndDateIsExposed()
    {
        using var api = db.CreateApi();
        var req = new HttpRequestMessage(HttpMethod.Get, "/classes");
        req.Headers.Add("Origin", "http://localhost:3000");

        var res = await api.SendRawAsync(req);

        Assert.Equal("http://localhost:3000", res.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.Contains("Date", string.Join(",", res.Headers.GetValues("Access-Control-Expose-Headers")));
    }

    [Fact]
    public async Task UnknownOrigin_GetsNoCorsHeaders()
    {
        using var api = db.CreateApi();
        var req = new HttpRequestMessage(HttpMethod.Get, "/classes");
        req.Headers.Add("Origin", "http://evil.example");

        var res = await api.SendRawAsync(req);

        Assert.False(res.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
