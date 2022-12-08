using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Errors;

/// <summary>
/// The error that is thrown when an <see cref="IActionResult" /> with non-success status is returned from a controller method.
/// </summary>
[PublicAPI]
public sealed class UnsuccessfulActionResultException : JsonApiException
{
    public UnsuccessfulActionResultException(HttpStatusCode status)
        : base(new ErrorObject(status)
        {
            Title = status.ToString()
        })
    {
    }

    public UnsuccessfulActionResultException(ProblemDetails problemDetails)
        : base(ToErrorObjects(problemDetails))
    {
    }

    private static IEnumerable<ErrorObject> ToErrorObjects(ProblemDetails problemDetails)
    {
        ArgumentGuard.NotNull(problemDetails);

        HttpStatusCode status = problemDetails.Status != null ? (HttpStatusCode)problemDetails.Status.Value : HttpStatusCode.InternalServerError;

        if (problemDetails is HttpValidationProblemDetails validationProblemDetails && validationProblemDetails.Errors.Any())
        {
            foreach (string errorMessage in validationProblemDetails.Errors.SelectMany(pair => pair.Value))
            {
                yield return ToErrorObject(status, validationProblemDetails, errorMessage);
            }
        }
        else
        {
            yield return ToErrorObject(status, problemDetails, problemDetails.Detail);
        }
    }

    private static ErrorObject ToErrorObject(HttpStatusCode status, ProblemDetails problemDetails, string detail)
    {
        var error = new ErrorObject(status)
        {
            Title = problemDetails.Title,
            Detail = detail
        };

        if (!string.IsNullOrWhiteSpace(problemDetails.Instance))
        {
            error.Id = problemDetails.Instance;
        }

        if (!string.IsNullOrWhiteSpace(problemDetails.Type))
        {
            error.Links ??= new ErrorLinks();
            error.Links.About = problemDetails.Type;
        }

        return error;
    }
}
