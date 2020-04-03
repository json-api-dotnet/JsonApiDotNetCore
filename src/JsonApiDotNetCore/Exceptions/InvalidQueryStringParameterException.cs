using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Exceptions
{
    /// <summary>
    /// The error that is thrown when parsing the request query string fails.
    /// </summary>
    public sealed class InvalidQueryStringParameterException : JsonApiException
    {
        public string QueryParameterName { get; }

        public InvalidQueryStringParameterException(string queryParameterName, string genericMessage,
            string specificMessage)
            : base(new Error(HttpStatusCode.BadRequest)
            {
                Title = genericMessage,
                Detail = specificMessage,
                Source =
                {
                    Parameter = queryParameterName
                }
            })
        {
            QueryParameterName = queryParameterName;
        }
    }
}
