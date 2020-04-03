using System;
using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Exceptions
{
    public class JsonApiException : Exception
    {
        public Error Error { get; }

        public JsonApiException(Error error)
            : base(error.Title)
        {
            Error = error;
        }

        public JsonApiException(Error error, Exception innerException)
            : base(error.Title, innerException)
        {
            Error = error;
        }

        public JsonApiException(HttpStatusCode status, string message)
            : base(message)
        {
            Error = new Error(status)
            {
                Title = message
            };
        }

        public JsonApiException(HttpStatusCode status, string message, string detail)
            : base(message)
        {
            Error = new Error(status)
            {
                Title = message,
                Detail = detail
            };
        }
    }
}
