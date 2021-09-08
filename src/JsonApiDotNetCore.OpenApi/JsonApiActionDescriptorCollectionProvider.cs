using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.OpenApi.JsonApiMetadata;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace JsonApiDotNetCore.OpenApi
{
    /// <summary>
    /// Adds JsonApiDotNetCore metadata to <see cref="ControllerActionDescriptor" />s if available. This translates to updating response types in
    /// <see cref="ProducesResponseTypeAttribute" /> and performing an expansion for secondary and relationship endpoints (eg
    /// /article/{id}/{relationshipName} -> /article/{id}/author, /article/{id}/revisions, etc).
    /// </summary>
    internal sealed class JsonApiActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
    {
        private readonly IActionDescriptorCollectionProvider _defaultProvider;
        private readonly JsonApiEndpointMetadataProvider _jsonApiEndpointMetadataProvider;

        public ActionDescriptorCollection ActionDescriptors => GetActionDescriptors();

        public JsonApiActionDescriptorCollectionProvider(IResourceGraph resourceGraph, IControllerResourceMapping controllerResourceMapping,
            IActionDescriptorCollectionProvider defaultProvider)
        {
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(controllerResourceMapping, nameof(controllerResourceMapping));
            ArgumentGuard.NotNull(defaultProvider, nameof(defaultProvider));

            _defaultProvider = defaultProvider;
            _jsonApiEndpointMetadataProvider = new JsonApiEndpointMetadataProvider(resourceGraph, controllerResourceMapping);
        }

        private ActionDescriptorCollection GetActionDescriptors()
        {
            List<ActionDescriptor> newDescriptors = _defaultProvider.ActionDescriptors.Items.ToList();
            List<ActionDescriptor> endpoints = newDescriptors.Where(IsVisibleJsonApiEndpoint).ToList();

            foreach (ActionDescriptor endpoint in endpoints)
            {
                JsonApiEndpointMetadataContainer endpointMetadataContainer = _jsonApiEndpointMetadataProvider.Get(endpoint.GetActionMethod());

                List<ActionDescriptor> replacementDescriptorsForEndpoint = new();
                replacementDescriptorsForEndpoint.AddRange(AddJsonApiMetadataToAction(endpoint, endpointMetadataContainer.RequestMetadata));
                replacementDescriptorsForEndpoint.AddRange(AddJsonApiMetadataToAction(endpoint, endpointMetadataContainer.ResponseMetadata));

                if (replacementDescriptorsForEndpoint.Any())
                {
                    newDescriptors.InsertRange(newDescriptors.IndexOf(endpoint) - 1, replacementDescriptorsForEndpoint);
                    newDescriptors.Remove(endpoint);
                }
            }

            int descriptorVersion = _defaultProvider.ActionDescriptors.Version;
            return new ActionDescriptorCollection(newDescriptors.AsReadOnly(), descriptorVersion);
        }

        private static bool IsVisibleJsonApiEndpoint(ActionDescriptor descriptor)
        {
            // Only if in a convention ApiExplorer.IsVisible was set to false, the ApiDescriptionActionData will not be present.
            return descriptor is ControllerActionDescriptor controllerAction && controllerAction.Properties.ContainsKey(typeof(ApiDescriptionActionData));
        }

        private static IList<ActionDescriptor> AddJsonApiMetadataToAction(ActionDescriptor endpoint, IJsonApiEndpointMetadata jsonApiEndpointMetadata)
        {
            switch (jsonApiEndpointMetadata)
            {
                case PrimaryResponseMetadata primaryMetadata:
                {
                    UpdateProducesResponseTypeAttribute(endpoint, primaryMetadata.Type);
                    return Array.Empty<ActionDescriptor>();
                }
                case PrimaryRequestMetadata primaryMetadata:
                {
                    UpdateBodyParameterDescriptor(endpoint, primaryMetadata.Type);
                    return Array.Empty<ActionDescriptor>();
                }
                case ExpansibleEndpointMetadata expansibleMetadata
                    when expansibleMetadata is RelationshipResponseMetadata || expansibleMetadata is SecondaryResponseMetadata:
                {
                    return Expand(endpoint, expansibleMetadata,
                        (expandedEndpoint, relationshipType, _) => UpdateProducesResponseTypeAttribute(expandedEndpoint, relationshipType));
                }
                case ExpansibleEndpointMetadata expansibleMetadata when expansibleMetadata is RelationshipRequestMetadata:
                {
                    return Expand(endpoint, expansibleMetadata, UpdateBodyParameterDescriptor);
                }
                default:
                {
                    return Array.Empty<ActionDescriptor>();
                }
            }
        }

        private static void UpdateProducesResponseTypeAttribute(ActionDescriptor endpoint, Type responseTypeToSet)
        {
            // TODO: is this check really adding anything? is the "else" ever hit, and if so, is that even meaningful?
            if (ProducesJsonApiResponseBody(endpoint))
            {
                var producesResponse = endpoint.GetFilterMetadata<ProducesResponseTypeAttribute>();
                producesResponse.Type = responseTypeToSet;
            }
        }

        private static bool ProducesJsonApiResponseBody(ActionDescriptor endpoint)
        {
            var produces = endpoint.GetFilterMetadata<ProducesAttribute>();

            return produces != null && produces.ContentTypes.Any(contentType => contentType == HeaderConstants.MediaType);
        }

        private static IList<ActionDescriptor> Expand(ActionDescriptor genericEndpoint, ExpansibleEndpointMetadata metadata,
            Action<ActionDescriptor, Type, string> expansionCallback)
        {
            var expansion = new List<ActionDescriptor>();

            foreach ((string relationshipName, Type relationshipType) in metadata.ExpansionElements)
            {
                ActionDescriptor expandedEndpoint = Clone(genericEndpoint);
                RemovePathParameter(expandedEndpoint.Parameters, JsonApiPathParameter.RelationshipName);
                ExpandTemplate(expandedEndpoint.AttributeRouteInfo, relationshipName);

                expansionCallback(expandedEndpoint, relationshipType, relationshipName);

                expansion.Add(expandedEndpoint);
            }

            return expansion;
        }

        private static void UpdateBodyParameterDescriptor(ActionDescriptor endpoint, Type bodyType, string parameterName = null)
        {
            ControllerParameterDescriptor requestBodyDescriptor = endpoint.GetBodyParameterDescriptor();
            requestBodyDescriptor.ParameterType = bodyType;
            ParameterInfo replacementParameterInfo = requestBodyDescriptor.ParameterInfo.WithParameterType(bodyType);

            if (parameterName != null)
            {
                replacementParameterInfo = replacementParameterInfo.WithName(parameterName);
            }

            requestBodyDescriptor.ParameterInfo = replacementParameterInfo;
        }

        private static ActionDescriptor Clone(ActionDescriptor descriptor)
        {
            var clonedDescriptor = (ActionDescriptor)descriptor.MemberwiseClone();

            clonedDescriptor.AttributeRouteInfo = (AttributeRouteInfo)descriptor.AttributeRouteInfo.MemberwiseClone();

            clonedDescriptor.FilterDescriptors = new List<FilterDescriptor>();

            foreach (FilterDescriptor filter in descriptor.FilterDescriptors)
            {
                clonedDescriptor.FilterDescriptors.Add(Clone(filter));
            }

            clonedDescriptor.Parameters = new List<ParameterDescriptor>();

            foreach (ParameterDescriptor parameter in descriptor.Parameters)
            {
                clonedDescriptor.Parameters.Add((ParameterDescriptor)parameter.MemberwiseClone());
            }

            return clonedDescriptor;
        }

        private static FilterDescriptor Clone(FilterDescriptor descriptor)
        {
            var clonedFilter = (IFilterMetadata)descriptor.Filter.MemberwiseClone();

            return new FilterDescriptor(clonedFilter, descriptor.Scope)
            {
                Order = descriptor.Order
            };
        }

        private static void RemovePathParameter(ICollection<ParameterDescriptor> parameters, string parameterName)
        {
            ParameterDescriptor relationshipName = parameters.Single(parameterDescriptor => parameterDescriptor.Name == parameterName);

            parameters.Remove(relationshipName);
        }

        private static void ExpandTemplate(AttributeRouteInfo route, string expansionParameter)
        {
            route.Template = route.Template!.Replace(JsonApiRoutingTemplate.RelationshipNameUrlPlaceholder, expansionParameter);
        }
    }
}
