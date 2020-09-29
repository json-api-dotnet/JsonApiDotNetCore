using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used with model state validation as a replacement for the built-in <see cref="RequiredAttribute"/> to support partial updates.
    /// </summary>
    public sealed class IsRequiredAttribute : RequiredAttribute
    {
        private const string _isSelfReferencingResourceKey = "JsonApiDotNetCore_IsSelfReferencingResource";

        public override bool RequiresValidationContext => true;

        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null) throw new ArgumentNullException(nameof(validationContext));

            var request = validationContext.GetRequiredService<IJsonApiRequest>();
            var httpContextAccessor = validationContext.GetRequiredService<IHttpContextAccessor>();

            if (ShouldSkipValidationForResource(validationContext, request, httpContextAccessor.HttpContext) ||
                ShouldSkipValidationForProperty(validationContext, httpContextAccessor.HttpContext))
            {
                return ValidationResult.Success;
            }

            return base.IsValid(value, validationContext);
        }

        private bool ShouldSkipValidationForResource(ValidationContext validationContext, IJsonApiRequest request,
            HttpContext httpContext)
        {
            if (request.Kind == EndpointKind.Primary)
            {
                // If there is a relationship included in the data of the POST or PATCH, then the 'IsRequired' attribute will be disabled for any
                // property within that object. For instance, a new article is posted and has a relationship included to an author. In this case,
                // the author name (which has the 'IsRequired' attribute) will not be included in the POST. Unless disabled, the POST will fail.

                if (validationContext.ObjectType != request.PrimaryResource.ResourceType)
                {
                    return true;
                }

                if (validationContext.ObjectInstance is IIdentifiable identifiable)
                {
                    if (identifiable.StringId != request.PrimaryId)
                    {
                        return true;
                    }

                    var isSelfReferencingResource = (bool?) httpContext.Items[_isSelfReferencingResourceKey];

                    if (isSelfReferencingResource == null)
                    {
                        // When processing a request, the first time we get here is for the top-level resource.
                        // Subsequent validations for related resources inspect the cache to know that their validation can be skipped.

                        isSelfReferencingResource = IsSelfReferencingResource(identifiable, validationContext);
                        httpContext.Items[_isSelfReferencingResourceKey] = isSelfReferencingResource;
                    }

                    if (isSelfReferencingResource.Value)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        private bool IsSelfReferencingResource(IIdentifiable identifiable, ValidationContext validationContext)
        {
            var provider = validationContext.GetRequiredService<IResourceContextProvider>();
            var relationships = provider.GetResourceContext(validationContext.ObjectType).Relationships;

            foreach (var relationship in relationships)
            {
                if (relationship is HasOneAttribute hasOne)
                {
                    var relationshipValue = (IIdentifiable) hasOne.GetValue(identifiable);
                    if (IdentifiableComparer.Instance.Equals(identifiable, relationshipValue))
                    {
                        return true;
                    }
                }

                if (relationship is HasManyAttribute hasMany)
                {
                    var collection = (IEnumerable) hasMany.GetValue(identifiable);

                    if (collection != null && collection.OfType<IIdentifiable>().Any(resource =>
                        IdentifiableComparer.Instance.Equals(identifiable, resource)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool ShouldSkipValidationForProperty(ValidationContext validationContext, HttpContext httpContext)
        {
            return httpContext.IsRequiredValidatorDisabled(validationContext.MemberName,
                validationContext.ObjectType.Name);
        }
    }
}
