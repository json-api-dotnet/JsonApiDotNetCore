using System;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when processing the request fails due to an error in the request query string.
    /// </summary>
    [PublicAPI]
    public sealed class InvalidQueryStringParameterException : JsonApiException
    {
        public string QueryParameterName { get; }

        public InvalidQueryStringParameterException(string queryParameterName, string genericMessage, string specificMessage, Exception innerException = null)
            : base(new Error(HttpStatusCode.BadRequest)
            {
                Title = genericMessage,
                Detail = specificMessage,
                Source =
                {
                    Parameter = queryParameterName
                }
            }, innerException)
        {
            QueryParameterName = queryParameterName;
        }
    }
}
