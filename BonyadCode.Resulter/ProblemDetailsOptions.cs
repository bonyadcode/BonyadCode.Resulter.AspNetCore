namespace BonyadCode.Resulter;

/// <summary>
/// Options class to allow flexible customization of ProblemDetails metadata.
/// </summary>
public class ProblemDetailsOptions
{
    /// <summary>
    /// Custom title for the ProblemDetails (defaults to "One or more validation errors occurred.").
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Custom type for the ProblemDetails (defaults to "https://tools.ietf.org/html/rfc7231#section-6.5.1").
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Custom instance for the ProblemDetails (defaults to the current request path).
    /// </summary>
    public string? Instance { get; set; }
}