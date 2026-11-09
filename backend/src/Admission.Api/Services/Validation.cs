using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Admission.Api.Errors;

namespace Admission.Api.Services;

public static partial class Validation
{
    public const string StudentIdHeader = "X-Student-Id";
    public const int MaxNameLength = 100;
    public const int MinSeats = 1;
    public const int MaxSeats = 100_000;

    private const string IdAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    [GeneratedRegex(@"^\d{17}$")]
    private static partial Regex StudentIdRegex();

    [GeneratedRegex(@"^[A-Za-z0-9_-]{1,64}$")]
    private static partial Regex IdRegex();

    public static string NewId(string prefix) => prefix + RandomNumberGenerator.GetString(IdAlphabet, 12);

    /// <summary>Reads and validates X-Student-Id; throws 400 INVALID_STUDENT_ID.</summary>
    public static string RequireStudentId(HttpRequest request)
    {
        var value = request.Headers[StudentIdHeader].ToString().Trim();
        if (!StudentIdRegex().IsMatch(value))
        {
            throw ApiException.BadRequest("Enter a 17-digit birth registration number.", ErrorCodes.InvalidStudentId);
        }

        return value;
    }

    /// <summary>True when the id is shaped like one we could have issued. Others cannot exist, so callers return 404.</summary>
    public static bool IsPlausibleId(string? id) => id is not null && IdRegex().IsMatch(id);

    public static (string Name, int TotalSeats) ValidateNewClass(Models.CreateClassRequest? request)
    {
        if (request is null)
        {
            throw ApiException.BadRequest("Check the highlighted fields.");
        }

        var name = request.Name?.Trim() ?? "";
        if (name.Length is < 1 or > MaxNameLength || name.Any(char.IsControl))
        {
            throw ApiException.BadRequest($"Class name must be 1 to {MaxNameLength} characters.");
        }

        if (request.TotalSeats is not { } seats || seats < MinSeats || seats > MaxSeats)
        {
            throw ApiException.BadRequest($"Total seats must be a whole number from {MinSeats} to {MaxSeats}.");
        }

        return (name, seats);
    }
}
