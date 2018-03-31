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

            // TODO: this is for mismatching type requests (e.g. posting an author to articles endpoint)
            // however, we can't actually guarantee that this is the source of this exception
            // we should probably use an action filter or when we improve the ContextGraph 
            // we might be able to skip most of deserialization entirely by checking the JToken
            // directly
            if (exceptionType == typeof(InvalidCastException))
                return new JsonApiException(409, exception.Message, exception);

            return new JsonApiException(500, exceptionType.Name, exception);
        }
    }
}
