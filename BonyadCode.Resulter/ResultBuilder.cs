using Microsoft.AspNetCore.Http;

namespace BonyadCode.Resulter;

/// <summary>
/// A generic result class for encapsulating operation results with data and errors.
/// </summary>
public class ResultBuilder<T>(
    bool succeeded,
    int? statusCode,
    T? data = default,
    Dictionary<string, string[]>? errors = null)
{
    /// <summary>
    /// Indicates if the operation was successful.
    /// </summary>
    public bool Succeeded { get; set; } = succeeded;

    /// <summary>
    /// The HTTP status code for the result.
    /// </summary>
    public int? StatusCode { get; set; } = statusCode;

    /// <summary>
    /// The data returned by the operation (if successful).
    /// </summary>
    public T? Data { get; set; } = data;

    /// <summary>
    /// A dictionary of errors (if the operation failed).
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; } = errors;

    /// <summary>
    /// Creates a new instance of ResultBuilder.
    /// </summary>
    public static ResultBuilder<T> Create(bool succeeded, int? statusCode, T? data = default, Dictionary<string, string[]>? errors = null) =>
        new(succeeded, statusCode, data, errors);

    /// <summary>
    /// Creates a successful result with the provided data.
    /// </summary>
    public static ResultBuilder<T> Success(T? data, int? statusCode = StatusCodes.Status200OK) =>
        new(true, statusCode, data);

    /// <summary>
    /// Creates a failed result with optional error details.
    /// </summary>
    public static ResultBuilder<T> Failure(Dictionary<string, string[]>? errors = null, int? statusCode = StatusCodes.Status400BadRequest) =>
        new(false, statusCode, default, errors is { Count: > 0 } ? new Dictionary<string, string[]>(errors) : null);
}

/// <summary>
/// A non-generic variant of ResultBuilder.
/// </summary>
public class ResultBuilder : ResultBuilder<object?>
{
    private ResultBuilder(bool succeeded, int? statusCode, object? data = null, Dictionary<string, string[]>? errors = null)
        : base(succeeded, statusCode, data, errors) { }

    /// <summary>
    /// Creates a successful result with no data.
    /// </summary>
    public static ResultBuilder Success(int? statusCode = StatusCodes.Status200OK) =>
        new(true, statusCode);

    /// <summary>
    /// Creates a failed result with optional error details.
    /// </summary>
    public new static ResultBuilder Failure(Dictionary<string, string[]>? errors = null, int? statusCode = StatusCodes.Status400BadRequest) =>
        new(false, statusCode, null, errors is { Count: > 0 } ? new Dictionary<string, string[]>(errors) : null);

    /// <summary>
    /// Creates a custom result with data, status code, and errors.
    /// </summary>
    public new static ResultBuilder Create(bool succeeded, int? statusCode, object? data = null, Dictionary<string, string[]>? errors = null) =>
        new(succeeded, statusCode, data, errors);
}
