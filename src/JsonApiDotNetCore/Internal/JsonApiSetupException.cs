using System;

namespace JsonApiDotNetCore.Internal
{
    public class JsonApiSetupException : Exception
    {
        public JsonApiSetupException(string message) 
            : base(message) { }

        public JsonApiSetupException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}