using System.Text.Json;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

namespace JsonApiDotNetCore.OpenApi;

internal sealed class JsonApiSchemaIdSelector
{
    private static readonly IDictionary<Type, string> OpenTypeToSchemaTemplateMap = new Dictionary<Type, string>
    {
        [typeof(ResourcePostRequestDocument<>)] = "[ResourceName] Post Request Document",
        [typeof(ResourcePatchRequestDocument<>)] = "[ResourceName] Patch Request Document",
        [typeof(ResourceObjectInPostRequest<>)] = "[ResourceName] Data In Post Request",
        [typeof(AttributesInPostRequest<>)] = "[ResourceName] Attributes In Post Request",
        [typeof(RelationshipsInPostRequest<>)] = "[ResourceName] Relationships In Post Request",
        [typeof(ResourceObjectInPatchRequest<>)] = "[ResourceName] Data In Patch Request",
        [typeof(AttributesInPatchRequest<>)] = "[ResourceName] Attributes In Patch Request",
        [typeof(RelationshipsInPatchRequest<>)] = "[ResourceName] Relationships In Patch Request",
        [typeof(ToOneRelationshipInRequest<>)] = "To One [ResourceName] In Request",
        [typeof(NullableToOneRelationshipInRequest<>)] = "Nullable To One [ResourceName] In Request",
        [typeof(ToManyRelationshipInRequest<>)] = "To Many [ResourceName] In Request",
        [typeof(PrimaryResourceResponseDocument<>)] = "[ResourceName] Primary Response Document",
        [typeof(SecondaryResourceResponseDocument<>)] = "[ResourceName] Secondary Response Document",
        [typeof(NullableSecondaryResourceResponseDocument<>)] = "Nullable [ResourceName] Secondary Response Document",
        [typeof(ResourceCollectionResponseDocument<>)] = "[ResourceName] Collection Response Document",
        [typeof(ResourceIdentifierResponseDocument<>)] = "[ResourceName] Identifier Response Document",
        [typeof(NullableResourceIdentifierResponseDocument<>)] = "Nullable [ResourceName] Identifier Response Document",
        [typeof(ResourceIdentifierCollectionResponseDocument<>)] = "[ResourceName] Identifier Collection Response Document",
        [typeof(ToOneRelationshipInResponse<>)] = "To One [ResourceName] In Response",
        [typeof(NullableToOneRelationshipInResponse<>)] = "Nullable To One [ResourceName] In Response",
        [typeof(ToManyRelationshipInResponse<>)] = "To Many [ResourceName] In Response",
        [typeof(ResourceObjectInResponse<>)] = "[ResourceName] Data In Response",
        [typeof(AttributesInResponse<>)] = "[ResourceName] Attributes In Response",
        [typeof(RelationshipsInResponse<>)] = "[ResourceName] Relationships In Response",
        [typeof(ResourceIdentifierObject<>)] = "[ResourceName] Identifier"
    };

    private readonly JsonNamingPolicy? _namingPolicy;
    private readonly IResourceGraph _resourceGraph;

    public JsonApiSchemaIdSelector(JsonNamingPolicy? namingPolicy, IResourceGraph resourceGraph)
    {
        ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

        _namingPolicy = namingPolicy;
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
            Type openType = type.GetGenericTypeDefinition();
            Type resourceClrType = type.GetGenericArguments().First();
            resourceType = _resourceGraph.FindResourceType(resourceClrType);

            if (resourceType == null)
            {
                throw new UnreachableCodeException();
            }

            string pascalCaseSchemaId = OpenTypeToSchemaTemplateMap[openType].Replace("[ResourceName]", resourceType.PublicName.Singularize()).ToPascalCase();

            return _namingPolicy != null ? _namingPolicy.ConvertName(pascalCaseSchemaId) : pascalCaseSchemaId;
        }

        // Used for a fixed set of types, such as JsonApiObject, LinksInResourceCollectionDocument etc.
        return _namingPolicy != null ? _namingPolicy.ConvertName(type.Name) : type.Name;
    }
}
