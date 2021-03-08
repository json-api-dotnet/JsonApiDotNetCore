using System;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// An interface used to separate the registration of the global <see cref="IServiceProvider" /> from a request-scoped service provider. This is useful
    /// in cases when we need to manually resolve services from the request scope (e.g. operation processors).
    /// </summary>
    public interface IRequestScopedServiceProvider : IServiceProvider
    {
    }
}
