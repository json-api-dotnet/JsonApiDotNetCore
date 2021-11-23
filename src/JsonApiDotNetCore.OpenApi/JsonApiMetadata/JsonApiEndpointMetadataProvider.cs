using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.JsonApiObjects;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    /// <summary>
    /// Provides JsonApiDotNetCore related metadata for an ASP.NET controller action that can only be computed from the <see cref="ResourceGraph" /> at
    /// runtime.
    /// </summary>
    internal sealed class JsonApiEndpointMetadataProvider
    {
        private readonly IControllerResourceMapping _controllerResourceMapping;
        private readonly EndpointResolver _endpointResolver = new();

        public JsonApiEndpointMetadataProvider(IControllerResourceMapping controllerResourceMapping)
        {
            ArgumentGuard.NotNull(controllerResourceMapping, nameof(controllerResourceMapping));

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

            IJsonApiRequestMetadata? requestMetadata = GetRequestMetadata(endpoint.Value, primaryResourceType);
            IJsonApiResponseMetadata? responseMetadata = GetResponseMetadata(endpoint.Value, primaryResourceType);
            return new JsonApiEndpointMetadataContainer(requestMetadata, responseMetadata);
        }

        private IJsonApiRequestMetadata? GetRequestMetadata(JsonApiEndpoint endpoint, ResourceType primaryResourceType)
        {
            switch (endpoint)
            {
                case JsonApiEndpoint.Post:
                {
                    return GetPostRequestMetadata(primaryResourceType.ClrType);
                }
                case JsonApiEndpoint.Patch:
                {
                    return GetPatchRequestMetadata(primaryResourceType.ClrType);
                }
                case JsonApiEndpoint.PostRelationship:
                case JsonApiEndpoint.PatchRelationship:
                case JsonApiEndpoint.DeleteRelationship:
                {
                    return GetRelationshipRequestMetadata(primaryResourceType.Relationships, endpoint != JsonApiEndpoint.PatchRelationship);
                }
                default:
                {
                    return null;
                }
            }
        }

        private static PrimaryRequestMetadata GetPostRequestMetadata(Type resourceClrType)
        {
            Type documentType = typeof(ResourcePostRequestDocument<>).MakeGenericType(resourceClrType);

            return new PrimaryRequestMetadata(documentType);
        }

        private static PrimaryRequestMetadata GetPatchRequestMetadata(Type resourceClrType)
        {
            Type documentType = typeof(ResourcePatchRequestDocument<>).MakeGenericType(resourceClrType);

            return new PrimaryRequestMetadata(documentType);
        }

        private static RelationshipRequestMetadata GetRelationshipRequestMetadata(IEnumerable<RelationshipAttribute> relationships,
            bool ignoreHasOneRelationships)
        {
            IEnumerable<RelationshipAttribute> relationshipsOfEndpoint = ignoreHasOneRelationships ? relationships.OfType<HasManyAttribute>() : relationships;

            IDictionary<string, Type> requestDocumentTypesByRelationshipName = relationshipsOfEndpoint.ToDictionary(relationship => relationship.PublicName,
                NonPrimaryDocumentTypeFactory.Instance.GetForRelationshipRequest);

            return new RelationshipRequestMetadata(requestDocumentTypesByRelationshipName);
        }

        private IJsonApiResponseMetadata? GetResponseMetadata(JsonApiEndpoint endpoint, ResourceType primaryResourceType)
        {
            switch (endpoint)
            {
                case JsonApiEndpoint.GetCollection:
                case JsonApiEndpoint.GetSingle:
                case JsonApiEndpoint.Post:
                case JsonApiEndpoint.Patch:
                {
                    return GetPrimaryResponseMetadata(primaryResourceType.ClrType, endpoint == JsonApiEndpoint.GetCollection);
                }
                case JsonApiEndpoint.GetSecondary:
                {
                    return GetSecondaryResponseMetadata(primaryResourceType.Relationships);
                }
                case JsonApiEndpoint.GetRelationship:
                {
                    return GetRelationshipResponseMetadata(primaryResourceType.Relationships);
                }
                default:
                {
                    return null;
                }
            }
        }

        private static PrimaryResponseMetadata GetPrimaryResponseMetadata(Type resourceClrType, bool endpointReturnsCollection)
        {
            Type documentOpenType = endpointReturnsCollection ? typeof(ResourceCollectionResponseDocument<>) : typeof(PrimaryResourceResponseDocument<>);
            Type documentType = documentOpenType.MakeGenericType(resourceClrType);

            return new PrimaryResponseMetadata(documentType);
        }

        private static SecondaryResponseMetadata GetSecondaryResponseMetadata(IEnumerable<RelationshipAttribute> relationships)
        {
            IDictionary<string, Type> responseDocumentTypesByRelationshipName = relationships.ToDictionary(relationship => relationship.PublicName,
                NonPrimaryDocumentTypeFactory.Instance.GetForSecondaryResponse);

            return new SecondaryResponseMetadata(responseDocumentTypesByRelationshipName);
        }

        private static RelationshipResponseMetadata GetRelationshipResponseMetadata(IEnumerable<RelationshipAttribute> relationships)
        {
            IDictionary<string, Type> responseDocumentTypesByRelationshipName = relationships.ToDictionary(relationship => relationship.PublicName,
                NonPrimaryDocumentTypeFactory.Instance.GetForRelationshipResponse);

            return new RelationshipResponseMetadata(responseDocumentTypesByRelationshipName);
        }
    }
}
