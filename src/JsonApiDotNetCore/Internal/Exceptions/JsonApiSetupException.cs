using System;

namespace JsonApiDotNetCore.Internal
{
    public sealed class JsonApiSetupException : Exception
    {
        public JsonApiSetupException(string message) 
            : base(message) { }

        public JsonApiSetupException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
