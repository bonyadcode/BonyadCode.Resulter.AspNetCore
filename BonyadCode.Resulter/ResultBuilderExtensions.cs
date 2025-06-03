using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text.Json;

namespace BonyadCode.Resulter;

/// <summary>
/// Extension methods for converting result objects to HTTP results,
/// and attaching structured error metadata using ProblemDetails.
/// </summary>
public static partial class ResultBuilderExtensions
{
    // ------------------
    // Controller Support
    // ------------------

    /// <summary>
    /// Converts a non-generic result to IActionResult for use in controllers.
    /// </summary>
    public static IActionResult ToHttpResultController(this ResultBuilder result) =>
        result.ToHttpResultController<object?>();

    public static IActionResult ToHttpResultController(this ResultBuilder result, HttpStatusCode? statusCode) =>
        result.ToHttpResultController<object?>(statusCode);

    /// <summary>
    /// Converts a generic result to IActionResult for use in controllers.
    /// </summary>
    public static ObjectResult ToHttpResultController<T>(this ResultBuilder<T> result) =>
        new(result)
        {
            StatusCode = (int)(result.StatusCode ??
                               (result.Succeeded ? HttpStatusCode.OK : HttpStatusCode.BadRequest))
        };

    public static ObjectResult ToHttpResultController<T>(this ResultBuilder<T> result, HttpStatusCode? statusCode)
    {
        if (statusCode != null)
        {
            result.StatusCode = statusCode;
            if (result.ProblemDetails != null) result.ProblemDetails.Status = (int)statusCode;
        }

        return new ObjectResult(result)
        {
            StatusCode = (int)(statusCode ?? result.StatusCode ??
                (result.Succeeded ? HttpStatusCode.OK : HttpStatusCode.BadRequest))
        };
    }

    // ------------------
    // Minimal API Support
    // ------------------

    /// <summary>
    /// Converts a non-generic result to IResult for use in minimal APIs.
    /// </summary>
    public static IResult ToHttpResultMinimal(this ResultBuilder result) =>
        result.ToHttpResultMinimal<object?>();

    public static IResult ToHttpResultMinimal(this ResultBuilder result, HttpStatusCode? statusCode) =>
        result.ToHttpResultMinimal<object?>(statusCode);

    /// <summary>
    /// Converts a generic result to IResult for use in minimal APIs.
    /// </summary>
    public static IResult ToHttpResultMinimal<T>(this ResultBuilder<T> result) =>
        Results.Json(result,
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            contentType: "application/json",
            statusCode: (int)(result.StatusCode ??
                              (result.Succeeded ? HttpStatusCode.OK : HttpStatusCode.BadRequest)));

    public static IResult ToHttpResultMinimal<T>(this ResultBuilder<T> result, HttpStatusCode? statusCode)
    {
        if (statusCode != null)
        {
            result.StatusCode = statusCode;
            if (result.ProblemDetails != null) result.ProblemDetails.Status = (int)statusCode;
        }

        return Results.Json(result,
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            contentType: "application/json",
            statusCode: (int)(result.StatusCode ??
                              (result.Succeeded ? HttpStatusCode.OK : HttpStatusCode.BadRequest)));
    }

    /// <summary>
    /// Converts a generic result to a non-generic version.
    /// </summary>
    private static ResultBuilder ToVoidResultBuilder<T>(this ResultBuilder<T> result) =>
        ResultBuilder.Create(result.Succeeded, result.StatusCode, result.Data, result.ProblemDetails);
}

public static partial class ResultBuilderExtensions
{
    // ------------------
    // ProblemDetails Helpers
    // ------------------

    /// <summary>
    /// Adds a basic ProblemDetails object to a result.
    /// </summary>
    public static ResultBuilder WithSimpleProblemDetails(this ResultBuilder result) =>
        result.WithSimpleProblemDetails<object?>().ToVoidResultBuilder();

    /// <summary>
    /// Adds a basic ProblemDetails object to a generic result.
    /// </summary>
    public static ResultBuilder<T> WithSimpleProblemDetails<T>(this ResultBuilder<T> result)
    {
        result.ProblemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "A problem occurred.",
            Detail = "A problem occurred.",
            Status = (int)(result.StatusCode ?? HttpStatusCode.BadRequest),
            Extensions = new Dictionary<string, object?>()
        };
        return result;
    }

    /// <summary>
    /// Adds a customized ProblemDetails object to a result.
    /// </summary>
    public static ResultBuilder WithCustomProblemDetails(this ResultBuilder result,
        string? type,
        string? title,
        string? details,
        HttpStatusCode? statusCode,
        string? instance,
        Dictionary<string, object?>? errors,
        HttpContext? httpContext = null) =>
        result.WithCustomProblemDetails<object?>(type, title, details, statusCode, instance, errors, httpContext)
            .ToVoidResultBuilder();

    /// <summary>
    /// Adds a customized ProblemDetails object to a generic result.
    /// </summary>
    public static ResultBuilder<T> WithCustomProblemDetails<T>(this ResultBuilder<T> result,
        string? type,
        string? title,
        string? details,
        HttpStatusCode? statusCode,
        string? instance,
        Dictionary<string, object?>? errors,
        HttpContext? httpContext = null)
    {
        result.ProblemDetails = new ProblemDetails
        {
            Type = type ?? "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = title ?? "A problem occurred.",
            Detail = details ?? "A problem occurred.",
            Status = (int)(statusCode ?? HttpStatusCode.BadRequest),
            Instance = instance ?? httpContext?.Request.Path,
            Extensions = errors ?? new Dictionary<string, object?>()
        };
        return result;
    }

    /// <summary>
    /// Adds exception information to ProblemDetails.
    /// </summary>
    public static ResultBuilder WithExceptionProblemDetails(this ResultBuilder result,
        Exception ex,
        HttpContext? httpContext = null) =>
        result.WithExceptionProblemDetails<object?>(ex, httpContext)
            .ToVoidResultBuilder();

    /// <summary>
    /// Adds exception information to a generic result's ProblemDetails.
    /// </summary>
    public static ResultBuilder<T> WithExceptionProblemDetails<T>(this ResultBuilder<T> result,
        Exception ex,
        HttpContext? httpContext = null)
    {
        result = result.WithSimpleProblemDetails();
        foreach (var prop in ex.GetType().GetProperties())
        {
            var value = prop.GetValue(ex)?.ToString();
            if (value != null)
                result.ProblemDetails?.Extensions.TryAdd(prop.Name, value);
        }

        result.ProblemDetails!.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
        result.ProblemDetails.Title = "An exception was thrown";
        result.ProblemDetails.Detail = JsonConvert.SerializeObject(ex);
        result.ProblemDetails.Status = (int)(result.StatusCode ?? HttpStatusCode.InternalServerError);
        result.ProblemDetails.Instance = httpContext?.Request.Path ?? ex.StackTrace;
        return result;
    }
}

public static partial class ResultBuilderExtensions
{
    // ------------------
    // Error Extension Helpers
    // ------------------

    /// <summary>
    /// Adds validation error information using a single key and multiple values.
    /// </summary>
    public static ResultBuilder AddErrorsFromKeyValuePairs(this ResultBuilder result, string key,
        IList<string> valueList) =>
        result.AddErrorsFromKeyValuePairs<object?>(key, valueList).ToVoidResultBuilder();

    public static ResultBuilder<T> AddErrorsFromKeyValuePairs<T>(this ResultBuilder<T> result, string key,
        IList<string> valueList)
    {
        result = result.ProblemDetails == null ? result.WithSimpleProblemDetails() : result;
        result.ProblemDetails?.Extensions.TryAdd(key, valueList.ToArray());
        return result;
    }

    /// <summary>
    /// Adds a single key-value pair as error detail.
    /// </summary>
    public static ResultBuilder
        AddErrorsFromKeyValuePairs(this ResultBuilder result, string key, string value) =>
        result.AddErrorsFromKeyValuePairs<object?>(key, value).ToVoidResultBuilder();

    public static ResultBuilder<T> AddErrorsFromKeyValuePairs<T>(this ResultBuilder<T> result, string key,
        string value)
    {
        result = result.ProblemDetails == null ? result.WithSimpleProblemDetails() : result;
        result.ProblemDetails?.Extensions.TryAdd(key, new[] { value });
        return result;
    }

    /// <summary>
    /// Adds multiple keys sharing the same value.
    /// </summary>
    public static ResultBuilder AddErrorsFromKeyValuePairs(this ResultBuilder result, IList<string> keys,
        string value) =>
        result.AddErrorsFromKeyValuePairs<object?>(keys, value).ToVoidResultBuilder();

    public static ResultBuilder<T> AddErrorsFromKeyValuePairs<T>(this ResultBuilder<T> result,
        IList<string> keys, string value)
    {
        result = result.ProblemDetails == null ? result.WithSimpleProblemDetails() : result;
        foreach (var key in keys)
            result.ProblemDetails?.Extensions.TryAdd(key, new[] { value });
        return result;
    }

    /// <summary>
    /// Adds multiple error key-value pairs to ProblemDetails.Extensions, where each key at index `i` is mapped to the value at index `i`.
    /// </summary>
    public static ResultBuilder AddErrorsFromKeyValuePairs(this ResultBuilder result, IList<string> keys,
        IList<string> values) =>
        result.AddErrorsFromKeyValuePairs<object?>(keys, values).ToVoidResultBuilder();

    public static ResultBuilder<T> AddErrorsFromKeyValuePairs<T>(this ResultBuilder<T> result,
        IList<string> keys, IList<string> values)
    {
        result = result.ProblemDetails == null ? result.WithSimpleProblemDetails() : result;
        for (int i = 0; i < keys.Count; i++)
            result.ProblemDetails?.Extensions.TryAdd(keys[i], new[] { values[i] });
        return result;
    }

    /// <summary>
    /// Adds IdentityResult errors to the ProblemDetails extensions.
    /// </summary>
    public static ResultBuilder AddErrorsFromIdentityResult(this ResultBuilder result,
        IdentityResult identityResult) =>
        result.AddErrorsFromIdentityResult<object?>(identityResult).ToVoidResultBuilder();

    public static ResultBuilder<T> AddErrorsFromIdentityResult<T>(this ResultBuilder<T> result,
        IdentityResult identityResult)
    {
        result = result.ProblemDetails == null ? result.WithSimpleProblemDetails() : result;
        foreach (var error in identityResult.Errors)
            result.ProblemDetails?.Extensions.TryAdd(error.Code, new[] { error.Description });
        return result;
    }

    /// <summary>
    /// Adds FluentValidation results to the ProblemDetails extensions.
    /// using property names as keys and error messages as values.
    /// </summary>
    public static ResultBuilder AddErrorsFromFluentValidationResult(this ResultBuilder result,
        FluentValidation.Results.ValidationResult validationResult) =>
        result.AddErrorsFromFluentValidationResult<object?>(validationResult)
            .ToVoidResultBuilder();

    public static ResultBuilder<T> AddErrorsFromFluentValidationResult<T>(
        this ResultBuilder<T> result, FluentValidation.Results.ValidationResult validationResult)
    {
        result = result.ProblemDetails == null ? result.WithSimpleProblemDetails() : result;
        foreach (var error in validationResult.Errors)
        {
            var key = error.PropertyName.Contains('.')
                ? error.PropertyName.Split('.').Last()
                : error.PropertyName;
            result.ProblemDetails?.Extensions.TryAdd(key, new[] { error.ErrorMessage });
        }

        return result;
    }

    /// <summary>
    /// Adds DataAnnotations validation error.
    /// </summary>
    public static ResultBuilder AddErrorsFromValidationResult(this ResultBuilder result,
        System.ComponentModel.DataAnnotations.ValidationResult validationResult) =>
        result.AddErrorsFromValidationResult<object?>(validationResult).ToVoidResultBuilder();

    public static ResultBuilder<T> AddErrorsFromValidationResult<T>(this ResultBuilder<T> result,
        System.ComponentModel.DataAnnotations.ValidationResult validationResult)
    {
        result = result.ProblemDetails == null ? result.WithSimpleProblemDetails() : result;
        var key = validationResult.ErrorMessage ?? "Validation Error";
        result.ProblemDetails?.Extensions.TryAdd(key, validationResult.MemberNames.ToArray());
        return result;
    }

    /// <summary>
    /// Adds a list of invalid values for a specific property.
    /// </summary>
    public static ResultBuilder AddErrorsFromValidationError(this ResultBuilder result,
        string property, IList<string> values) =>
        result.AddErrorsFromValidationError<object?>(property, values).ToVoidResultBuilder();

    public static ResultBuilder<T> AddErrorsFromValidationError<T>(this ResultBuilder<T> result,
        string property, IList<string> values)
    {
        result = result.ProblemDetails == null ? result.WithSimpleProblemDetails() : result;
        result.ProblemDetails?.Extensions.TryAdd(property, values.ToArray());
        return result;
    }

    /// <summary>
    /// Adds all public properties of an exception as ProblemDetails extensions.
    /// </summary>
    public static ResultBuilder
        AddErrorsFromException(this ResultBuilder result, Exception ex) =>
        result.AddErrorsFromException<object?>(ex).ToVoidResultBuilder();

    public static ResultBuilder<T> AddErrorsFromException<T>(this ResultBuilder<T> result,
        Exception ex)
    {
        result = result.WithExceptionProblemDetails(ex);
        foreach (var property in ex.GetType().GetProperties())
            result.ProblemDetails?.Extensions.TryAdd(property.Name,
                new[] { property.GetValue(ex)?.ToString() ?? string.Empty });
        return result;
    }
}