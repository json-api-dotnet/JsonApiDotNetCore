using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.RelationshipData;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Configuration;

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
            [typeof(ToOneRelationshipRequestData<>)] = "to-one-###-request-data",
            [typeof(ToManyRelationshipRequestData<>)] = "to-many-###-request-data",
            [typeof(PrimaryResourceResponseDocument<>)] = "###-primary-response-document",
            [typeof(SecondaryResourceResponseDocument<>)] = "###-secondary-response-document",
            [typeof(ResourceCollectionResponseDocument<>)] = "###-collection-response-document",
            [typeof(ResourceIdentifierResponseDocument<>)] = "###-identifier-response-document",
            [typeof(ResourceIdentifierCollectionResponseDocument<>)] = "###-identifier-collection-response-document",
            [typeof(ToOneRelationshipResponseData<>)] = "to-one-###-response-data",
            [typeof(ToManyRelationshipResponseData<>)] = "to-many-###-response-data",
            [typeof(ResourceResponseObject<>)] = "###-data-in-response",
            [typeof(ResourceIdentifierObject<>)] = "###-identifier"
        };

        private readonly ResourceNameFormatterProxy _formatter;
        private readonly IResourceContextProvider _resourceContextProvider;

        public JsonApiSchemaIdSelector(ResourceNameFormatterProxy formatter, IResourceContextProvider resourceContextProvider)
        {
            ArgumentGuard.NotNull(formatter, nameof(formatter));
            ArgumentGuard.NotNull(resourceContextProvider, nameof(resourceContextProvider));

            _formatter = formatter;
            _resourceContextProvider = resourceContextProvider;
        }

        public string GetSchemaId(Type type)
        {
            ArgumentGuard.NotNull(type, nameof(type));

            ResourceContext resourceContext = _resourceContextProvider.GetResourceContext(type);

            if (resourceContext != null)
            {
                return resourceContext.PublicName.Singularize();
            }

            if (type.IsConstructedGenericType && OpenTypeToSchemaTemplateMap.ContainsKey(type.GetGenericTypeDefinition()))
            {
                Type resourceType = type.GetGenericArguments().First();
                string resourceName = _formatter.FormatResourceName(resourceType).Singularize();

                string template = OpenTypeToSchemaTemplateMap[type.GetGenericTypeDefinition()];
                return template.Replace("###", resourceName);
            }

            // Used for a fixed set of types, such as jsonapi-object, links-in-many-resource-document etc.
            return _formatter.FormatResourceName(type).Singularize();
        }
    }
}
