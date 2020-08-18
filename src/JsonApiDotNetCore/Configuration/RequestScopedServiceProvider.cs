using System;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Configuration
{
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
