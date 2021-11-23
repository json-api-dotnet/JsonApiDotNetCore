using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

namespace JsonApiDotNetCore.OpenApi
{
    internal sealed class JsonApiSchemaIdSelector
    {
        private static readonly IDictionary<Type, string> OpenTypeToSchemaTemplateMap = new Dictionary<Type, string>
        {
            [typeof(ResourcePostRequestDocument<>)] = "###-post-request-document",
            [typeof(ResourcePatchRequestDocument<>)] = "###-patch-request-document",
            [typeof(ResourcePostRequestObject<>)] = "###-data-in-post-request",
            [typeof(ResourcePatchRequestObject<>)] = "###-data-in-patch-request",
            [typeof(ToOneRelationshipInRequest<>)] = "to-one-###-data-in-request",
            [typeof(NullableToOneRelationshipInRequest<>)] = "nullable-to-one-###-data-in-request",
            [typeof(ToManyRelationshipInRequest<>)] = "to-many-###-data-in-request",
            [typeof(PrimaryResourceResponseDocument<>)] = "###-primary-response-document",
            [typeof(SecondaryResourceResponseDocument<>)] = "###-secondary-response-document",
            [typeof(NullableSecondaryResourceResponseDocument<>)] = "nullable-###-secondary-response-document",
            [typeof(ResourceCollectionResponseDocument<>)] = "###-collection-response-document",
            [typeof(ResourceIdentifierResponseDocument<>)] = "###-identifier-response-document",
            [typeof(NullableResourceIdentifierResponseDocument<>)] = "nullable-###-identifier-response-document",
            [typeof(ResourceIdentifierCollectionResponseDocument<>)] = "###-identifier-collection-response-document",
            [typeof(ToOneRelationshipInResponse<>)] = "to-one-###-data-in-response",
            [typeof(NullableToOneRelationshipInResponse<>)] = "nullable-to-one-###-data-in-response",
            [typeof(ToManyRelationshipInResponse<>)] = "to-many-###-data-in-response",
            [typeof(ResourceResponseObject<>)] = "###-data-in-response",
            [typeof(ResourceIdentifierObject<>)] = "###-identifier"
        };

        private readonly ResourceNameFormatter _formatter;
        private readonly IResourceGraph _resourceGraph;

        public JsonApiSchemaIdSelector(ResourceNameFormatter formatter, IResourceGraph resourceGraph)
        {
            ArgumentGuard.NotNull(formatter, nameof(formatter));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

            _formatter = formatter;
            _resourceGraph = resourceGraph;
        }

        public string GetSchemaId(Type type)
        {
            ArgumentGuard.NotNull(type, nameof(type));

            ResourceType? resourceType = _resourceGraph.FindResourceType(type);

            if (resourceType != null)
            {
                return resourceType.PublicName.Singularize();
            }

            if (type.IsConstructedGenericType && OpenTypeToSchemaTemplateMap.ContainsKey(type.GetGenericTypeDefinition()))
            {
                Type resourceClrType = type.GetGenericArguments().First();
                string resourceName = _formatter.FormatResourceName(resourceClrType).Singularize();

                string template = OpenTypeToSchemaTemplateMap[type.GetGenericTypeDefinition()];
                return template.Replace("###", resourceName);
            }

            // Used for a fixed set of types, such as jsonapi-object, links-in-many-resource-document etc.
            return _formatter.FormatResourceName(type).Singularize();
        }
    }
}
