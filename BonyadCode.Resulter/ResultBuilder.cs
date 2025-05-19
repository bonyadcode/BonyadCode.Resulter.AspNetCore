using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace BonyadCode.Resulter;

/// <summary>
/// A generic result class that encapsulates the result of an operation,
/// optionally containing data and problem details.
/// </summary>
/// <typeparam name="T">The type of the result data.</typeparam>
public class ResultBuilder<T>(
    bool succeeded,
    HttpStatusCode? httpStatusCode,
    T? data = default,
    ProblemDetails? problemDetails = null)
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool Succeeded { get; set; } = succeeded;

    /// <summary>
    /// The HTTP status code associated with the result.
    /// </summary>
    public HttpStatusCode? HttpStatusCode { get; set; } = httpStatusCode;

    /// <summary>
    /// The result data returned from the operation (if any).
    /// </summary>
    public T? Data { get; set; } = data;

    /// <summary>
    /// Problem details containing error information (if any).
    /// </summary>
    public ProblemDetails? ProblemDetails { get; set; } = problemDetails;

    /// <summary>
    /// Implicitly converts the data to a successful <see cref="ResultBuilder{T}"/>.
    /// </summary>
    /// <param name="data">The result data.</param>
    public static implicit operator ResultBuilder<T>(T data) => Success(data);

    /// <summary>
    /// Creates a new <see cref="ResultBuilder{T}"/> instance with the specified parameters.
    /// </summary>
    /// <param name="succeeded">Indicates whether the operation succeeded.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="data">The result data.</param>
    /// <param name="problemDetails">Optional problem details.</param>
    public static ResultBuilder<T> Create(
        bool succeeded,
        HttpStatusCode? statusCode,
        T? data = default,
        ProblemDetails? problemDetails = null)
    {
        var result = new ResultBuilder<T>(succeeded, statusCode, data, problemDetails);
        result.HttpStatusCode ??= (HttpStatusCode)(problemDetails?.Status ?? 100);
        if (problemDetails != null) problemDetails.Status ??= (int)result.HttpStatusCode;
        result.HttpStatusCode ??= System.Net.HttpStatusCode.Continue;
        return result;
    }

    /// <summary>
    /// Creates a successful <see cref="ResultBuilder{T}"/> instance.
    /// </summary>
    /// <param name="data">The result data.</param>
    /// <param name="statusCode">The HTTP status code (default: 200 OK).</param>
    public static ResultBuilder<T> Success(
        T? data,
        HttpStatusCode? statusCode = System.Net.HttpStatusCode.OK)
    {
        var result = new ResultBuilder<T>(true, statusCode, data);
        return result;
    }

    /// <summary>
    /// Creates a failed <see cref="ResultBuilder{T}"/> instance.
    /// </summary>
    /// <param name="problemDetails">Optional problem details.</param>
    /// <param name="statusCode">The HTTP status code (default: 400 Bad Request).</param>
    public static ResultBuilder<T> Failure(
        ProblemDetails? problemDetails = null,
        HttpStatusCode? statusCode = System.Net.HttpStatusCode.BadRequest)
    {
        var result = new ResultBuilder<T>(false, statusCode, default, problemDetails).WithEmptyProblemDetails();
        result.HttpStatusCode ??= (HttpStatusCode)(problemDetails?.Status ?? 100);
        if (problemDetails != null)
            problemDetails.Status ??= (int)result.HttpStatusCode;
        return result;
    }
}

/// <summary>
/// A non-generic version of <see cref="ResultBuilder{T}"/> for scenarios where result data is not needed.
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
    /// Creates a new <see cref="ResultBuilder"/> instance.
    /// </summary>
    /// <param name="succeeded">Indicates whether the operation succeeded.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="data">The result data (if any).</param>
    /// <param name="problemDetails">Optional problem details.</param>
    public new static ResultBuilder Create(
        bool succeeded,
        HttpStatusCode? statusCode,
        object? data = null,
        ProblemDetails? problemDetails = null)
    {
        var result = new ResultBuilder(succeeded, statusCode, data, problemDetails);
        result.HttpStatusCode ??= (HttpStatusCode)(problemDetails?.Status ?? 100);
        if (problemDetails != null) problemDetails.Status ??= (int)result.HttpStatusCode;
        result.HttpStatusCode ??= System.Net.HttpStatusCode.Continue;
        return result;
    }

    /// <summary>
    /// Creates a successful <see cref="ResultBuilder"/> instance with no result data.
    /// </summary>
    /// <param name="statusCode">The HTTP status code (default: 200 OK).</param>
    public static ResultBuilder Success(
        HttpStatusCode? statusCode = System.Net.HttpStatusCode.OK)
    {
        var result = new ResultBuilder(true, statusCode);
        return result;
    }

    /// <summary>
    /// Creates a failed <see cref="ResultBuilder"/> instance with optional problem details.
    /// </summary>
    /// <param name="problemDetails">Optional problem details.</param>
    /// <param name="statusCode">The HTTP status code (default: 400 Bad Request).</param>
    public new static ResultBuilder Failure(
        ProblemDetails? problemDetails = null,
        HttpStatusCode? statusCode = System.Net.HttpStatusCode.BadRequest)
    {
        var result = new ResultBuilder(false, statusCode, null, problemDetails).WithEmptyProblemDetails();
        result.HttpStatusCode ??= (HttpStatusCode)(problemDetails?.Status ?? 100);
        if (problemDetails != null) problemDetails.Status ??= (int)result.HttpStatusCode;
        return result;
    }
}
