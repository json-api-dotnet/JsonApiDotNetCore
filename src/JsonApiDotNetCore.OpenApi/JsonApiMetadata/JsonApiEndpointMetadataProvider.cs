using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.RelationshipData;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    /// <summary>
    /// Provides JsonApiDotNetCore related metadata for an ASP.NET controller action that can only be computed from the <see cref="ResourceGraph" /> at
    /// runtime.
    /// </summary>
    internal sealed class JsonApiEndpointMetadataProvider
    {
        private readonly IResourceGraph _resourceGraph;
        private readonly IControllerResourceMapping _controllerResourceMapping;
        private readonly EndpointResolver _endpointResolver = new();

        public JsonApiEndpointMetadataProvider(IResourceGraph resourceGraph, IControllerResourceMapping controllerResourceMapping)
        {
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(controllerResourceMapping, nameof(controllerResourceMapping));

            _resourceGraph = resourceGraph;
            _controllerResourceMapping = controllerResourceMapping;
        }

        public JsonApiEndpointMetadataContainer Get(MethodInfo controllerAction)
        {
            ArgumentGuard.NotNull(controllerAction, nameof(controllerAction));

            JsonApiEndpoint? endpoint = _endpointResolver.Get(controllerAction);

            if (endpoint == null)
            {
                throw new NotSupportedException($"Unable to provide metadata for non-JsonApiDotNetCore endpoint '{controllerAction.ReflectedType!.FullName}'.");
            }

            ResourceType? primaryResourceType = _controllerResourceMapping.GetResourceTypeForController(controllerAction.ReflectedType);

            if (primaryResourceType == null)
            {
                throw new UnreachableCodeException();
            }

            IJsonApiRequestMetadata? requestMetadata = GetRequestMetadata(endpoint.Value, primaryResourceType.ClrType);
            IJsonApiResponseMetadata? responseMetadata = GetResponseMetadata(endpoint.Value, primaryResourceType.ClrType);
            return new JsonApiEndpointMetadataContainer(requestMetadata, responseMetadata);
        }

        private IJsonApiRequestMetadata? GetRequestMetadata(JsonApiEndpoint endpoint, Type primaryResourceType)
        {
            switch (endpoint)
            {
                case JsonApiEndpoint.Post:
                {
                    return GetPostRequestMetadata(primaryResourceType);
                }
                case JsonApiEndpoint.Patch:
                {
                    return GetPatchRequestMetadata(primaryResourceType);
                }
                case JsonApiEndpoint.PostRelationship:
                case JsonApiEndpoint.PatchRelationship:
                case JsonApiEndpoint.DeleteRelationship:
                {
                    return GetRelationshipRequestMetadata(primaryResourceType, endpoint != JsonApiEndpoint.PatchRelationship);
                }
                default:
                {
                    return null;
                }
            }
        }

        private static PrimaryRequestMetadata GetPostRequestMetadata(Type primaryResourceType)
        {
            Type documentType = typeof(ResourcePostRequestDocument<>).MakeGenericType(primaryResourceType);

            return new PrimaryRequestMetadata(documentType);
        }

        private static PrimaryRequestMetadata GetPatchRequestMetadata(Type primaryResourceType)
        {
            Type documentType = typeof(ResourcePatchRequestDocument<>).MakeGenericType(primaryResourceType);

            return new PrimaryRequestMetadata(documentType);
        }

        private RelationshipRequestMetadata GetRelationshipRequestMetadata(Type primaryResourceType, bool ignoreHasOneRelationships)
        {
            IEnumerable<RelationshipAttribute> relationships = _resourceGraph.GetResourceType(primaryResourceType).Relationships;

            if (ignoreHasOneRelationships)
            {
                relationships = relationships.OfType<HasManyAttribute>();
            }

            IDictionary<string, Type> resourceTypesByRelationshipName = relationships.ToDictionary(relationship => relationship.PublicName,
                relationship => relationship is HasManyAttribute
                    ? typeof(ToManyRelationshipRequestData<>).MakeGenericType(relationship.RightType.ClrType)
                    : typeof(ToOneRelationshipRequestData<>).MakeGenericType(relationship.RightType.ClrType));

            return new RelationshipRequestMetadata(resourceTypesByRelationshipName);
        }

        private IJsonApiResponseMetadata? GetResponseMetadata(JsonApiEndpoint endpoint, Type primaryResourceType)
        {
            switch (endpoint)
            {
                case JsonApiEndpoint.GetCollection:
                case JsonApiEndpoint.GetSingle:
                case JsonApiEndpoint.Post:
                case JsonApiEndpoint.Patch:
                {
                    return GetPrimaryResponseMetadata(primaryResourceType, endpoint == JsonApiEndpoint.GetCollection);
                }
                case JsonApiEndpoint.GetSecondary:
                {
                    return GetSecondaryResponseMetadata(primaryResourceType);
                }
                case JsonApiEndpoint.GetRelationship:
                {
                    return GetRelationshipResponseMetadata(primaryResourceType);
                }
                default:
                {
                    return null;
                }
            }
        }

        private static PrimaryResponseMetadata GetPrimaryResponseMetadata(Type primaryResourceType, bool endpointReturnsCollection)
        {
            Type documentOpenType = endpointReturnsCollection ? typeof(ResourceCollectionResponseDocument<>) : typeof(PrimaryResourceResponseDocument<>);
            Type documentType = documentOpenType.MakeGenericType(primaryResourceType);

            return new PrimaryResponseMetadata(documentType);
        }

        private SecondaryResponseMetadata GetSecondaryResponseMetadata(Type primaryResourceType)
        {
            IDictionary<string, Type> responseTypesByRelationshipName = GetMetadataByRelationshipName(primaryResourceType, relationship =>
            {
                Type documentType = relationship is HasManyAttribute
                    ? typeof(ResourceCollectionResponseDocument<>)
                    : typeof(SecondaryResourceResponseDocument<>);

                return documentType.MakeGenericType(relationship.RightType.ClrType);
            });

            return new SecondaryResponseMetadata(responseTypesByRelationshipName);
        }

        private IDictionary<string, Type> GetMetadataByRelationshipName(Type primaryResourceType,
            Func<RelationshipAttribute, Type> extractRelationshipMetadataCallback)
        {
            IReadOnlyCollection<RelationshipAttribute> relationships = _resourceGraph.GetResourceType(primaryResourceType).Relationships;

            return relationships.ToDictionary(relationship => relationship.PublicName, extractRelationshipMetadataCallback);
        }

        private RelationshipResponseMetadata GetRelationshipResponseMetadata(Type primaryResourceType)
        {
            IDictionary<string, Type> responseTypesByRelationshipName = GetMetadataByRelationshipName(primaryResourceType,
                relationship => relationship is HasManyAttribute
                    ? typeof(ResourceIdentifierCollectionResponseDocument<>).MakeGenericType(relationship.RightType.ClrType)
                    : typeof(ResourceIdentifierResponseDocument<>).MakeGenericType(relationship.RightType.ClrType));

            return new RelationshipResponseMetadata(responseTypesByRelationshipName);
        }
    }
}
