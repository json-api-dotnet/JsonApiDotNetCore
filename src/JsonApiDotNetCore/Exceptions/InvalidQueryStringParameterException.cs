using System;
using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Exceptions
{
    /// <summary>
    /// The error that is thrown when processing the request fails due to an error in the request query string.
    /// </summary>
    public sealed class InvalidQueryStringParameterException : JsonApiException
    {
        public string QueryParameterName { get; }

        public InvalidQueryStringParameterException(string queryParameterName, string genericMessage,
            string specificMessage)
            : this(queryParameterName, genericMessage, specificMessage, null)
        {
        }

        public InvalidQueryStringParameterException(string queryParameterName, string genericMessage,
            string specificMessage, Exception innerException)
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
