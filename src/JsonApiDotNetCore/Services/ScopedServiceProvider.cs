using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using JsonApiDotNetCore.Exceptions;

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
                throw new ResolveScopedServiceRequiresHttpContextException(serviceType);
            }

            return _httpContextAccessor.HttpContext.RequestServices.GetService(serviceType);
        }
    }
}
