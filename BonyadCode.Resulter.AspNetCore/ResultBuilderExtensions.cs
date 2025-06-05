using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text.Json;

namespace BonyadCode.Resulter.AspNetCore;

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
    public static IActionResult ToHttpResultController(this ResultBuilder result,
        HttpContext? httpContext = null) =>
        result.ToHttpResultController<object?>(httpContext);

    public static IActionResult ToHttpResultController(this ResultBuilder result,
        HttpStatusCode? statusCode,
        HttpContext? httpContext = null) =>
        result.ToHttpResultController<object?>(statusCode, httpContext);

    /// <summary>
    /// Converts a generic result to IActionResult for use in controllers.
    /// </summary>
    public static ObjectResult ToHttpResultController<T>(this ResultBuilder<T> result,
        HttpContext? httpContext = null)
    {
        if (result.ProblemDetails != null) result.ProblemDetails.Status = (int?)result.StatusCode;
        if (result.ProblemDetails != null) result.ProblemDetails.Instance = httpContext?.Request.Path ?? null;

        return new ObjectResult(result)
        {
            StatusCode = (int)(result.StatusCode ??
                               (result.Succeeded ? HttpStatusCode.OK : HttpStatusCode.BadRequest))
        };
    }

    public static ObjectResult ToHttpResultController<T>(this ResultBuilder<T> result,
        HttpStatusCode? statusCode,
        HttpContext? httpContext = null)
    {
        if (statusCode != null)
        {
            result.StatusCode = statusCode;
            if (result.ProblemDetails != null) result.ProblemDetails.Status = (int)statusCode;
        }

        if (result.ProblemDetails != null) result.ProblemDetails.Instance = httpContext?.Request.Path ?? null;

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
    public static IResult ToHttpResultMinimal(this ResultBuilder result,
        HttpContext? httpContext = null) =>
        result.ToHttpResultMinimal<object?>(httpContext);

    public static IResult ToHttpResultMinimal(this ResultBuilder result,
        HttpStatusCode? statusCode,
        HttpContext? httpContext = null) =>
        result.ToHttpResultMinimal<object?>(statusCode, httpContext);

    /// <summary>
    /// Converts a generic result to IResult for use in minimal APIs.
    /// </summary>
    public static IResult ToHttpResultMinimal<T>(this ResultBuilder<T> result,
        HttpContext? httpContext = null)
    {
        if (result.ProblemDetails != null) result.ProblemDetails.Status = (int?)result.StatusCode;
        if (result.ProblemDetails != null) result.ProblemDetails.Instance = httpContext?.Request.Path ?? null;

        return Results.Json(result,
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            contentType: "application/json",
            statusCode: (int)(result.StatusCode ??
                              (result.Succeeded ? HttpStatusCode.OK : HttpStatusCode.BadRequest)));
    }

    public static IResult ToHttpResultMinimal<T>(this ResultBuilder<T> result,
        HttpStatusCode? statusCode,
        HttpContext? httpContext = null)
    {
        if (statusCode != null)
        {
            result.StatusCode = statusCode;
            if (result.ProblemDetails != null) result.ProblemDetails.Status = (int)statusCode;
        }

        if (result.ProblemDetails != null) result.ProblemDetails.Instance = httpContext?.Request.Path ?? null;
        
        return Results.Json(result,
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            contentType: "application/json",
            statusCode: (int)(result.StatusCode ??
                              (result.Succeeded ? HttpStatusCode.OK : HttpStatusCode.BadRequest)));
    }

    /// <summary>
    /// Converts a typed ResultBuilder to a void ResultBuilder.
    /// Preserves Data as (object) type 
    /// </summary>
    public static ResultBuilder ToVoidResultBuilder<TSource>(this ResultBuilder<TSource> result) =>
        ResultBuilder.Create(result.Succeeded, result.StatusCode, result.Data, result.ProblemDetails);

    /// <summary>
    /// Converts a void ResultBuilder to a typed ResultBuilder.
    /// Preserves Data only if the source Data is type of T.
    /// </summary>
    public static ResultBuilder<TTarget> ToTypedResultBuilder<TTarget>(this ResultBuilder result) =>
        ResultBuilder<TTarget>.Create(result.Succeeded, result.StatusCode, result.Data is TTarget typedData ? typedData : default,
            result.ProblemDetails);

    /// <summary>
    /// Converts a source typed ResultBuilder to a target typed ResultBuilder.
    /// Preserves Data only if the TSource and TTarget are of the same Type. Use like this: ResultBuilder&lt;TTarget, TSource> e.g. ResultBuilder&lt;string, int>
    /// </summary>
    public static ResultBuilder<TTarget> ToTypedResultBuilder<TTarget, TSource>(this ResultBuilder<TSource> result) =>
        ResultBuilder<TTarget>.Create(result.Succeeded, result.StatusCode,
            result.Data is TTarget typedData ? typedData : default, result.ProblemDetails);
}

public static partial class ResultBuilderExtensions
{
    private const string ExtensionsKey = "extensions";

    // ------------------
    // ProblemDetails Helpers
    // ------------------

    /// <summary>
    /// Adds a basic ProblemDetails object to a result.
    /// </summary>
    public static ResultBuilder AddSimpleProblemDetails(this ResultBuilder result) =>
        result.AddSimpleProblemDetails<object?>().ToVoidResultBuilder();

    public static ResultBuilder<T> AddSimpleProblemDetails<T>(this ResultBuilder<T> result)
    {
        result.ProblemDetails = GetProblemDetailsFromHttpStatusCode(result.StatusCode);
        result.ProblemDetails.Status = (int)(result.StatusCode ?? HttpStatusCode.BadRequest);
        result.ProblemDetails.Instance = null;
        if (!result.ProblemDetails.Extensions.ContainsKey(ExtensionsKey))
            result.ProblemDetails.Extensions[ExtensionsKey] = new Dictionary<string, object?>();

        return result;
    }

    /// <summary>
    /// Adds a customized ProblemDetails object to a result.
    /// </summary>
    public static ResultBuilder AddCustomProblemDetails(this ResultBuilder result,
        string? type,
        string? title,
        string? detail,
        HttpStatusCode? statusCode,
        string? instance,
        Dictionary<string, object?>? extensions) =>
        result.AddCustomProblemDetails<object?>(type, title, detail, statusCode, instance, extensions)
            .ToVoidResultBuilder();

    public static ResultBuilder<T> AddCustomProblemDetails<T>(this ResultBuilder<T> result,
        string? type,
        string? title,
        string? detail,
        HttpStatusCode? statusCode,
        string? instance,
        Dictionary<string, object?>? extensions)
    {
        result.AddSimpleProblemDetails();
        if (result.ProblemDetails != null)
        {
            result.ProblemDetails.Type = type ?? result.ProblemDetails.Type;
            result.ProblemDetails.Title = title ?? result.ProblemDetails.Title;
            result.ProblemDetails.Detail = detail ?? result.ProblemDetails.Detail;
            result.ProblemDetails.Status = statusCode != null ? (int)statusCode : result.ProblemDetails.Status;
            result.ProblemDetails.Instance = instance ?? result.ProblemDetails.Instance;

            if (extensions != null)
            {
                if (!result.ProblemDetails.Extensions.TryGetValue(ExtensionsKey, out var extObj) ||
                    extObj is not Dictionary<string, object?> extDict)
                {
                    extDict = new Dictionary<string, object?>();
                    result.ProblemDetails.Extensions[ExtensionsKey] = extDict;
                }

                foreach (var kvp in extensions)
                    extDict[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }

    /// <summary>
    /// Adds exception information to ProblemDetails.
    /// Adds all public properties of an exception as ProblemDetails extensions.
    /// </summary>
    public static ResultBuilder AddProblemDetailsFromException(this ResultBuilder result,
        Exception exception) =>
        result.AddProblemDetailsFromException<object?>(exception)
            .ToVoidResultBuilder();

    public static ResultBuilder<T> AddProblemDetailsFromException<T>(this ResultBuilder<T> result,
        Exception exception)
    {
        result = result.AddSimpleProblemDetails();
        if (result.ProblemDetails != null)
        {
            result.ProblemDetails.Title = "A Server problem occurred.";

            result.ProblemDetails.Detail = JsonConvert.SerializeObject(exception);
            result.ProblemDetails.Status = (int)(result.StatusCode ?? HttpStatusCode.InternalServerError);
            result.ProblemDetails.Instance = exception.StackTrace;

            if (!result.ProblemDetails.Extensions.TryGetValue(ExtensionsKey, out var extObj) ||
                extObj is not Dictionary<string, object?> extDict)
            {
                extDict = new Dictionary<string, object?>();
                result.ProblemDetails.Extensions[ExtensionsKey] = extDict;
            }

            foreach (var property in exception.GetType().GetProperties())
            {
                var value = property.GetValue(exception)?.ToString();
                if (value != null)
                    extDict.TryAdd(property.Name, new[] { value });
            }
        }

        return result;
    }
}

public static partial class ResultBuilderExtensions
{
    // ------------------
    // ProblemDetails Helpers
    // ------------------

    /// <summary>
    /// Adds IdentityResult errors to the ProblemDetails extensions.
    /// </summary>
    public static ResultBuilder AddProblemDetailsFromIdentityResult(this ResultBuilder result,
        IdentityResult identityResult) =>
        result.AddProblemDetailsFromIdentityResult<object?>(identityResult).ToVoidResultBuilder();

    public static ResultBuilder<T> AddProblemDetailsFromIdentityResult<T>(this ResultBuilder<T> result,
        IdentityResult identityResult)
    {
        result = result.ProblemDetails == null ? result.AddSimpleProblemDetails() : result;
        if (result.ProblemDetails != null)
        {
            result.ProblemDetails.Title = "An Identity problem occurred.";
            result.ProblemDetails.Title = "One or more validation failures occured.";

            if (!result.ProblemDetails.Extensions.TryGetValue(ExtensionsKey, out var extObj) ||
                extObj is not Dictionary<string, object?> extDict)
            {
                extDict = new Dictionary<string, object?>();
                result.ProblemDetails.Extensions[ExtensionsKey] = extDict;
            }

            foreach (var error in identityResult.Errors)
                extDict.TryAdd(error.Code, new[] { error.Description });
        }

        return result;
    }

    /// <summary>
    /// Adds FluentValidation results to the ProblemDetails extensions.
    /// using property names as keys and error messages as values.
    /// </summary>
    public static ResultBuilder AddProblemDetailsFromFluentValidationResult(this ResultBuilder result,
        FluentValidation.Results.ValidationResult validationResult) =>
        result.AddProblemDetailsFromFluentValidationResult<object?>(validationResult)
            .ToVoidResultBuilder();

    public static ResultBuilder<T> AddProblemDetailsFromFluentValidationResult<T>(
        this ResultBuilder<T> result,
        FluentValidation.Results.ValidationResult validationResult)
    {
        result = result.ProblemDetails == null ? result.AddSimpleProblemDetails() : result;
        if (result.ProblemDetails != null)
        {
            result.ProblemDetails.Title = "A Validation problem occurred.";
            result.ProblemDetails.Title = "One or more validation failures occured.";

            if (!result.ProblemDetails.Extensions.TryGetValue(ExtensionsKey, out var extObj) ||
                extObj is not Dictionary<string, object?> extDict)
            {
                extDict = new Dictionary<string, object?>();
                result.ProblemDetails.Extensions[ExtensionsKey] = extDict;
            }

            foreach (var error in validationResult.Errors)
            {
                var key = error.PropertyName.Contains('.')
                    ? error.PropertyName.Split('.').Last()
                    : error.PropertyName;
                extDict.TryAdd(key, new[] { error.ErrorMessage });
            }
        }

        return result;
    }

    /// <summary>
    /// Adds DataAnnotations validation error.
    /// </summary>
    public static ResultBuilder AddProblemDetailsFromValidationResult(this ResultBuilder result,
        System.ComponentModel.DataAnnotations.ValidationResult validationResult) =>
        result.AddProblemDetailsFromValidationResult<object?>(validationResult).ToVoidResultBuilder();

    public static ResultBuilder<T> AddProblemDetailsFromValidationResult<T>(this ResultBuilder<T> result,
        System.ComponentModel.DataAnnotations.ValidationResult validationResult)
    {
        result = result.ProblemDetails == null ? result.AddSimpleProblemDetails() : result;
        if (result.ProblemDetails != null)
        {
            result.ProblemDetails.Title = "A Validation problem occurred.";
            result.ProblemDetails.Title = "One or more validation failures occured.";

            if (!result.ProblemDetails.Extensions.TryGetValue(ExtensionsKey, out var extObj) ||
                extObj is not Dictionary<string, object?> extDict)
            {
                extDict = new Dictionary<string, object?>();
                result.ProblemDetails.Extensions[ExtensionsKey] = extDict;
            }

            var key = validationResult.ErrorMessage ?? "Validation Error";
            extDict.TryAdd(key, validationResult.MemberNames.ToArray());
        }

        return result;
    }
}

public static partial class ResultBuilderExtensions
{
    // ------------------
    // ProblemDetails Helpers
    // ------------------

    /// <summary>
    /// Adds a single key-value pair as error detail.
    /// </summary>
    public static ResultBuilder
        AddProblemDetailsFromKeyValuePairs(this ResultBuilder result,
            string key,
            string value) =>
        result.AddProblemDetailsFromKeyValuePairs<object?>(key, value).ToVoidResultBuilder();

    public static ResultBuilder<T> AddProblemDetailsFromKeyValuePairs<T>(this ResultBuilder<T> result,
        string key,
        string value)
    {
        result = result.ProblemDetails == null ? result.AddSimpleProblemDetails() : result;
        if (result.ProblemDetails != null)
        {
            if (!result.ProblemDetails.Extensions.TryGetValue(ExtensionsKey, out var extObj) ||
                extObj is not Dictionary<string, object?> extDict)
            {
                extDict = new Dictionary<string, object?>();
                result.ProblemDetails.Extensions[ExtensionsKey] = extDict;
            }

            extDict.TryAdd(key, new[] { value });
        }

        return result;
    }

    /// <summary>
    /// Adds validation error information using a single key and multiple values.
    /// </summary>
    public static ResultBuilder AddProblemDetailsFromKeyValuePairs(this ResultBuilder result,
        string key,
        IList<string> values) =>
        result.AddProblemDetailsFromKeyValuePairs<object?>(key, values).ToVoidResultBuilder();

    public static ResultBuilder<T> AddProblemDetailsFromKeyValuePairs<T>(this ResultBuilder<T> result,
        string key,
        IList<string> values)
    {
        result = result.ProblemDetails == null ? result.AddSimpleProblemDetails() : result;
        if (result.ProblemDetails != null)
        {
            if (!result.ProblemDetails.Extensions.TryGetValue(ExtensionsKey, out var extObj) ||
                extObj is not Dictionary<string, object?> extDict)
            {
                extDict = new Dictionary<string, object?>();
                result.ProblemDetails.Extensions[ExtensionsKey] = extDict;
            }

            extDict.TryAdd(key, values.ToArray());
        }

        return result;
    }

    /// <summary>
    /// Adds multiple keys sharing the same value.
    /// </summary>
    public static ResultBuilder AddProblemDetailsFromKeyValuePairs(this ResultBuilder result,
        IList<string> keys,
        string value) =>
        result.AddProblemDetailsFromKeyValuePairs<object?>(keys, value).ToVoidResultBuilder();

    public static ResultBuilder<T> AddProblemDetailsFromKeyValuePairs<T>(this ResultBuilder<T> result,
        IList<string> keys,
        string value)
    {
        result = result.ProblemDetails == null ? result.AddSimpleProblemDetails() : result;
        if (result.ProblemDetails != null)
        {
            if (!result.ProblemDetails.Extensions.TryGetValue(ExtensionsKey, out var extObj) ||
                extObj is not Dictionary<string, object?> extDict)
            {
                extDict = new Dictionary<string, object?>();
                result.ProblemDetails.Extensions[ExtensionsKey] = extDict;
            }

            foreach (var key in keys)
                extDict.TryAdd(key, new[] { value });
        }

        return result;
    }

    /// <summary>
    /// Adds multiple error key-value pairs to ProblemDetails.Extensions, where each key at index `i` is mapped to the value at index `i`.
    /// </summary>
    public static ResultBuilder AddProblemDetailsFromKeyValuePairs(this ResultBuilder result,
        IList<string> keys,
        IList<string> values) =>
        result.AddProblemDetailsFromKeyValuePairs<object?>(keys, values).ToVoidResultBuilder();

    public static ResultBuilder<T> AddProblemDetailsFromKeyValuePairs<T>(this ResultBuilder<T> result,
        IList<string> keys,
        IList<string> values)
    {
        result = result.ProblemDetails == null ? result.AddSimpleProblemDetails() : result;
        if (result.ProblemDetails != null)
        {
            if (!result.ProblemDetails.Extensions.TryGetValue(ExtensionsKey, out var extObj) ||
                extObj is not Dictionary<string, object?> extDict)
            {
                extDict = new Dictionary<string, object?>();
                result.ProblemDetails.Extensions[ExtensionsKey] = extDict;
            }

            for (var i = 0; i < keys.Count; i++)
                extDict.TryAdd(keys[i], new[] { values[i] });
        }

        return result;
    }
}

public static partial class ResultBuilderExtensions
{
    // ------------------
    // ProblemDetails Helpers
    // ------------------

    private static ProblemDetails GetProblemDetailsFromHttpStatusCode(
        HttpStatusCode? httpStatusCode = HttpStatusCode.BadRequest)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "A problem occurred.",
            Detail = $"A problem occurred ({httpStatusCode} {httpStatusCode.ToString()}).",
            Type = GetHttpStatusType(httpStatusCode)
        };

        return problemDetails;
    }

    private static string GetHttpStatusType(HttpStatusCode? httpStatusCode)
    {
        return httpStatusCode switch
        {
            // Informational (1xx)
            HttpStatusCode.Continue => "https://tools.ietf.org/html/rfc7231#section-6.2.1",
            HttpStatusCode.SwitchingProtocols => "https://tools.ietf.org/html/rfc7231#section-6.2.2",
            HttpStatusCode.Processing => "https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml",
            HttpStatusCode.EarlyHints => "https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml",

            // Success (2xx)
            HttpStatusCode.OK => "https://tools.ietf.org/html/rfc7231#section-6.3.1",
            HttpStatusCode.Created => "https://tools.ietf.org/html/rfc7231#section-6.3.2",
            HttpStatusCode.Accepted => "https://tools.ietf.org/html/rfc7231#section-6.3.3",
            HttpStatusCode.NonAuthoritativeInformation => "https://tools.ietf.org/html/rfc7231#section-6.3.4",
            HttpStatusCode.NoContent => "https://tools.ietf.org/html/rfc7231#section-6.3.5",
            HttpStatusCode.ResetContent => "https://tools.ietf.org/html/rfc7231#section-6.3.6",
            HttpStatusCode.PartialContent => "https://tools.ietf.org/html/rfc7233#section-4.1",
            HttpStatusCode.MultiStatus => "https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml",
            HttpStatusCode.AlreadyReported =>
                "https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml",
            HttpStatusCode.IMUsed => "https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml",

            // Redirection (3xx)
            HttpStatusCode.MultipleChoices => "https://tools.ietf.org/html/rfc7231#section-6.4.1",
            HttpStatusCode.MovedPermanently => "https://tools.ietf.org/html/rfc7231#section-6.4.2",
            HttpStatusCode.Found => "https://tools.ietf.org/html/rfc7231#section-6.4.3",
            HttpStatusCode.SeeOther => "https://tools.ietf.org/html/rfc7231#section-6.4.4",
            HttpStatusCode.NotModified => "https://tools.ietf.org/html/rfc7232#section-4.1",
            HttpStatusCode.UseProxy => "https://tools.ietf.org/html/rfc7231#section-6.4.5",
            HttpStatusCode.TemporaryRedirect => "https://tools.ietf.org/html/rfc7231#section-6.4.7",
            HttpStatusCode.PermanentRedirect =>
                "https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml",

            // Client Errors (4xx)
            HttpStatusCode.BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            HttpStatusCode.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
            HttpStatusCode.PaymentRequired => "https://tools.ietf.org/html/rfc7231#section-6.5.2",
            HttpStatusCode.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            HttpStatusCode.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            HttpStatusCode.MethodNotAllowed => "https://tools.ietf.org/html/rfc7231#section-6.5.5",
            HttpStatusCode.NotAcceptable => "https://tools.ietf.org/html/rfc7231#section-6.5.6",
            HttpStatusCode.ProxyAuthenticationRequired => "https://tools.ietf.org/html/rfc7235#section-3.2",
            HttpStatusCode.RequestTimeout => "https://tools.ietf.org/html/rfc7231#section-6.5.7",
            HttpStatusCode.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            HttpStatusCode.Gone => "https://tools.ietf.org/html/rfc7231#section-6.5.9",
            HttpStatusCode.LengthRequired => "https://tools.ietf.org/html/rfc7231#section-6.5.10",
            HttpStatusCode.PreconditionFailed => "https://tools.ietf.org/html/rfc7232#section-4.2",
            // HttpStatusCode.PayloadTooLarge => "https://tools.ietf.org/html/rfc7231#section-6.5.11",
            // HttpStatusCode.URITooLong => "https://tools.ietf.org/html/rfc7231#section-6.5.12",
            HttpStatusCode.UnsupportedMediaType => "https://tools.ietf.org/html/rfc7231#section-6.5.13",
            HttpStatusCode.ExpectationFailed => "https://tools.ietf.org/html/rfc7231#section-6.5.14",
            HttpStatusCode.MisdirectedRequest =>
                "https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml",
            HttpStatusCode.UnprocessableEntity =>
                "https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml",
            HttpStatusCode.Locked => "https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml",
            HttpStatusCode.FailedDependency =>
                "https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml",
            // HttpStatusCode.TooEarly => "https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml",
            HttpStatusCode.UpgradeRequired =>
                "https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml",
            HttpStatusCode.PreconditionRequired => "https://datatracker.ietf.org/doc/html/rfc6585",
            HttpStatusCode.TooManyRequests => "https://datatracker.ietf.org/doc/html/rfc6585",
            HttpStatusCode.RequestHeaderFieldsTooLarge => "https://datatracker.ietf.org/doc/html/rfc6585",
            HttpStatusCode.UnavailableForLegalReasons =>
                "https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml",

            // Server Errors (5xx)
            HttpStatusCode.InternalServerError => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            HttpStatusCode.NotImplemented => "https://tools.ietf.org/html/rfc7231#section-6.6.2",
            HttpStatusCode.BadGateway => "https://tools.ietf.org/html/rfc7231#section-6.6.3",
            HttpStatusCode.ServiceUnavailable => "https://tools.ietf.org/html/rfc7231#section-6.6.4",
            HttpStatusCode.GatewayTimeout => "https://tools.ietf.org/html/rfc7231#section-6.6.5",
            // HttpStatusCode.HTTPVersionNotSupported => "https://tools.ietf.org/html/rfc7231#section-6.6.6",

            _ => "https://tools.ietf.org/html/rfc7231"
        };
    }
}