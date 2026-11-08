namespace Admission.Api.Models;

public static class HoldStatus
{
    public const string Held = "HELD";
    public const string Confirmed = "CONFIRMED";
    public const string Expired = "EXPIRED";
    public const string Cancelled = "CANCELLED";
}

/// <summary>A seat application. HELD reserves a seat until ExpiresAt; CONFIRMED is permanent.</summary>
public class Hold
{
    public required string Id { get; set; }

    public required string ClassId { get; set; }

    public required string StudentId { get; set; }

    public required string Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public ClassEntity? Class { get; set; }
}
