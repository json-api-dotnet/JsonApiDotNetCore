using System;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when translating a <see cref="QueryLayer" /> to Entity Framework Core fails.
    /// </summary>
    [PublicAPI]
    public sealed class InvalidQueryException : JsonApiException
    {
        public InvalidQueryException(string reason, Exception exception)
            : base(new Error(HttpStatusCode.BadRequest)
            {
                Title = reason,
                Detail = exception?.Message
            }, exception)
        {
        }
    }
}
