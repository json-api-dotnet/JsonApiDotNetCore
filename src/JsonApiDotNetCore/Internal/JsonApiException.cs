using System;

namespace JsonApiDotNetCore.Internal
{
    public class JsonApiException : Exception
    {
        private ErrorCollection _errors = new ErrorCollection();

        public JsonApiException(ErrorCollection errorCollection)
        { 
            _errors = errorCollection;
        }

        public JsonApiException(Error error)
        : base(error.Title)
        { 
            _errors.Add(error);
        }

        public JsonApiException(string statusCode, string message)
        : base(message)
        { 
            _errors.Add(new Error(statusCode, message, null));
        }

        public JsonApiException(string statusCode, string message, string detail)
        : base(message)
        { 
            _errors.Add(new Error(statusCode, message, detail));
        }

        public ErrorCollection GetError()
        {
            return _errors;
        }
    }
}
