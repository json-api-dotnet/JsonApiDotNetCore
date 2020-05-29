using System;
using System.Net;
using JsonApiDotNetCore.Internal.Queries;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Exceptions
{
    /// <summary>
    /// The error that is thrown when translating a <see cref="QueryLayer"/> to Entity Framework Core fails.
    /// </summary>
    public sealed class InvalidQueryException : JsonApiException
    {
        public InvalidQueryException(string reason, Exception exception)
            : base(new Error(HttpStatusCode.BadRequest)
            {
                Title = reason,
                Detail = exception.Message
            }, exception)
        {
        }
    }
}
