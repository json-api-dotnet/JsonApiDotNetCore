using Microsoft.AspNetCore.Http;
using System;

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// An interface used to separate the registration of the global ServiceProvider
    /// from a request scoped service provider. This is useful in cases when we need to 
    /// manually resolve services from the request scope (e.g. operation processors)
    /// </summary>
    public interface IScopedServiceProvider : IServiceProvider { }

    /// <summary>
    /// A service provider that uses the current HttpContext request scope
    /// </summary>
    public sealed class RequestScopedServiceProvider : IScopedServiceProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestScopedServiceProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public object GetService(Type serviceType)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                throw new InvalidOperationException(
                    $"Cannot resolve scoped service '{serviceType.FullName}' outside the context of an HTTP request. " +
                    "If you are hitting this error in automated tests, you should instead inject your own " +
                    "IScopedServiceProvider implementation. See the GitHub repository for how we do this internally. " +
                    "https://github.com/json-api-dotnet/JsonApiDotNetCore/search?q=TestScopedServiceProvider&unscoped_q=TestScopedServiceProvider");
            }

            return _httpContextAccessor.HttpContext.RequestServices.GetService(serviceType);
        }
    }
}
