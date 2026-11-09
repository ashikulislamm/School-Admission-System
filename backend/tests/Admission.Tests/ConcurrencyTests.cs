using System.Net;

namespace Admission.Tests;

[Collection(DbCollection.Name)]
public sealed class ConcurrencyTests(PostgresFixture db)
{
    [Fact]
    public async Task ThreeHundredParallelStudents_OnlyTotalSeatsSucceed()
    {
        using var api = db.CreateApi();
        var cls = await api.CreateClassAsync(120);

        var results = await Task.WhenAll(Enumerable.Range(0, 300)
            .Select(_ => Task.Run(async () => (await api.ApplyAsync(cls.Id, Api.NewStudent())).StatusCode)));

        Assert.Equal(120, results.Count(s => s == HttpStatusCode.Created));
        Assert.Equal(180, results.Count(s => s == HttpStatusCode.Conflict));

        var after = await api.GetClassAsync(cls.Id);
        Assert.Equal(120, after.HeldSeats);
        Assert.Equal(0, after.AvailableSeats);
    }

    [Fact]
    public async Task SameStudentTwentyParallel_ExactlyOneHold()
    {
        using var api = db.CreateApi();
        var cls = await api.CreateClassAsync(10);
        var student = Api.NewStudent();

        var responses = await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(_ => Task.Run(() => api.ApplyAsync(cls.Id, student))));

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.All(responses.Where(r => r.StatusCode != HttpStatusCode.Created), r =>
            Assert.Equal(HttpStatusCode.Conflict, r.StatusCode));
        Assert.Equal(1, (await api.GetClassAsync(cls.Id)).HeldSeats);
    }

    [Fact]
    public async Task TwentyParallelConfirms_ConfirmOnce()
    {
        using var api = db.CreateApi();
        var cls = await api.CreateClassAsync(5);
        var student = Api.NewStudent();
        var hold = await api.ApplyOkAsync(cls.Id, student);

        var responses = await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(_ => Task.Run(() => api.ConfirmAsync(hold.HoldId, student))));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        var after = await api.GetClassAsync(cls.Id);
        Assert.Equal(1, after.ConfirmedSeats);
        Assert.Equal(0, after.HeldSeats);
    }

    [Fact]
    public async Task TwoInstancesOnOneDatabase_NeverOversell()
    {
        using var a = db.CreateApi();
        using var b = db.CreateApi();
        var cls = await a.CreateClassAsync(50);

        var results = await Task.WhenAll(Enumerable.Range(0, 200).Select(i => Task.Run(async () =>
            (await (i % 2 == 0 ? a : b).ApplyAsync(cls.Id, Api.NewStudent())).StatusCode)));

        Assert.Equal(50, results.Count(s => s == HttpStatusCode.Created));
        Assert.Equal(150, results.Count(s => s == HttpStatusCode.Conflict));
        Assert.Equal(50, (await b.GetClassAsync(cls.Id)).HeldSeats);
    }
}
