using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Internal.Exceptions
{
    public sealed class ActionResultException : JsonApiException
    {
        public ActionResultException(HttpStatusCode status) 
            : base(new Error(status)
        {
            Title = status.ToString()
        })
        {
        }

        public ActionResultException(ProblemDetails problemDetails)
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
