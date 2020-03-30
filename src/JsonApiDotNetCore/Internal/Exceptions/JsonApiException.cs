using System;
using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Internal
{
    public class JsonApiException : Exception
    {
        private readonly ErrorCollection _errors = new ErrorCollection();

        public JsonApiException(ErrorCollection errorCollection)
        {
            _errors = errorCollection;
        }

        public JsonApiException(Error error)
        : base(error.Title) => _errors.Add(error);
            
        public JsonApiException(HttpStatusCode status, string message, ErrorSource source = null)
        : base(message)
            => _errors.Add(new Error(status, message, null, GetMeta(), source));

        public JsonApiException(HttpStatusCode status, string message, string detail, ErrorSource source = null)
        : base(message)
            => _errors.Add(new Error(status, message, detail, GetMeta(), source));

        public JsonApiException(HttpStatusCode status, string message, Exception innerException)
        : base(message, innerException)
            => _errors.Add(new Error(status, message, innerException.Message, GetMeta(innerException)));

        public ErrorCollection GetErrors() => _errors;

        public HttpStatusCode GetStatusCode()
        {
            return _errors.GetErrorStatusCode();
        }

        private ErrorMeta GetMeta() => ErrorMeta.FromException(this);
        private ErrorMeta GetMeta(Exception e) => ErrorMeta.FromException(e);
    }
}
