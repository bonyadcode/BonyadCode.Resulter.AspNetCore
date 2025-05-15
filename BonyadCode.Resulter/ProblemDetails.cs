using System.Net;
using System.Text.Json.Serialization;

namespace BonyadCode.Resulter;

/// <summary>
/// Provides configurable options for defining ProblemDetails metadata.
/// </summary>
public class ProblemDetails
{
    /// <summary>
    /// A URI that uniquely identifies the type of problem encountered.
    /// Default: "https://tools.ietf.org/html/rfc7231#section-6.5.1".
    /// </summary>
    public string? Type { get; set; } = "https://tools.ietf.org/html/rfc7231#section-6.5.1";

    /// <summary>
    /// A brief, human-readable summary of the problem.
    /// Default: "One or more validation errors occurred."
    /// </summary>
    public string? Title { get; set; } = "One or more validation errors occurred.";

    /// <summary>
    /// The HTTP status code associated with this problem instance.
    /// Default: 400 (Bad Request).
    /// </summary>
    public HttpStatusCode? Status { get; set; } = HttpStatusCode.BadRequest;

    /// <summary>
    /// A detailed explanation of the problem, providing additional context.
    /// Default: "One or more validation errors occurred."
    /// </summary>
    public string? Detail { get; set; } = "One or more validation errors occurred.";

    /// <summary>
    /// A URI reference to the specific occurrence of this problem.
    /// Typically set to the current request path.
    /// </summary>
    public string? Instance { get; set; }

    /// <summary>
    /// Additional key-value pairs for extending problem details with custom metadata.
    /// Typically, includes contextual data relevant to the request.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? Extensions { get; set; }
}