using System;

namespace JsonApiDotNetCore.Internal
{
    public static class JsonApiExceptionFactory
    {
        public static JsonApiException GetException(Exception exception)
        {
            var exceptionType = exception.GetType();

            if (exceptionType == typeof(JsonApiException))
                return (JsonApiException)exception;

            return new JsonApiException(500, exceptionType.Name, exception);
        }
    }
}
