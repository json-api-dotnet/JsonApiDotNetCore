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
            Error = new Error(status, message, null, GetMeta());
        }

        public JsonApiException(HttpStatusCode status, string message, string detail)
            : base(message)
        {
            Error = new Error(status, message, detail, GetMeta());
        }

        public JsonApiException(HttpStatusCode status, string message, Exception innerException)
            : base(message, innerException)
        {
            Error = new Error(status, message, innerException.Message, GetMeta(innerException));
        }

        private ErrorMeta GetMeta()
        {
            return ErrorMeta.FromException(this);
        }

        private ErrorMeta GetMeta(Exception e)
        {
            return ErrorMeta.FromException(e);
        }
    }
}
