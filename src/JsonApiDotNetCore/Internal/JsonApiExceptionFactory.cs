using System;
using System.Linq;

namespace JsonApiDotNetCore.Internal
{
    public static class JsonApiExceptionFactory
    {
        public static JsonApiException GetException(Exception exception)
        {
            var exceptionType = exception.GetType().ToString().Split('.').Last();
            switch(exceptionType)
            {
                case "JsonApiException":
                    return (JsonApiException)exception;
                case "InvalidCastException":
                    return new JsonApiException("409", exception.Message);
                default:
                    return new JsonApiException("500", exception.Message);
            }
        }
    }
}
