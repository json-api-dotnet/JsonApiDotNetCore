using System;

namespace JsonApiDotNetCore.Internal
{
    public class JsonApiException : Exception
    {
        private string _statusCode;
        private string _detail;
        private string _message;

        public JsonApiException(string statusCode, string message)
        : base(message)
        { 
            _statusCode = statusCode;
            _message = message;
        }

        public JsonApiException(string statusCode, string message, string detail)
        : base(message)
        { 
            _statusCode = statusCode;
            _message = message;
            _detail = detail;            
        }

        public Error GetError()
        {
            return new Error(_statusCode, _message, _detail);
        }
    }
}
