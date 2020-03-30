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

        public JsonApiException(HttpStatusCode status, string message, ErrorSource source = null)
            : base(message)
        {
            Error = new Error(status, message, null, GetMeta(), source);
        }

        public JsonApiException(HttpStatusCode status, string message, string detail, ErrorSource source = null)
            : base(message)
        {
            Error = new Error(status, message, detail, GetMeta(), source);
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
