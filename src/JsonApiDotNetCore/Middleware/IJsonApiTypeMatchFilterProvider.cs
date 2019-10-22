using System;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Provides the type of the global action filter that is configured in MVC during startup.
    /// This can be overridden to let JADNC use your own action filter. The default action filter used
    /// is <see cref="DefaultTypeMatchFilter"/>
    /// </summary>
    public interface IJsonApiTypeMatchFilterProvider
    {
        Type Get();
    }

    /// <inheritdoc/>
    public class JsonApiTypeMatchFilterProvider : IJsonApiTypeMatchFilterProvider
    {
        public Type Get() => typeof(DefaultTypeMatchFilter);
    }
}
