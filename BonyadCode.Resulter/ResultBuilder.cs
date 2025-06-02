using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BonyadCode.Resulter;

/// <summary>
/// A generic result class that encapsulates the result of an operation,
/// optionally containing data and structured problem details.
/// </summary>
/// <typeparam name="T">The type of the result data.</typeparam>
public class ResultBuilder<T>(
    bool succeeded,
    HttpStatusCode? statusCode,
    T? data = default,
    ProblemDetails? problemDetails = null)
{
    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    public bool Succeeded { get; set; } = succeeded;

    /// <summary>
    /// The HTTP status code associated with the result.
    /// Defaults based on success/failure if not explicitly set.
    /// </summary>
    public HttpStatusCode? StatusCode { get; set; } = statusCode;

    /// <summary>
    /// The result data returned from the operation, if any.
    /// </summary>
    public T? Data { get; set; } = data;

    /// <summary>
    /// Contains structured error information, if applicable.
    /// </summary>
    public ProblemDetails? ProblemDetails { get; set; } = problemDetails;

    /// <summary>
    /// Implicitly converts data to a successful ResultBuilder instance.
    /// </summary>
    /// <param name="data">The result data.</param>
    public static implicit operator ResultBuilder<T>(T data) => Success(data);

    /// <summary>
    /// Creates a new result with optional data or error details.
    /// </summary>
    public static ResultBuilder<T> Create(
        bool succeeded,
        HttpStatusCode? statusCode,
        T? data = default,
        ProblemDetails? problemDetails = null)
    {
        var result = new ResultBuilder<T>(succeeded, statusCode, data, problemDetails);
        result.StatusCode ??= (HttpStatusCode)(problemDetails?.Status ?? 200);
        if (problemDetails != null) problemDetails.Status ??= (int)result.StatusCode;
        result.StatusCode ??= HttpStatusCode.OK;
        return result;
    }

    /// <summary>
    /// Returns a successful result.
    /// </summary>
    public static ResultBuilder<T> Success(
        T? data,
        HttpStatusCode? statusCode = HttpStatusCode.OK)
    {
        return new ResultBuilder<T>(true, statusCode, data);
    }

    /// <summary>
    /// Returns a failure result, optionally with problem details.
    /// </summary>
    public static ResultBuilder<T> Failure(
        ProblemDetails? problemDetails = null,
        HttpStatusCode? statusCode = HttpStatusCode.BadRequest)
    {
        var result = new ResultBuilder<T>(false, statusCode, default, problemDetails)
            .WithSimpleProblemDetails();
        result.StatusCode ??= (HttpStatusCode)(problemDetails?.Status ?? 400);
        if (problemDetails != null) problemDetails.Status ??= (int)result.StatusCode;
        result.StatusCode ??= HttpStatusCode.BadRequest;
        return result;
    }
}

/// <summary>
/// A non-generic version of ResultBuilder used when no result data is required.
/// </summary>
public class ResultBuilder : ResultBuilder<object?>
{
    private ResultBuilder(
        bool succeeded,
        HttpStatusCode? statusCode,
        object? data = null,
        ProblemDetails? problemDetails = null)
        : base(succeeded, statusCode, data, problemDetails) { }

    /// <summary>
    /// Creates a result indicating success or failure.
    /// </summary>
    public new static ResultBuilder Create(
        bool succeeded,
        HttpStatusCode? statusCode,
        object? data = null,
        ProblemDetails? problemDetails = null)
    {
        var result = new ResultBuilder(succeeded, statusCode, data, problemDetails);
        result.StatusCode ??= (HttpStatusCode)(problemDetails?.Status ?? 200);
        if (problemDetails != null) problemDetails.Status ??= (int)result.StatusCode;
        result.StatusCode ??= HttpStatusCode.OK;
        return result;
    }

    /// <summary>
    /// Returns a successful result.
    /// </summary>
    public static ResultBuilder Success(HttpStatusCode? statusCode = System.Net.HttpStatusCode.OK)
    {
        return new ResultBuilder(true, statusCode);
    }

    /// <summary>
    /// Returns a failed result with optional problem details.
    /// </summary>
    public new static ResultBuilder Failure(
        ProblemDetails? problemDetails = null,
        HttpStatusCode? statusCode = HttpStatusCode.BadRequest)
    {
        var result = new ResultBuilder(false, statusCode, null, problemDetails)
            .WithSimpleProblemDetails();
        result.StatusCode ??= (HttpStatusCode)(problemDetails?.Status ?? 400);
        if (problemDetails != null) problemDetails.Status ??= (int)result.StatusCode;
        result.StatusCode ??= HttpStatusCode.BadRequest;
        return result;
    }
}