using System.Net;
using System.Text;

namespace Admission.Tests;

[Collection(DbCollection.Name)]
public sealed class ValidationTests(PostgresFixture db)
{
    private static StringContent Json(string body) => new(body, Encoding.UTF8, "application/json");

    [Theory]
    [InlineData("""{"name":"A","totalSeats":0}""")]
    [InlineData("""{"name":"A","totalSeats":-3}""")]
    [InlineData("""{"name":"A","totalSeats":100001}""")]
    [InlineData("""{"name":"A","totalSeats":5.5}""")]
    [InlineData("""{"name":"A","totalSeats":"5"}""")]
    [InlineData("""{"name":"A","totalSeats":99999999999999}""")]
    [InlineData("""{"name":"A"}""")]
    [InlineData("""{"totalSeats":5}""")]
    [InlineData("""{"name":"   ","totalSeats":5}""")]
    [InlineData("""{"name":12,"totalSeats":5}""")]
    [InlineData("""{"name":"bad\u0000name","totalSeats":5}""")]
    [InlineData("not json")]
    [InlineData("null")]
    [InlineData("[]")]
    public async Task CreateClass_InvalidBody_Returns400(string body)
    {
        using var api = db.CreateApi();
        var res = await api.RawAsync(HttpMethod.Post, "/classes", body: Json(body));
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task CreateClass_NameTooLong_Returns400()
    {
        using var api = db.CreateApi();
        var res = await api.RawAsync(HttpMethod.Post, "/classes", body: Json($$"""{"name":"{{new string('x', 101)}}","totalSeats":5}"""));
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task CreateClass_EmptyBodyOrWrongContentType_Is4xx()
    {
        using var api = db.CreateApi();
        var empty = await api.RawAsync(HttpMethod.Post, "/classes");
        var text = await api.RawAsync(HttpMethod.Post, "/classes", body: new StringContent("x", Encoding.UTF8, "text/plain"));
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.InRange((int)text.StatusCode, 400, 499);
    }

    [Fact]
    public async Task CreateClass_TrimsName()
    {
        using var api = db.CreateApi();
        var cls = await api.CreateClassAsync(3, "  Grade 1  ");
        Assert.Equal("Grade 1", cls.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234567890123456")]
    [InlineData("123456789012345678")]
    [InlineData("1234567890123456a")]
    [InlineData("12345 67890123456")]
    public async Task StudentEndpoints_BadHeader_Return400(string? header)
    {
        using var api = db.CreateApi();
        var cls = await api.CreateClassAsync(1);

        var apply = await api.ApplyAsync(cls.Id, header);
        Assert.Equal(HttpStatusCode.BadRequest, apply.StatusCode);
        Assert.Equal("INVALID_STUDENT_ID", (await apply.ErrorAsync()).Code);
        Assert.Equal(HttpStatusCode.BadRequest, (await api.MyHoldsAsync(header)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await api.ConfirmAsync("h_x", header)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await api.CancelAsync("h_x", header)).StatusCode);
    }

    [Theory]
    [InlineData("/classes/c_doesnotexist")]
    [InlineData("/holds/h_doesnotexist")]
    [InlineData("/holds/a%20b")]
    public async Task UnknownOrMalformedIds_Return404_OnReads(string url)
    {
        using var api = db.CreateApi();
        var res = await api.RawAsync(HttpMethod.Get, url);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        Assert.Equal("NOT_FOUND", (await res.ErrorAsync()).Code);
    }

    [Fact]
    public async Task UnknownIds_Return404_OnWrites()
    {
        using var api = db.CreateApi();
        var student = Api.NewStudent();
        Assert.Equal(HttpStatusCode.NotFound, (await api.ApplyAsync("c_missing", student)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.ConfirmAsync("h_missing", student)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.CancelAsync("h_missing", student)).StatusCode);
    }

    [Fact]
    public async Task Errors_AreJsonWithCodeAndMessage()
    {
        using var api = db.CreateApi();
        var res = await api.RawAsync(HttpMethod.Get, "/no/such/route");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        var err = await res.ErrorAsync();
        Assert.False(string.IsNullOrWhiteSpace(err.Code));
        Assert.False(string.IsNullOrWhiteSpace(err.Message));
    }

    [Fact]
    public async Task ClassList_ContainsDerivedCounts()
    {
        using var api = db.CreateApi();
        var cls = await api.CreateClassAsync(4);
        var student = Api.NewStudent();
        var hold = await api.ApplyOkAsync(cls.Id, student);
        await api.ConfirmAsync(hold.HoldId, student);
        await api.ApplyOkAsync(cls.Id, Api.NewStudent());

        var one = await api.GetClassAsync(cls.Id);
        Assert.Equal(new ClassDto(cls.Id, "Class One", 4, 1, 1, 2), one);
    }
}
