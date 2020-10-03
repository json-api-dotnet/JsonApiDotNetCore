using System;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Validation filter that enables partial patching as part of the json:api spec.
    /// </summary>
    internal sealed class PartialPatchValidationFilter : IPropertyValidationFilter
    {
        /// <inheritdoc />
        public bool ShouldValidateEntry(ValidationEntry entry, ValidationEntry parentEntry) => true;

        public bool ShouldValidateEntry(ValidationEntry entry, ValidationEntry parentEntry, IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentException(nameof(serviceProvider));

            var request = serviceProvider.GetRequiredService<IJsonApiRequest>();
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var targetedFields = serviceProvider.GetRequiredService<ITargetedFields>();

            if (request.Kind == EndpointKind.Primary && string.IsNullOrEmpty(parentEntry.Key) && RequiredFieldIsTargeted(entry, targetedFields, httpContextAccessor))
            {
                return true;
            }
            
            return false;
        }

        private bool RequiredFieldIsTargeted(ValidationEntry entry, ITargetedFields targetedFields, IHttpContextAccessor httpContextAccessor)
        {
            var requestMethod = httpContextAccessor.HttpContext.Request.Method;
            
            if (requestMethod == HttpMethods.Post)
            {
                return true;
            }
            
            if (requestMethod == HttpMethods.Patch)
            {
                foreach (var attribute in targetedFields.Attributes)
                {
                    if (attribute.Property.Name == entry.Key)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
}
