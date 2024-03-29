using System.Text.Json;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

namespace JsonApiDotNetCore.OpenApi;

internal sealed class JsonApiSchemaIdSelector
{
    private const string ResourceTypeSchemaIdTemplate = "[ResourceName] Resource Type";

    private static readonly IDictionary<Type, string> TypeToSchemaTemplateMap = new Dictionary<Type, string>
    {
        [typeof(ResourcePostRequestDocument<>)] = "[ResourceName] Post Request Document",
        [typeof(ResourcePatchRequestDocument<>)] = "[ResourceName] Patch Request Document",
        [typeof(ResourceDataInPostRequest<>)] = "[ResourceName] Data In Post Request",
        [typeof(AttributesInPostRequest<>)] = "[ResourceName] Attributes In Post Request",
        [typeof(RelationshipsInPostRequest<>)] = "[ResourceName] Relationships In Post Request",
        [typeof(ResourceDataInPatchRequest<>)] = "[ResourceName] Data In Patch Request",
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
        [typeof(ResourceData)] = "Data In Response",
        [typeof(ResourceDataInResponse<>)] = "[ResourceName] Data In Response",
        [typeof(AttributesInResponse<>)] = "[ResourceName] Attributes In Response",
        [typeof(RelationshipsInResponse<>)] = "[ResourceName] Relationships In Response",
        [typeof(ResourceIdentifier<>)] = "[ResourceName] Identifier"
    };

    private readonly IResourceGraph _resourceGraph;
    private readonly IJsonApiOptions _options;

    public JsonApiSchemaIdSelector(IResourceGraph resourceGraph, IJsonApiOptions options)
    {
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(options);

        _resourceGraph = resourceGraph;
        _options = options;
    }

    public string GetSchemaId(Type type)
    {
        ArgumentGuard.NotNull(type);

        ResourceType? resourceType = _resourceGraph.FindResourceType(type);

        if (resourceType != null)
        {
            return resourceType.PublicName.Singularize();
        }

        Type openType = type.ConstructedToOpenType();

        if (openType != type)
        {
            if (TypeToSchemaTemplateMap.TryGetValue(openType, out string? schemaTemplate))
            {
                Type resourceClrType = type.GetGenericArguments().First();
                resourceType = _resourceGraph.GetResourceType(resourceClrType);

                return ApplySchemaTemplate(schemaTemplate, resourceType);
            }
        }
        else
        {
            if (TypeToSchemaTemplateMap.TryGetValue(type, out string? schemaTemplate))
            {
                return ApplySchemaTemplate(schemaTemplate, null);
            }
        }

        // Used for a fixed set of non-generic types, such as Jsonapi, ResourceCollectionTopLevelLinks etc.
        return ApplySchemaTemplate(type.Name, null);
    }

    private string ApplySchemaTemplate(string schemaTemplate, ResourceType? resourceType)
    {
        string pascalCaseSchemaId = resourceType != null
            ? schemaTemplate.Replace("[ResourceName]", resourceType.PublicName.Singularize()).ToPascalCase()
            : schemaTemplate.ToPascalCase();

        JsonNamingPolicy? namingPolicy = _options.SerializerOptions.PropertyNamingPolicy;
        return namingPolicy != null ? namingPolicy.ConvertName(pascalCaseSchemaId) : pascalCaseSchemaId;
    }

    public string GetSchemaId(ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

        return ApplySchemaTemplate(ResourceTypeSchemaIdTemplate, resourceType);
    }
}
