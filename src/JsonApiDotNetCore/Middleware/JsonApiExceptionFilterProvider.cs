using System;

namespace JsonApiDotNetCore.Middleware
{
    /// <inheritdoc/>
    public sealed class JsonApiExceptionFilterProvider : IJsonApiExceptionFilterProvider
    {
        public Type Get() => typeof(JsonApiExceptionFilter);
    }
}
