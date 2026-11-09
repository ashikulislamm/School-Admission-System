using Admission.Api.Data;
using Admission.Api.Errors;
using Admission.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Admission.Api.Services;

public sealed class ClassService(AppDbContext db)
{
    public async Task<ClassResponse> CreateAsync(string name, int totalSeats, CancellationToken ct)
    {
        var entity = new ClassEntity { Id = Validation.NewId("c_"), Name = name, TotalSeats = totalSeats };
        db.Classes.Add(entity);
        await db.SaveChangesAsync(ct);
        return new ClassResponse(entity.Id, entity.Name, entity.TotalSeats, 0, 0, entity.TotalSeats);
    }

    public async Task<List<ClassResponse>> ListAsync(CancellationToken ct)
    {
        var rows = await db.Database.SqlQuery<ClassRow>($"""
            SELECT c.id AS "Id", c.name AS "Name", c.total_seats AS "TotalSeats",
                   (COUNT(*) FILTER (WHERE h.status = 'HELD' AND h.expires_at > clock_timestamp()))::int AS "HeldSeats",
                   (COUNT(*) FILTER (WHERE h.status = 'CONFIRMED'))::int AS "ConfirmedSeats"
            FROM classes c
            LEFT JOIN holds h ON h.class_id = c.id
            GROUP BY c.id, c.name, c.total_seats, c.created_at
            ORDER BY c.created_at, c.id
            """).ToListAsync(ct);
        return rows.Select(ToResponse).ToList();
    }

    public async Task<ClassResponse> GetAsync(string id, CancellationToken ct)
    {
        if (Validation.IsPlausibleId(id))
        {
            var rows = await db.Database.SqlQuery<ClassRow>($"""
                SELECT c.id AS "Id", c.name AS "Name", c.total_seats AS "TotalSeats",
                       (COUNT(*) FILTER (WHERE h.status = 'HELD' AND h.expires_at > clock_timestamp()))::int AS "HeldSeats",
                       (COUNT(*) FILTER (WHERE h.status = 'CONFIRMED'))::int AS "ConfirmedSeats"
                FROM classes c
                LEFT JOIN holds h ON h.class_id = c.id
                WHERE c.id = {id}
                GROUP BY c.id, c.name, c.total_seats
                """).ToListAsync(ct);
            if (rows.Count == 1)
            {
                return ToResponse(rows[0]);
            }
        }

        throw ApiException.NotFound("That class was not found.");
    }

    private static ClassResponse ToResponse(ClassRow r) =>
        new(r.Id, r.Name, r.TotalSeats, r.HeldSeats, r.ConfirmedSeats, r.TotalSeats - r.HeldSeats - r.ConfirmedSeats);
}
