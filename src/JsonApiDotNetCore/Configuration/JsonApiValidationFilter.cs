using System;
using System.Linq;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Validation filter that blocks ASP.NET Core ModelState validation on data according to the JSON:API spec.
    /// </summary>
    internal sealed class JsonApiValidationFilter : IPropertyValidationFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JsonApiValidationFilter(IHttpContextAccessor httpContextAccessor)
        {
            ArgumentGuard.NotNull(httpContextAccessor, nameof(httpContextAccessor));

            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public bool ShouldValidateEntry(ValidationEntry entry, ValidationEntry parentEntry)
        {
            IServiceProvider serviceProvider = GetScopedServiceProvider();

            var request = serviceProvider.GetRequiredService<IJsonApiRequest>();

            if (IsId(entry.Key))
            {
                return true;
            }

            bool isTopResourceInPrimaryRequest = string.IsNullOrEmpty(parentEntry.Key) && IsAtPrimaryEndpoint(request);

            if (!isTopResourceInPrimaryRequest)
            {
                return false;
            }

            if (request.WriteOperation == WriteOperationKind.UpdateResource)
            {
                var targetedFields = serviceProvider.GetRequiredService<ITargetedFields>();
                return IsFieldTargeted(entry, targetedFields);
            }

            return true;
        }

        private IServiceProvider GetScopedServiceProvider()
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                throw new InvalidOperationException("Cannot resolve scoped services outside the context of an HTTP request.");
            }

            return httpContext.RequestServices;
        }

        private static bool IsId(string key)
        {
            return key == nameof(Identifiable<object>.Id) || key.EndsWith($".{nameof(Identifiable<object>.Id)}", StringComparison.Ordinal);
        }

        private static bool IsAtPrimaryEndpoint(IJsonApiRequest request)
        {
            return request.Kind is EndpointKind.Primary or EndpointKind.AtomicOperations;
        }

        private static bool IsFieldTargeted(ValidationEntry entry, ITargetedFields targetedFields)
        {
            return targetedFields.Attributes.Any(attribute => attribute.Property.Name == entry.Key) ||
                targetedFields.Relationships.Any(relationship => relationship.Property.Name == entry.Key);
        }
    }
}
