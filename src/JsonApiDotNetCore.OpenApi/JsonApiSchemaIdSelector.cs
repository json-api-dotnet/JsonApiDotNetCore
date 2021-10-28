using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.RelationshipData;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

namespace JsonApiDotNetCore.OpenApi
{
    internal sealed class JsonApiSchemaIdSelector
    {
        private static readonly IDictionary<Type, string> OpenTypeToSchemaTemplateMap = new Dictionary<Type, string>
        {
            [typeof(ResourcePostRequestDocument<>)] = "[ResourceName] Post Request Document",
            [typeof(ResourcePatchRequestDocument<>)] = "[ResourceName] Patch Request Document",
            [typeof(ResourcePostRequestObject<>)] = "[ResourceName] Data In Post Request",
            [typeof(ResourcePatchRequestObject<>)] = "[ResourceName] Data In Patch Request",
            [typeof(ToOneRelationshipRequestData<>)] = "To One [ResourceName] Request Data",
            [typeof(ToManyRelationshipRequestData<>)] = "To Many [ResourceName] Request Data",
            [typeof(PrimaryResourceResponseDocument<>)] = "[ResourceName] Primary Response Document",
            [typeof(SecondaryResourceResponseDocument<>)] = "[ResourceName] Secondary Response Document",
            [typeof(ResourceCollectionResponseDocument<>)] = "[ResourceName] Collection Response Document",
            [typeof(ResourceIdentifierResponseDocument<>)] = "[ResourceName] Identifier Response Document",
            [typeof(ResourceIdentifierCollectionResponseDocument<>)] = "[ResourceName] Identifier Collection Response Document",
            [typeof(ToOneRelationshipResponseData<>)] = "To One [ResourceName] Response Data",
            [typeof(ToManyRelationshipResponseData<>)] = "To Many [ResourceName] Response Data",
            [typeof(ResourceResponseObject<>)] = "[ResourceName] Data In Response",
            [typeof(ResourceIdentifierObject<>)] = "[ResourceName] Identifier"
        };

        private readonly ResourceNameFormatter _formatter;
        private readonly JsonNamingPolicy _namingPolicy;
        private readonly IResourceGraph _resourceGraph;

        public JsonApiSchemaIdSelector(JsonNamingPolicy namingPolicy, IResourceGraph resourceGraph)
        {
            ArgumentGuard.NotNull(namingPolicy, nameof(namingPolicy));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

            _namingPolicy = namingPolicy;
            _resourceGraph = resourceGraph;
            _formatter = new ResourceNameFormatter(namingPolicy);
        }

        public string GetSchemaId(Type type)
        {
            ArgumentGuard.NotNull(type, nameof(type));

            ResourceContext resourceContext = _resourceGraph.TryGetResourceContext(type);

            if (resourceContext != null)
            {
                return resourceContext.PublicName.Singularize();
            }

            if (type.IsConstructedGenericType && OpenTypeToSchemaTemplateMap.ContainsKey(type.GetGenericTypeDefinition()))
            {
                Type resourceType = type.GetGenericArguments().First();
                string resourceName = _formatter.FormatResourceName(resourceType).Singularize();

                string pascalCaseSchemaIdTemplate = OpenTypeToSchemaTemplateMap[type.GetGenericTypeDefinition()];

                // @formatter:wrap_chained_method_calls chop_always
                // @formatter:keep_existing_linebreaks true

                string capitalizedResourceName = Capitalize(resourceName);

                string pascalCaseSchemaId = pascalCaseSchemaIdTemplate
                    .Replace("[ResourceName]", capitalizedResourceName)
                    .Replace(" ", "");

                // @formatter:keep_existing_linebreaks restore
                // @formatter:wrap_chained_method_calls restore

                return _namingPolicy.ConvertName(pascalCaseSchemaId);
            }

            // Used for a fixed set of types, such as jsonapi-object, links-in-many-resource-document etc.
            return _formatter.FormatResourceName(type).Singularize();
        }

        private static string Capitalize(string term)
        {
            return string.Concat(term[0].ToString().ToUpper(), term.AsSpan(1));
        }
    }
}
