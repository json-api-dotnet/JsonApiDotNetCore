using System;

namespace JsonApiDotNetCore.Middleware
{
    /// <inheritdoc />
    public class JsonApiTypeMatchFilterProvider : IJsonApiTypeMatchFilterProvider
    {
        public Type Get() => typeof(IncomingTypeMatchFilter);
    }
}
