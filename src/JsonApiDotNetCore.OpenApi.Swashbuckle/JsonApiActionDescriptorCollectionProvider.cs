using System.Reflection;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

/// <summary>
/// Adds JsonApiDotNetCore metadata to <see cref="ControllerActionDescriptor" />s if available. This translates to updating response types in
/// <see cref="ProducesResponseTypeAttribute" /> and performing an expansion for secondary and relationship endpoints. For example:
/// <code><![CDATA[
/// /article/{id}/{relationshipName} -> /article/{id}/author, /article/{id}/revisions, etc.
/// ]]></code>
/// </summary>
internal sealed class JsonApiActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
{
    private static readonly string DefaultMediaType = JsonApiMediaType.Default.ToString();

    private readonly IActionDescriptorCollectionProvider _defaultProvider;
    private readonly JsonApiEndpointMetadataProvider _jsonApiEndpointMetadataProvider;

    public ActionDescriptorCollection ActionDescriptors => GetActionDescriptors();

    public JsonApiActionDescriptorCollectionProvider(IActionDescriptorCollectionProvider defaultProvider,
        JsonApiEndpointMetadataProvider jsonApiEndpointMetadataProvider)
    {
        ArgumentNullException.ThrowIfNull(defaultProvider);
        ArgumentNullException.ThrowIfNull(jsonApiEndpointMetadataProvider);

        _defaultProvider = defaultProvider;
        _jsonApiEndpointMetadataProvider = jsonApiEndpointMetadataProvider;
    }

    private ActionDescriptorCollection GetActionDescriptors()
    {
        var newDescriptors = _defaultProvider.ActionDescriptors.Items.ToList();
        var endpoints = newDescriptors.Where(IsVisibleJsonApiEndpoint).ToArray();

        foreach (var endpoint in endpoints)
        {
            var actionMethod = endpoint.GetActionMethod();
            var endpointMetadataContainer = _jsonApiEndpointMetadataProvider.Get(actionMethod);

            List<ActionDescriptor> replacementDescriptorsForEndpoint =
            [
                .. AddJsonApiMetadataToAction(endpoint, endpointMetadataContainer.RequestMetadata),
                .. AddJsonApiMetadataToAction(endpoint, endpointMetadataContainer.ResponseMetadata)
            ];

            if (replacementDescriptorsForEndpoint.Count > 0)
            {
                newDescriptors.InsertRange(newDescriptors.IndexOf(endpoint), replacementDescriptorsForEndpoint);
                newDescriptors.Remove(endpoint);
            }
        }

        var descriptorVersion = _defaultProvider.ActionDescriptors.Version;
        return new ActionDescriptorCollection(newDescriptors.AsReadOnly(), descriptorVersion);
    }

    internal static bool IsVisibleJsonApiEndpoint(ActionDescriptor descriptor)
    {
        // Only if in a convention ApiExplorer.IsVisible was set to false, the ApiDescriptionActionData will not be present.
        return descriptor is ControllerActionDescriptor controllerAction && controllerAction.Properties.ContainsKey(typeof(ApiDescriptionActionData));
    }

    private static List<ActionDescriptor> AddJsonApiMetadataToAction(ActionDescriptor endpoint, IJsonApiEndpointMetadata? jsonApiEndpointMetadata)
    {
        switch (jsonApiEndpointMetadata)
        {
            case PrimaryResponseMetadata primaryMetadata:
            {
                UpdateProducesResponseTypeAttribute(endpoint, primaryMetadata.DocumentType);
                return [];
            }
            case PrimaryRequestMetadata primaryMetadata:
            {
                UpdateBodyParameterDescriptor(endpoint, primaryMetadata.DocumentType, null);
                return [];
            }
            case NonPrimaryEndpointMetadata nonPrimaryEndpointMetadata and (RelationshipResponseMetadata or SecondaryResponseMetadata):
            {
                return Expand(endpoint, nonPrimaryEndpointMetadata,
                    (expandedEndpoint, documentType, _) => UpdateProducesResponseTypeAttribute(expandedEndpoint, documentType));
            }
            case NonPrimaryEndpointMetadata nonPrimaryEndpointMetadata and RelationshipRequestMetadata:
            {
                return Expand(endpoint, nonPrimaryEndpointMetadata, UpdateBodyParameterDescriptor);
            }
            case AtomicOperationsRequestMetadata:
            {
                UpdateBodyParameterDescriptor(endpoint, typeof(OperationsRequestDocument), null);
                return [];
            }
            case AtomicOperationsResponseMetadata:
            {
                UpdateProducesResponseTypeAttribute(endpoint, typeof(OperationsResponseDocument));
                return [];
            }
            default:
            {
                return [];
            }
        }
    }

    private static void UpdateProducesResponseTypeAttribute(ActionDescriptor endpoint, Type responseDocumentType)
    {
        ProducesResponseTypeAttribute? attribute = null;

        if (ProducesJsonApiResponseDocument(endpoint))
        {
            var producesResponse = endpoint.GetFilterMetadata<ProducesResponseTypeAttribute>();

            if (producesResponse != null)
            {
                attribute = producesResponse;
            }
        }

        ConsistencyGuard.ThrowIf(attribute == null);
        attribute.Type = responseDocumentType;
    }

    private static bool ProducesJsonApiResponseDocument(ActionDescriptor endpoint)
    {
        var produces = endpoint.GetFilterMetadata<ProducesAttribute>();

        if (produces != null)
        {
            foreach (var contentType in produces.ContentTypes)
            {
                if (MediaTypeHeaderValue.TryParse(contentType, out var headerValue))
                {
                    if (headerValue.MediaType.Equals(DefaultMediaType, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static List<ActionDescriptor> Expand(ActionDescriptor genericEndpoint, NonPrimaryEndpointMetadata metadata,
        Action<ActionDescriptor, Type, string> expansionCallback)
    {
        List<ActionDescriptor> expansion = [];

        foreach ((var relationshipName, var documentType) in metadata.DocumentTypesByRelationshipName)
        {
            if (genericEndpoint.AttributeRouteInfo == null)
            {
                throw new NotSupportedException("Only attribute routing is supported for JsonApiDotNetCore endpoints.");
            }

            var expandedEndpoint = Clone(genericEndpoint);

            RemovePathParameter(expandedEndpoint.Parameters, "relationshipName");

            ExpandTemplate(expandedEndpoint.AttributeRouteInfo!, relationshipName);

            expansionCallback(expandedEndpoint, documentType, relationshipName);

            expansion.Add(expandedEndpoint);
        }

        return expansion;
    }

    private static void UpdateBodyParameterDescriptor(ActionDescriptor endpoint, Type documentType, string? parameterName)
    {
        var requestBodyDescriptor = endpoint.GetBodyParameterDescriptor();

        if (requestBodyDescriptor == null)
        {
            var actionMethod = endpoint.GetActionMethod();

            throw new InvalidConfigurationException(
                $"The action method '{actionMethod}' on type '{actionMethod.ReflectedType?.FullName}' contains no parameter with a [FromBody] attribute.");
        }

        requestBodyDescriptor.ParameterType = documentType;
        requestBodyDescriptor.ParameterInfo = new ParameterInfoWrapper(requestBodyDescriptor.ParameterInfo, documentType, parameterName);
    }

    private static ActionDescriptor Clone(ActionDescriptor descriptor)
    {
        var clone = descriptor.MemberwiseClone();
        clone.AttributeRouteInfo = descriptor.AttributeRouteInfo!.MemberwiseClone();
        clone.FilterDescriptors = descriptor.FilterDescriptors.Select(Clone).ToList();
        clone.Parameters = descriptor.Parameters.Select(parameter => parameter.MemberwiseClone()).ToList();
        return clone;
    }

    private static FilterDescriptor Clone(FilterDescriptor descriptor)
    {
        var clone = descriptor.Filter.MemberwiseClone();

        return new FilterDescriptor(clone, descriptor.Scope)
        {
            Order = descriptor.Order
        };
    }

    private static void RemovePathParameter(ICollection<ParameterDescriptor> parameters, string parameterName)
    {
        var descriptor = parameters.Single(parameterDescriptor => parameterDescriptor.Name == parameterName);
        parameters.Remove(descriptor);
    }

    private static void ExpandTemplate(AttributeRouteInfo route, string expansionParameter)
    {
        route.Template = route.Template!.Replace("{relationshipName}", expansionParameter);
    }
}
