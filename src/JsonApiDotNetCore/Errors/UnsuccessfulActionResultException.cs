using System.Net;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when an <see cref="IActionResult"/> with non-success status is returned from a controller method.
    /// </summary>
    public sealed class UnsuccessfulActionResultException : JsonApiException
    {
        public UnsuccessfulActionResultException(HttpStatusCode status) 
            : base(new Error(status)
        {
            Title = status.ToString()
        })
        {
        }

        public UnsuccessfulActionResultException(ProblemDetails problemDetails)
            : base(ToError(problemDetails))
        {
        }

        private static Error ToError(ProblemDetails problemDetails)
        {
            var status = problemDetails.Status != null
                ? (HttpStatusCode) problemDetails.Status.Value
                : HttpStatusCode.InternalServerError;

            var error = new Error(status)
            {
                Title = problemDetails.Title,
                Detail = problemDetails.Detail
            };

            if (!string.IsNullOrWhiteSpace(problemDetails.Instance))
            {
                error.Id = problemDetails.Instance;
            }

            if (!string.IsNullOrWhiteSpace(problemDetails.Type))
            {
                error.Links.About = problemDetails.Type;
            }

            return error;
        }
    }
}
