namespace Admission.Api.Errors;

/// <summary>Stable error codes returned in the `code` field of every error body.</summary>
public static class ErrorCodes
{
    public const string InvalidInput = "INVALID_INPUT";
    public const string InvalidStudentId = "INVALID_STUDENT_ID";
    public const string NotFound = "NOT_FOUND";
    public const string NotOwner = "NOT_OWNER";
    public const string ClassFull = "CLASS_FULL";
    public const string AlreadyApplied = "ALREADY_APPLIED";
    public const string HoldExpired = "HOLD_EXPIRED";
    public const string HoldNotActive = "HOLD_NOT_ACTIVE";
    public const string MethodNotAllowed = "METHOD_NOT_ALLOWED";
    public const string Unavailable = "SERVICE_UNAVAILABLE";
    public const string Internal = "INTERNAL_ERROR";
}

/// <summary>Body of every error response.</summary>
public sealed record ErrorResponse(string Code, string Message);

/// <summary>A domain failure that maps to a specific HTTP status and error code.</summary>
public sealed class ApiException(int statusCode, string code, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    public string Code { get; } = code;

    public static ApiException BadRequest(string message, string code = ErrorCodes.InvalidInput) =>
        new(StatusCodes.Status400BadRequest, code, message);

    public static ApiException Forbidden(string message) =>
        new(StatusCodes.Status403Forbidden, ErrorCodes.NotOwner, message);

    public static ApiException NotFound(string message) =>
        new(StatusCodes.Status404NotFound, ErrorCodes.NotFound, message);

    public static ApiException Conflict(string code, string message) =>
        new(StatusCodes.Status409Conflict, code, message);
}
