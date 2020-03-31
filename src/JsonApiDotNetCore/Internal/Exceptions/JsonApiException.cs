using System;
using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Internal
{
    public class JsonApiException : Exception
    {
        public Error Error { get; }

        public JsonApiException(Error error)
            : base(error.Title)
        {
            Error = error;
        }

        public JsonApiException(HttpStatusCode status, string message)
            : base(message)
        {
            Error = new Error(status)
            {
                Title = message,
                Meta = CreateErrorMeta(this)
            };
        }

        public JsonApiException(HttpStatusCode status, string message, string detail)
            : base(message)
        {
            Error = new Error(status)
            {
                Title = message,
                Detail = detail,
                Meta = CreateErrorMeta(this)
            };
        }

        public JsonApiException(HttpStatusCode status, string message, Exception innerException)
            : base(message, innerException)
        {
            Error = new Error(status)
            {
                Title = message,
                Detail = innerException.Message,
                Meta = CreateErrorMeta(innerException)
            };
        }

        private static ErrorMeta CreateErrorMeta(Exception exception)
        {
            var meta = new ErrorMeta();
            meta.IncludeExceptionStackTrace(exception);
            return meta;
        }
    }
}
