namespace Admission.Api.Models;

/// <summary>An admission class (one shift of one class) with a fixed seat count.</summary>
public class ClassEntity
{
    public required string Id { get; set; }

    public required string Name { get; set; }

    public int TotalSeats { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<Hold> Holds { get; set; } = [];
}
