namespace Admission.Api.Models;

// Requests. Properties are nullable so missing fields reach our validation (4xx) instead of defaulting.
public sealed record CreateClassRequest(string? Name, int? TotalSeats);

// Responses (contract: PRD §6). Extra fields allowed, none removed.
public sealed record ClassResponse(
    string Id,
    string Name,
    int TotalSeats,
    int HeldSeats,
    int ConfirmedSeats,
    int AvailableSeats);

public sealed record HoldCreatedResponse(string HoldId, string ClassId, string StudentId, string Status, DateTime ExpiresAt);

public sealed record HoldResponse(string HoldId, string ClassId, string StudentId, string Status, DateTime ExpiresAt);

public sealed record HoldConfirmedResponse(string HoldId, string Status);

public sealed record MyHoldResponse(string HoldId, string ClassId, string ClassName, string Status, DateTime ExpiresAt);

// Raw SQL row shapes (read with Database.SqlQuery; column aliases match property names).
public sealed class ClassRow
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int TotalSeats { get; set; }
    public int HeldSeats { get; set; }
    public int ConfirmedSeats { get; set; }
}

public sealed class HoldRow
{
    public string Id { get; set; } = "";
    public string ClassId { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string StudentId { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
}
