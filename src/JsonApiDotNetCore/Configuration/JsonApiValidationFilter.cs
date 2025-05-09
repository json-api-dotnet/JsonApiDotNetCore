using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// Validation filter that blocks ASP.NET ModelState validation on data according to the JSON:API spec.
/// </summary>
internal sealed class JsonApiValidationFilter : IPropertyValidationFilter
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JsonApiValidationFilter(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);

        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public bool ShouldValidateEntry(ValidationEntry entry, ValidationEntry parentEntry)
    {
        if (entry.Metadata.MetadataKind == ModelMetadataKind.Type || IsId(entry.Key))
        {
            return true;
        }

        IServiceProvider serviceProvider = GetScopedServiceProvider();
        var request = serviceProvider.GetRequiredService<IJsonApiRequest>();
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
        return key == nameof(Identifiable<>.Id) || key.EndsWith($".{nameof(Identifiable<>.Id)}", StringComparison.Ordinal);
    }

    private static bool IsAtPrimaryEndpoint(IJsonApiRequest request)
    {
        return request.Kind is EndpointKind.Primary or EndpointKind.AtomicOperations;
    }

    private static bool IsFieldTargeted(ValidationEntry entry, ITargetedFields targetedFields)
    {
        // TODO: Consider compound attributes, ensure proper source pointer.
        return targetedFields.Attributes.Any(target => target.Attribute.Property.Name == entry.Key) ||
            targetedFields.Relationships.Any(relationship => relationship.Property.Name == entry.Key);
    }
}
