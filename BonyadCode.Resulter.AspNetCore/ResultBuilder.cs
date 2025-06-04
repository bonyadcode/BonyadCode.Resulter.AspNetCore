using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BonyadCode.Resulter.AspNetCore
{
    /// <summary>
    /// Represents the outcome of an operation, optionally carrying data and structured problem details.
    /// </summary>
    /// <typeparam name="T">The type of data returned by the operation.</typeparam>
    public class ResultBuilder<T>(
        bool succeeded,
        HttpStatusCode? statusCode,
        T? data = default,
        ProblemDetails? problemDetails = null)
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation succeeded.
        /// </summary>
        public bool Succeeded { get; set; } = succeeded;

        /// <summary>
        /// Gets or sets the HTTP status code associated with the result.
        /// If not set explicitly, defaults are derived from success or problem details.
        /// </summary>
        public HttpStatusCode? StatusCode { get; set; } = statusCode;

        /// <summary>
        /// Gets or sets the data returned by the operation.
        /// </summary>
        public T? Data { get; set; } = data;

        /// <summary>
        /// Gets or sets structured problem details describing errors or failure information.
        /// </summary>
        public ProblemDetails? ProblemDetails { get; set; } = problemDetails;

        /// <summary>
        /// Implicitly converts a data instance into a successful <see cref="ResultBuilder{T}"/>.
        /// </summary>
        /// <param name="data">The result data.</param>
        public static implicit operator ResultBuilder<T>(T data) => Success(data);

        /// <summary>
        /// Creates a new <see cref="ResultBuilder{T}"/> instance.
        /// </summary>
        /// <param name="succeeded">Indicates success of the operation.</param>
        /// <param name="statusCode">Optional HTTP status code.</param>
        /// <param name="data">Optional result data.</param>
        /// <param name="problemDetails">Optional problem details.</param>
        /// <returns>A new <see cref="ResultBuilder{T}"/> instance.</returns>
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
        /// Returns a successful <see cref="ResultBuilder{T}"/> containing the specified data.
        /// </summary>
        /// <param name="data">The result data.</param>
        /// <param name="statusCode">Optional HTTP status code (default is 200 OK).</param>
        /// <returns>A successful result.</returns>
        public static ResultBuilder<T> Success(
            T? data,
            HttpStatusCode? statusCode = HttpStatusCode.OK)
        {
            return new ResultBuilder<T>(true, statusCode, data);
        }

        /// <summary>
        /// Returns a failure <see cref="ResultBuilder{T}"/>, optionally including problem details.
        /// </summary>
        /// <param name="problemDetails">Optional structured problem details describing the failure.</param>
        /// <param name="statusCode">Optional HTTP status code (default is 400 Bad Request).</param>
        /// <returns>A failure result.</returns>
        public static ResultBuilder<T> Failure(
            ProblemDetails? problemDetails = null,
            HttpStatusCode? statusCode = HttpStatusCode.BadRequest)
        {
            var result = new ResultBuilder<T>(false, statusCode, default, problemDetails)
                .AddSimpleProblemDetails();
            result.StatusCode ??= (HttpStatusCode)(problemDetails?.Status ?? 400);
            if (problemDetails != null) problemDetails.Status ??= (int)result.StatusCode;
            result.StatusCode ??= HttpStatusCode.BadRequest;
            return result;
        }
    }

    /// <summary>
    /// A non-generic <see cref="ResultBuilder"/> variant for operations with no data.
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
        /// Creates a new <see cref="ResultBuilder"/> instance.
        /// </summary>
        /// <param name="succeeded">Indicates success of the operation.</param>
        /// <param name="statusCode">Optional HTTP status code.</param>
        /// <param name="data">Optional data (ignored in this class).</param>
        /// <param name="problemDetails">Optional problem details.</param>
        /// <returns>A new <see cref="ResultBuilder"/> instance.</returns>
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
        /// Returns a successful <see cref="ResultBuilder"/> instance.
        /// </summary>
        /// <param name="statusCode">Optional HTTP status code (default is 200 OK).</param>
        /// <returns>A successful result.</returns>
        public static ResultBuilder Success(
            HttpStatusCode? statusCode = HttpStatusCode.OK)
        {
            return new ResultBuilder(true, statusCode);
        }

        /// <summary>
        /// Returns a failed <see cref="ResultBuilder"/>, optionally including problem details.
        /// </summary>
        /// <param name="problemDetails">Optional structured problem details describing the failure.</param>
        /// <param name="statusCode">Optional HTTP status code (default is 400 Bad Request).</param>
        /// <returns>A failure result.</returns>
        public new static ResultBuilder Failure(
            ProblemDetails? problemDetails = null,
            HttpStatusCode? statusCode = HttpStatusCode.BadRequest)
        {
            var result = new ResultBuilder(false, statusCode, null, problemDetails)
                .AddSimpleProblemDetails();
            result.StatusCode ??= (HttpStatusCode)(problemDetails?.Status ?? 400);
            if (problemDetails != null) problemDetails.Status ??= (int)result.StatusCode;
            result.StatusCode ??= HttpStatusCode.BadRequest;
            return result;
        }
    }
}