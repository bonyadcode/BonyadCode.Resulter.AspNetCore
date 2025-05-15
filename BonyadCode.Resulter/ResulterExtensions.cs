using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BonyadCode.Resulter;

public static class ResulterExtensions
{
    #region Controllers

    /// <summary>
    /// Converts ResultBuilder into IActionResult (Controllers).
    /// </summary>
    public static IActionResult ToHttpResultController(this ResultBuilder result) =>
        result.ToHttpResultController<object?>();

    /// <summary>
    /// Converts ResultBuilder into IActionResult (Controllers).
    /// </summary>
    private static ObjectResult ToHttpResultController<T>(this ResultBuilder<T> result)
    {
        if (result.Succeeded)
            return new ObjectResult(result.Data) { StatusCode = (int)result.HttpStatusCode };

        var problem = BuildProblemDetails(result.HttpStatusCode, result.ProblemDetails, (int)result.HttpStatusCode);
        return new ObjectResult(problem) { StatusCode = (int)result.HttpStatusCode };
    }

    #endregion

    #region Minimal Api

    /// <summary>
    /// Converts ResultBuilder into IResult (Minimal APIs).
    /// </summary>
    public static IResult ToHttpResultMinimal(this ResultBuilder result) =>
        result.ToHttpResultMinimal<object?>();

    /// <summary>
    /// Converts ResultBuilder into IResult (Minimal APIs).
    /// </summary>
    private static IResult ToHttpResultMinimal<T>(this ResultBuilder<T> result)
    {
        if (result.Succeeded)
            return Results.Json(result.Data, result.HttpStatusCode);

        var problem = BuildProblemDetails(result.HttpStatusCode, result.ProblemDetails, result.HttpStatusCode);
        return Results.Problem(problem);
    }

    #endregion

    #region Problem Details

    /// <summary>
    /// Helper method to build a ProblemDetails object for validation errors.
    /// </summary>
    private static ValidationProblemDetails BuildProblemDetails(
        HttpStatusCode? statusCode,
        ProblemDetails? problemDetails = null)
    {
        problemDetails ??= new ProblemDetails(); // Use default options if none provided

        var problem = new ValidationProblemDetails(new Dictionary<string, string[]>())
        {
            Type = problemDetails.Type ?? "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = problemDetails.Title ?? "A problem occured.",
            Detail = problemDetails.Detail ?? "One or more validation errors occurred.",
            Status = (int?)statusCode,
            Instance = problemDetails.Instance ?? httpContext.Request.Path, // Auto-set request path if Instance is null
            Extensions = problemDetails.Extensions?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };

        return problem;
    }

    #endregion
}