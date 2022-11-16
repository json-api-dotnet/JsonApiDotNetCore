using System;
using System.Net;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Internal
{
    public static class JsonApiExceptionFactory
    {
        public static JsonApiException GetException(Exception exception)
        {
            var exceptionType = exception.GetType();

            if (exceptionType == typeof(JsonApiException))
                return (JsonApiException)exception;

            return new JsonApiException(new Error(HttpStatusCode.BadRequest)
            {
                Title = exceptionType.Name
            });
        }
    }
}
