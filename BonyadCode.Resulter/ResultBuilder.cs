using System.Net;

namespace BonyadCode.Resulter;

/// <summary>
/// A generic result class for encapsulating operation results with data and errors.
/// </summary>
public class ResultBuilder<T>(
    bool succeeded,
    HttpStatusCode? httpStatusCode,
    T? data = default,
    ProblemDetails? problemDetails = null)
{
    /// <summary>
    /// Indicates if the operation was successful.
    /// </summary>
    public bool Succeeded { get; set; } = succeeded;

    /// <summary>
    /// The HTTP status code for the result.
    /// </summary>
    public HttpStatusCode? HttpStatusCode { get; set; } = httpStatusCode;

    /// <summary>
    /// The data returned by the operation (if successful).
    /// </summary>
    public T? Data { get; set; } = data;

    /// <summary>
    /// A dictionary of errors (if the operation failed).
    /// </summary>
    public ProblemDetails? ProblemDetails { get; set; }

    /// <summary>
    /// Creates a new instance of ResultBuilder.
    /// </summary>
    public static ResultBuilder<T> Create(
        bool succeeded,
        HttpStatusCode? statusCode,
        T? data = default,
        ProblemDetails? problemDetails = null)
    {
        var result = new ResultBuilder<T>(succeeded, statusCode, data, problemDetails);
        result.HttpStatusCode ??= problemDetails?.Status;
        if (problemDetails != null) problemDetails.Status ??= result.HttpStatusCode;
        result.HttpStatusCode ??= System.Net.HttpStatusCode.Continue;
        return result;
    }

    /// <summary>
    /// Creates a successful result with the provided data.
    /// </summary>
    public static ResultBuilder<T> Success(
        T? data,
        HttpStatusCode? statusCode = System.Net.HttpStatusCode.OK)
    {
        var result = new ResultBuilder<T>(true, statusCode, data);
        return result;
    }

    /// <summary>
    /// Creates a failed result with optional error details.
    /// </summary>
    public static ResultBuilder<T> Failure(
        ProblemDetails? problemDetails = null,
        HttpStatusCode? statusCode = System.Net.HttpStatusCode.BadRequest)
    {
        var result = new ResultBuilder<T>(false, statusCode, default, problemDetails);
        result.HttpStatusCode ??= problemDetails?.Status;
        if (problemDetails != null) problemDetails.Status ??= result.HttpStatusCode;
        return result;
    }
}

/// <summary>
/// A non-generic variant of ResultBuilder.
/// </summary>
public class ResultBuilder : ResultBuilder<object?>
{
    private ResultBuilder(
        bool succeeded,
        HttpStatusCode? httpStatusCode,
        object? data = null,
        ProblemDetails? problemDetails = null)
        : base(succeeded, httpStatusCode, data, problemDetails)
    {
    }

    /// <summary>
    /// Creates a custom result with data, status code, and errors.
    /// </summary>
    public new static ResultBuilder Create(
        bool succeeded,
        HttpStatusCode? statusCode,
        object? data = null,
        ProblemDetails? problemDetails = null)
    {
        var result = new ResultBuilder(succeeded, statusCode, data, problemDetails);
        result.HttpStatusCode ??= problemDetails?.Status;
        if (problemDetails != null) problemDetails.Status ??= result.HttpStatusCode;
        result.HttpStatusCode ??= System.Net.HttpStatusCode.Continue;
        return result;
    }

    /// <summary>
    /// Creates a successful result with no data.
    /// </summary>
    public static ResultBuilder Success(
        HttpStatusCode? statusCode = System.Net.HttpStatusCode.OK)
    {
        var result = new ResultBuilder(true, statusCode);
        return result;
    }

    /// <summary>
    /// Creates a failed result with optional error details.
    /// </summary>
    public new static ResultBuilder Failure(
        ProblemDetails? problemDetails = null,
        HttpStatusCode? statusCode = System.Net.HttpStatusCode.BadRequest)
    {
        var result = new ResultBuilder(false, statusCode, null, problemDetails);
        result.HttpStatusCode ??= problemDetails?.Status;
        if (problemDetails != null) problemDetails.Status ??= result.HttpStatusCode;
        return result;
    }
}