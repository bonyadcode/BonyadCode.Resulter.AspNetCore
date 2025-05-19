using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BonyadCode.Resulter;

/// <summary>
/// Extension methods for converting <see cref="ResultBuilder"/> instances into HTTP results
/// for ASP.NET Core Controllers and Minimal APIs. Also provides ProblemDetails helpers.
/// </summary>
public static class ResultBuilderExtensions
{
    #region Controllers

    /// <summary>
    /// Converts a non-generic <see cref="ResultBuilder"/> into an <see cref="IActionResult"/> for ASP.NET Core Controllers.
    /// </summary>
    /// <param name="result">The result builder instance.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result.</returns>
    public static IActionResult ToHttpResultController(this ResultBuilder result) =>
        result.ToHttpResultController<object?>();

    /// <summary>
    /// Converts a generic <see cref="ResultBuilder{T}"/> into an <see cref="ObjectResult"/> for ASP.NET Core Controllers.
    /// </summary>
    /// <typeparam name="T">The type of the result data.</typeparam>
    /// <param name="result">The result builder instance.</param>
    /// <returns>An <see cref="ObjectResult"/> representing the result.</returns>
    public static ObjectResult ToHttpResultController<T>(this ResultBuilder<T> result)
    {
        return result.Succeeded
            ? new ObjectResult(result) { StatusCode = (int)(result.HttpStatusCode ?? HttpStatusCode.OK) }
            : new ObjectResult(result) { StatusCode = (int)(result.HttpStatusCode ?? HttpStatusCode.BadRequest) };
    }

    #endregion

    #region Minimal Api

    /// <summary>
    /// Converts a non-generic <see cref="ResultBuilder"/> into an <see cref="IResult"/> for Minimal APIs.
    /// </summary>
    /// <param name="result">The result builder instance.</param>
    /// <returns>An <see cref="IResult"/> representing the result.</returns>
    public static IResult ToHttpResultMinimal(this ResultBuilder result) =>
        result.ToHttpResultMinimal<object?>();

    /// <summary>
    /// Converts a generic <see cref="ResultBuilder{T}"/> into an <see cref="IResult"/> for Minimal APIs.
    /// </summary>
    /// <typeparam name="T">The type of the result data.</typeparam>
    /// <param name="result">The result builder instance.</param>
    /// <returns>An <see cref="IResult"/> representing the result.</returns>
    public static IResult ToHttpResultMinimal<T>(this ResultBuilder<T> result)
    {
        return result.Succeeded
            ? Results.Json(result, new JsonSerializerOptions(JsonSerializerDefaults.Web), "application/json",
                (int)(result.HttpStatusCode ?? HttpStatusCode.OK))
            : Results.Json(result, new JsonSerializerOptions(JsonSerializerDefaults.Web), "application/json",
                (int)(result.HttpStatusCode ?? HttpStatusCode.BadRequest));
    }

    #endregion

    #region Problem Details

    /// <summary>
    /// Attaches an empty <see cref="ProblemDetails"/> to a non-generic <see cref="ResultBuilder"/>.
    /// </summary>
    public static ResultBuilder WithEmptyProblemDetails(this ResultBuilder result) =>
        result.WithEmptyProblemDetails<object?>()
            .ToVoidResultBuilder();

    /// <summary>
    /// Attaches an empty <see cref="ProblemDetails"/> to a generic <see cref="ResultBuilder{T}"/>.
    /// </summary>
    public static ResultBuilder<T> WithEmptyProblemDetails<T>(this ResultBuilder<T> result)
    {
        result.ProblemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "A problem occured.",
            Detail = "A problem occured.",
            Status = (int)HttpStatusCode.BadRequest,
            Instance = null,
            Extensions = new Dictionary<string, object?>(),
        };

        return result;
    }

    /// <summary>
    /// Attaches custom <see cref="ProblemDetails"/> to a non-generic <see cref="ResultBuilder"/>.
    /// </summary>
    public static ResultBuilder WithDetailedProblemDetails(this ResultBuilder result,
        string? type,
        string? title,
        string? details,
        HttpStatusCode? statusCode,
        string? instance,
        Dictionary<string, string[]>? errors,
        IHttpContextAccessor? httpContext = null) =>
        result.WithDetailedProblemDetails<object?>(type, title, details, statusCode, instance, errors, httpContext)
            .ToVoidResultBuilder();

    /// <summary>
    /// Attaches custom <see cref="ProblemDetails"/> to a generic <see cref="ResultBuilder{T}"/>.
    /// </summary>
    public static ResultBuilder<T> WithDetailedProblemDetails<T>(this ResultBuilder<T> result,
        string? type,
        string? title,
        string? details,
        HttpStatusCode? statusCode,
        string? instance,
        Dictionary<string, string[]>? errors,
        IHttpContextAccessor? httpContext = null)
    {
        result.ProblemDetails = new ProblemDetails
        {
            Type = type ?? "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = title ?? "A problem occured.",
            Detail = details ?? "A problem occured.",
            Status = (int)(statusCode ?? HttpStatusCode.BadRequest),
            Instance = instance ?? httpContext?.HttpContext?.Request.Path,
            Extensions = errors?.ToDictionary(kvp => kvp.Key, object (kvp) => kvp.Value)!,
        };

        return result;
    }

    /// <summary>
    /// Adds additional error key-value pairs to the <see cref="ProblemDetails.Extensions"/>.
    /// </summary>
    public static ResultBuilder AddErrorExtensionsToProblemDetails(this ResultBuilder result,
        Dictionary<string, string[]> errors) =>
        result.AddErrorExtensionsToProblemDetails<object?>(errors)
            .ToVoidResultBuilder();

    /// <summary>
    /// Adds additional error key-value pairs to the <see cref="ProblemDetails.Extensions"/>.
    /// </summary>
    public static ResultBuilder<T> AddErrorExtensionsToProblemDetails<T>(this ResultBuilder<T> result,
        Dictionary<string, string[]> errors)
    {
        if (result.ProblemDetails == null) result.ProblemDetails = result.WithEmptyProblemDetails().ProblemDetails;
        foreach (var error in errors)
        {
            result.ProblemDetails?.Extensions.Add(error.Key, error.Value);
        }

        return result;
    }

    /// <summary>
    /// Attaches <see cref="ProblemDetails"/> to the result using exception details.
    /// </summary>
    public static ResultBuilder WithExceptionProblemDetails(this ResultBuilder result,
        Exception ex,
        IHttpContextAccessor? httpContext = null) =>
        result.WithExceptionProblemDetails<object?>(ex, httpContext)
            .ToVoidResultBuilder();

    /// <summary>
    /// Attaches <see cref="ProblemDetails"/> to the result using exception details.
    /// </summary>
    public static ResultBuilder<T> WithExceptionProblemDetails<T>(this ResultBuilder<T> result,
        Exception ex,
        IHttpContextAccessor? httpContext = null)
    {
        var errors = ErrorDictionaryHelper.ExceptionError(ex);
        result.ProblemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "An exception problem occured.",
            Detail = ex.Message,
            Status = (int)HttpStatusCode.InternalServerError,
            Instance = ex.Source ?? httpContext?.HttpContext?.Request.Path,
            Extensions = errors.ToDictionary(kvp => kvp.Key, object (kvp) => kvp.Value)!,
        };

        return result;
    }

    #endregion

    /// <summary>
    /// Converts a generic <see cref="ResultBuilder{T}"/> to a non-generic <see cref="ResultBuilder"/>.
    /// </summary>
    private static ResultBuilder ToVoidResultBuilder<T>(this ResultBuilder<T> result)
    {
        var voidResultBuilder =
            ResultBuilder.Create(result.Succeeded, result.HttpStatusCode, result.Data, result.ProblemDetails);
        return voidResultBuilder;
    }
}
