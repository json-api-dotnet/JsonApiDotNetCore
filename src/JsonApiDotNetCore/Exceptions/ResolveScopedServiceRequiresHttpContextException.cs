using System;
using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Exceptions
{
    /// <summary>
    /// The error that is thrown when attempting to resolve an injected object instance that is scoped to a HTTP request, while no HTTP request is currently in progress.
    /// </summary>
    public sealed class ResolveScopedServiceRequiresHttpContextException : JsonApiException
    {
        public Type ServiceType { get; }

        public ResolveScopedServiceRequiresHttpContextException(Type serviceType)
            : base(new Error(HttpStatusCode.InternalServerError)
            {
                Title = "Cannot resolve scoped service outside the context of an HTTP request.",
                Detail =
                    $"Type requested was '{serviceType.FullName}'. If you are hitting this error in automated tests, you should instead inject your own " +
                    "IScopedServiceProvider implementation. See the GitHub repository for how we do this internally. " +
                    "https://github.com/json-api-dotnet/JsonApiDotNetCore/search?q=TestScopedServiceProvider&unscoped_q=TestScopedServiceProvider"
            })
        {
            ServiceType = serviceType;
        }
    }
}
