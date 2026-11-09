using Admission.Api.Configuration;
using Admission.Api.Data;
using Admission.Api.Errors;
using Admission.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Admission.Api.Services;

/// <summary>
/// Seat-hold rules. Every decision runs inside a Postgres row lock (class row, then hold row)
/// and uses the database clock, so any number of processes can share one database.
/// clock_timestamp() is used instead of now() because now() is frozen at transaction start,
/// which would be stale after waiting on a lock.
/// </summary>
public sealed class HoldService(AppDbContext db, AppSettings settings)
{
    // Effective status: a HELD row past its expiry reads as EXPIRED even before the sweeper persists it.
    private const string EffectiveStatusSql =
        "CASE WHEN h.status = 'HELD' AND h.expires_at <= clock_timestamp() THEN 'EXPIRED' ELSE h.status END";

    public async Task<HoldCreatedResponse> ApplyAsync(string classId, string studentId, CancellationToken ct)
    {
        if (!Validation.IsPlausibleId(classId))
        {
            throw ClassNotFound();
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // 1. Serialize all appliers for this class (across processes).
        var locked = await db.Database
            .SqlQuery<string>($"""SELECT id AS "Value" FROM classes WHERE id = {classId} FOR UPDATE""")
            .ToListAsync(ct);
        if (locked.Count == 0)
        {
            throw ClassNotFound();
        }

        // 2. Persist this student's lapsed holds so the unique index does not block a re-apply.
        await db.Database.ExecuteSqlAsync($"""
            UPDATE holds SET status = 'EXPIRED'
            WHERE class_id = {classId} AND student_id = {studentId}
              AND status = 'HELD' AND expires_at <= clock_timestamp()
            """, ct);

        // 3. One active application per student per class.
        var existing = await db.Database.SqlQuery<int>($"""
            SELECT count(*)::int AS "Value" FROM holds
            WHERE class_id = {classId} AND student_id = {studentId} AND status IN ('HELD', 'CONFIRMED')
            """).ToListAsync(ct);
        if (existing[0] > 0)
        {
            throw ApiException.Conflict(ErrorCodes.AlreadyApplied, "You already have an application for this class.");
        }

        // 4. Capacity.
        var total = await db.Database
            .SqlQuery<int>($"""SELECT total_seats AS "Value" FROM classes WHERE id = {classId}""")
            .ToListAsync(ct);
        var active = await db.Database.SqlQuery<int>($"""
            SELECT count(*)::int AS "Value" FROM holds
            WHERE class_id = {classId}
              AND (status = 'CONFIRMED' OR (status = 'HELD' AND expires_at > clock_timestamp()))
            """).ToListAsync(ct);
        if (active[0] >= total[0])
        {
            throw ApiException.Conflict(ErrorCodes.ClassFull, "All seats in this class are taken.");
        }

        // 5. Insert with the expiry computed by the database.
        var holdId = Validation.NewId("h_");
        var seconds = settings.HoldDurationSeconds;
        var expires = await db.Database.SqlQuery<DateTime>($"""
            INSERT INTO holds (id, class_id, student_id, status, expires_at)
            VALUES ({holdId}, {classId}, {studentId}, 'HELD', clock_timestamp() + make_interval(secs => {seconds}::double precision))
            RETURNING expires_at AS "Value"
            """).ToListAsync(ct);

        await tx.CommitAsync(ct);
        return new HoldCreatedResponse(holdId, classId, studentId, HoldStatus.Held, expires[0]);
    }

    public async Task<HoldResponse> GetAsync(string holdId, CancellationToken ct)
    {
        if (Validation.IsPlausibleId(holdId))
        {
            var rows = await db.Database.SqlQueryRaw<HoldRow>($$"""
                SELECT h.id AS "Id", h.class_id AS "ClassId", '' AS "ClassName", h.student_id AS "StudentId",
                       {{EffectiveStatusSql}} AS "Status", h.expires_at AS "ExpiresAt"
                FROM holds h WHERE h.id = {0}
                """, holdId).ToListAsync(ct);
            if (rows.Count == 1)
            {
                var r = rows[0];
                return new HoldResponse(r.Id, r.ClassId, r.StudentId, r.Status, r.ExpiresAt);
            }
        }

        throw HoldNotFound();
    }

    public async Task<HoldConfirmedResponse> ConfirmAsync(string holdId, string studentId, CancellationToken ct)
    {
        if (!Validation.IsPlausibleId(holdId))
        {
            throw HoldNotFound();
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var hold = await LockHoldAsync(holdId, ct);
        if (hold.StudentId != studentId)
        {
            throw ApiException.Forbidden("This application belongs to someone else.");
        }

        if (hold.Status == HoldStatus.Confirmed)
        {
            return new HoldConfirmedResponse(holdId, HoldStatus.Confirmed); // idempotent
        }

        if (hold.Status != HoldStatus.Held)
        {
            throw ApiException.Conflict(ErrorCodes.HoldNotActive, "This application is no longer active.");
        }

        var confirmed = await db.Database.ExecuteSqlAsync($"""
            UPDATE holds SET status = 'CONFIRMED', confirmed_at = clock_timestamp()
            WHERE id = {holdId} AND status = 'HELD' AND expires_at > clock_timestamp()
            """, ct);
        if (confirmed == 1)
        {
            await tx.CommitAsync(ct);
            return new HoldConfirmedResponse(holdId, HoldStatus.Confirmed);
        }

        // Lapsed: persist EXPIRED, then report it.
        await db.Database.ExecuteSqlAsync(
            $"UPDATE holds SET status = 'EXPIRED' WHERE id = {holdId} AND status = 'HELD'", ct);
        await tx.CommitAsync(ct);
        throw ApiException.Conflict(ErrorCodes.HoldExpired, "This application has expired.");
    }

    public async Task CancelAsync(string holdId, string studentId, CancellationToken ct)
    {
        if (!Validation.IsPlausibleId(holdId))
        {
            throw HoldNotFound();
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var hold = await LockHoldAsync(holdId, ct);
        if (hold.StudentId != studentId)
        {
            throw ApiException.Forbidden("This application belongs to someone else.");
        }

        if (hold.Status == HoldStatus.Confirmed)
        {
            throw ApiException.Conflict(ErrorCodes.HoldNotActive, "A confirmed application cannot be cancelled.");
        }

        if (hold.Status == HoldStatus.Held)
        {
            // A lapsed hold becomes EXPIRED rather than CANCELLED.
            await db.Database.ExecuteSqlAsync($"""
                UPDATE holds
                SET status = CASE WHEN expires_at > clock_timestamp() THEN 'CANCELLED' ELSE 'EXPIRED' END
                WHERE id = {holdId} AND status = 'HELD'
                """, ct);
        }

        await tx.CommitAsync(ct);
    }

    public async Task<List<MyHoldResponse>> ListMineAsync(string studentId, CancellationToken ct)
    {
        var rows = await db.Database.SqlQueryRaw<HoldRow>($$"""
            SELECT h.id AS "Id", h.class_id AS "ClassId", c.name AS "ClassName", h.student_id AS "StudentId",
                   {{EffectiveStatusSql}} AS "Status", h.expires_at AS "ExpiresAt"
            FROM holds h JOIN classes c ON c.id = h.class_id
            WHERE h.student_id = {0}
            ORDER BY h.created_at DESC, h.id
            """, studentId).ToListAsync(ct);
        return rows.Select(r => new MyHoldResponse(r.Id, r.ClassId, r.ClassName, r.Status, r.ExpiresAt)).ToList();
    }

    /// <summary>Locks the hold row (FOR UPDATE) and returns it with its stored status. Call inside a transaction.</summary>
    private async Task<Hold> LockHoldAsync(string holdId, CancellationToken ct)
    {
        var rows = await db.Holds
            .FromSql($"SELECT * FROM holds WHERE id = {holdId} FOR UPDATE")
            .AsNoTracking()
            .ToListAsync(ct);
        return rows.Count == 1 ? rows[0] : throw HoldNotFound();
    }

    private static ApiException ClassNotFound() => ApiException.NotFound("That class was not found.");

    private static ApiException HoldNotFound() => ApiException.NotFound("That application was not found.");
}
