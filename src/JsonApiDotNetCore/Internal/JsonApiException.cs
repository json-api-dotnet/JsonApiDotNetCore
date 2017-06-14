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
        : base(error.Title)
            => _errors.Add(error);

        [Obsolete("Use int statusCode overload instead")]
        public JsonApiException(string statusCode, string message)
        : base(message)
            => _errors.Add(new Error(statusCode, message, null));

        [Obsolete("Use int statusCode overload instead")]
        public JsonApiException(string statusCode, string message, string detail)
        : base(message)
            => _errors.Add(new Error(statusCode, message, detail));

        public JsonApiException(int statusCode, string message)
        : base(message)
            => _errors.Add(new Error(statusCode, message, null));

        public JsonApiException(int statusCode, string message, string detail)
        : base(message)
            => _errors.Add(new Error(statusCode, message, detail));

        public ErrorCollection GetError() => _errors;

        public int GetStatusCode()
        {
            if(_errors.Errors.Count == 1)
                return _errors.Errors[0].StatusCode;

            if(_errors.Errors.FirstOrDefault(e => e.StatusCode >= 500) != null)
                return 500;
                
            if(_errors.Errors.FirstOrDefault(e => e.StatusCode >= 400) != null)
                return 400;
            
            return 500;
        }
    }
}
