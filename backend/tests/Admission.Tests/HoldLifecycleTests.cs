using System.Net;
using System.Net.Http.Json;

namespace Admission.Tests;

[Collection(DbCollection.Name)]
public sealed class HoldLifecycleTests(PostgresFixture db)
{
    [Fact]
    public async Task Expiry_FreesSeatWithoutSweeper_AndConfirmIsRejected()
    {
        using var api = db.CreateApi(holdSeconds: 1, sweeper: false);
        var cls = await api.CreateClassAsync(1);
        var first = Api.NewStudent();
        var hold = await api.ApplyOkAsync(cls.Id, first);

        Assert.Equal(HttpStatusCode.Conflict, (await api.ApplyAsync(cls.Id, Api.NewStudent())).StatusCode);

        await Task.Delay(1300);

        Assert.Equal("EXPIRED", await api.StatusOfAsync(hold.HoldId));
        var counts = await api.GetClassAsync(cls.Id);
        Assert.Equal(0, counts.HeldSeats);
        Assert.Equal(1, counts.AvailableSeats);

        var confirm = await api.ConfirmAsync(hold.HoldId, first);
        Assert.Equal(HttpStatusCode.Conflict, confirm.StatusCode);
        Assert.Equal("HOLD_EXPIRED", (await confirm.ErrorAsync()).Code);

        Assert.Equal(HttpStatusCode.Created, (await api.ApplyAsync(cls.Id, Api.NewStudent())).StatusCode);
    }

    [Fact]
    public async Task SameStudent_CanReapplyAfterTheirHoldLapses()
    {
        using var api = db.CreateApi(holdSeconds: 1, sweeper: false);
        var cls = await api.CreateClassAsync(1);
        var student = Api.NewStudent();
        await api.ApplyOkAsync(cls.Id, student);

        await Task.Delay(1300);

        var again = await api.ApplyAsync(cls.Id, student);
        Assert.Equal(HttpStatusCode.Created, again.StatusCode);
    }

    [Fact]
    public async Task Sweeper_PersistsExpiredStatus()
    {
        using var api = db.CreateApi(holdSeconds: 1, sweeper: true);
        var cls = await api.CreateClassAsync(1);
        var hold = await api.ApplyOkAsync(cls.Id, Api.NewStudent());

        await Task.Delay(2500);

        Assert.Equal("EXPIRED", await api.StatusOfAsync(hold.HoldId));
        Assert.Equal(1, (await api.GetClassAsync(cls.Id)).AvailableSeats);
    }

    [Fact]
    public async Task ConfirmedHold_NeverExpires()
    {
        using var api = db.CreateApi(holdSeconds: 1, sweeper: true);
        var cls = await api.CreateClassAsync(1);
        var student = Api.NewStudent();
        var hold = await api.ApplyOkAsync(cls.Id, student);
        Assert.Equal(HttpStatusCode.OK, (await api.ConfirmAsync(hold.HoldId, student)).StatusCode);

        await Task.Delay(2500);

        Assert.Equal("CONFIRMED", await api.StatusOfAsync(hold.HoldId));
        Assert.Equal(1, (await api.GetClassAsync(cls.Id)).ConfirmedSeats);
    }

    [Fact]
    public async Task Cancel_FreesSeat_ReapplyWorks_AndCancelIsIdempotent()
    {
        using var api = db.CreateApi();
        var cls = await api.CreateClassAsync(1);
        var student = Api.NewStudent();
        var hold = await api.ApplyOkAsync(cls.Id, student);

        Assert.Equal(HttpStatusCode.NoContent, (await api.CancelAsync(hold.HoldId, student)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await api.CancelAsync(hold.HoldId, student)).StatusCode);
        Assert.Equal("CANCELLED", await api.StatusOfAsync(hold.HoldId));
        Assert.Equal(1, (await api.GetClassAsync(cls.Id)).AvailableSeats);

        Assert.Equal(HttpStatusCode.Created, (await api.ApplyAsync(cls.Id, student)).StatusCode);
    }

    [Fact]
    public async Task ConfirmedHold_CannotBeCancelled()
    {
        using var api = db.CreateApi();
        var cls = await api.CreateClassAsync(1);
        var student = Api.NewStudent();
        var hold = await api.ApplyOkAsync(cls.Id, student);
        await api.ConfirmAsync(hold.HoldId, student);

        var res = await api.CancelAsync(hold.HoldId, student);
        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
        Assert.Equal("HOLD_NOT_ACTIVE", (await res.ErrorAsync()).Code);
    }

    [Fact]
    public async Task Ownership_IsEnforcedOnConfirmAndCancel()
    {
        using var api = db.CreateApi();
        var cls = await api.CreateClassAsync(2);
        var owner = Api.NewStudent();
        var hold = await api.ApplyOkAsync(cls.Id, owner);
        var other = Api.NewStudent();

        var confirm = await api.ConfirmAsync(hold.HoldId, other);
        Assert.Equal(HttpStatusCode.Forbidden, confirm.StatusCode);
        Assert.Equal("NOT_OWNER", (await confirm.ErrorAsync()).Code);
        Assert.Equal(HttpStatusCode.Forbidden, (await api.CancelAsync(hold.HoldId, other)).StatusCode);
        Assert.Equal("HELD", await api.StatusOfAsync(hold.HoldId));
    }

    [Fact]
    public async Task MyHolds_ListsOwnApplicationsNewestFirst_WithClassName()
    {
        using var api = db.CreateApi();
        var a = await api.CreateClassAsync(2, "Alpha");
        var b = await api.CreateClassAsync(2, "Beta");
        var student = Api.NewStudent();
        await api.ApplyOkAsync(a.Id, student);
        await api.ApplyOkAsync(b.Id, student);
        await api.ApplyOkAsync(a.Id, Api.NewStudent());

        var list = await (await api.MyHoldsAsync(student)).Content.ReadFromJsonAsync<List<System.Text.Json.JsonElement>>();

        Assert.NotNull(list);
        Assert.Equal(2, list.Count);
        Assert.Equal("Beta", list[0].GetProperty("className").GetString());
        Assert.Equal("Alpha", list[1].GetProperty("className").GetString());
    }

    [Fact]
    public async Task ExpiresAt_IsUtcIso8601WithZ()
    {
        using var api = db.CreateApi();
        var cls = await api.CreateClassAsync(1);
        var res = await api.ApplyAsync(cls.Id, Api.NewStudent());
        var json = await res.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

        Assert.EndsWith("Z", json.GetProperty("expiresAt").GetString());
    }
}
