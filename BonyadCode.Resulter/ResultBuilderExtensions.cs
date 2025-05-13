using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BonyadCode.Resulter;

public static class ResultBuilderExtensions
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
            return new ObjectResult(result.Data) { StatusCode = result.StatusCode };

        var problem = BuildProblemDetails(result.Errors, result.StatusCode);
        return new ObjectResult(problem) { StatusCode = result.StatusCode };
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
            return Results.Json(result.Data, statusCode: result.StatusCode);

        var problem = BuildProblemDetails(result.Errors, result.StatusCode);
        return Results.Problem(problem);
    }

    #endregion

    #region Problem Details

    /// <summary>
    /// Helper method to build a ProblemDetails object for validation errors.
    /// </summary>
    private static ValidationProblemDetails BuildProblemDetails(
        Dictionary<string, string[]>? errors,
        int? statusCode,
        ProblemDetailsOptions? options = null)
    {
        options ??= new ProblemDetailsOptions(); // Use default options if none provided

        var problem = new ValidationProblemDetails(errors ?? new Dictionary<string, string[]>())
        {
            Title = options.Title ?? "One or more validation errors occurred.",
            Status = statusCode ?? StatusCodes.Status400BadRequest,
            Type = options.Type ?? "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            // If you don't pass a custom Instance, we will automatically set it to the path of the current request
            Instance = options.Instance ?? string.Empty
        };

        return problem;
    }

    #endregion
}