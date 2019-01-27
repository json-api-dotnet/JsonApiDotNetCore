using System;
using System.Linq;

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

        [Obsolete("Use int statusCode overload instead")]
        public JsonApiException(string statusCode, string message, string source = null)
        : base(message)
            => _errors.Add(new Error(statusCode, message, null, GetMeta(), source));

        [Obsolete("Use int statusCode overload instead")]
        public JsonApiException(string statusCode, string message, string detail, string source = null)
        : base(message)
            => _errors.Add(new Error(statusCode, message, detail, GetMeta(), source));

        public JsonApiException(int statusCode, string message, string source = null)
        : base(message)
            => _errors.Add(new Error(statusCode, message, null, GetMeta(), source));

        public JsonApiException(int statusCode, string message, string detail, string source = null)
        : base(message)
            => _errors.Add(new Error(statusCode, message, detail, GetMeta(), source));

        public JsonApiException(int statusCode, string message, Exception innerException)
        : base(message, innerException)
            => _errors.Add(new Error(statusCode, message, innerException.Message, GetMeta(innerException)));

        public ErrorCollection GetError() => _errors;

        public int GetStatusCode()
        {
            return _errors.GetErrorStatusCode();
        }

        private ErrorMeta GetMeta() => ErrorMeta.FromException(this);
        private ErrorMeta GetMeta(Exception e) => ErrorMeta.FromException(e);
    }
}
