using System.Text.Json;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Relationships;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal sealed class JsonApiSchemaIdSelector
{
    private const string ResourceTypeSchemaIdTemplate = "[ResourceName] Resource Type";
    private const string MetaSchemaIdTemplate = "Meta";

    private const string UpdateRelationshipAtomicOperationSchemaIdTemplate = "Update [ResourceName] [RelationshipName] Relationship Operation";
    private const string AddToRelationshipAtomicOperationSchemaIdTemplate = "Add To [ResourceName] [RelationshipName] Relationship Operation";
    private const string RemoveFromRelationshipAtomicOperationSchemaIdTemplate = "Remove From [ResourceName] [RelationshipName] Relationship Operation";
    private const string RelationshipIdentifierSchemaIdTemplate = "[ResourceName] [RelationshipName] Relationship Identifier";
    private const string RelationshipNameSchemaIdTemplate = "[ResourceName] [RelationshipName] Relationship Name";

    private static readonly Dictionary<Type, string> SchemaTypeToTemplateMap = new()
    {
        [typeof(CreateRequestDocument<>)] = "Create [ResourceName] Request Document",
        [typeof(UpdateRequestDocument<>)] = "Update [ResourceName] Request Document",
        [typeof(DataInCreateRequest<>)] = "Data In Create [ResourceName] Request",
        [typeof(AttributesInCreateRequest<>)] = "Attributes In Create [ResourceName] Request",
        [typeof(RelationshipsInCreateRequest<>)] = "Relationships In Create [ResourceName] Request",
        [typeof(DataInUpdateRequest<>)] = "Data In Update [ResourceName] Request",
        [typeof(AttributesInUpdateRequest<>)] = "Attributes In Update [ResourceName] Request",
        [typeof(RelationshipsInUpdateRequest<>)] = "Relationships In Update [ResourceName] Request",
        [typeof(ToOneInRequest<>)] = "To One [ResourceName] In Request",
        [typeof(NullableToOneInRequest<>)] = "Nullable To One [ResourceName] In Request",
        [typeof(ToManyInRequest<>)] = "To Many [ResourceName] In Request",
        [typeof(PrimaryResponseDocument<>)] = "Primary [ResourceName] Response Document",
        [typeof(SecondaryResponseDocument<>)] = "Secondary [ResourceName] Response Document",
        [typeof(NullableSecondaryResponseDocument<>)] = "Nullable Secondary [ResourceName] Response Document",
        [typeof(CollectionResponseDocument<>)] = "[ResourceName] Collection Response Document",
        [typeof(IdentifierResponseDocument<>)] = "[ResourceName] Identifier Response Document",
        [typeof(NullableIdentifierResponseDocument<>)] = "Nullable [ResourceName] Identifier Response Document",
        [typeof(IdentifierCollectionResponseDocument<>)] = "[ResourceName] Identifier Collection Response Document",
        [typeof(ToOneInResponse<>)] = "To One [ResourceName] In Response",
        [typeof(NullableToOneInResponse<>)] = "Nullable To One [ResourceName] In Response",
        [typeof(ToManyInResponse<>)] = "To Many [ResourceName] In Response",
        [typeof(ResourceInResponse)] = "Resource In Response",
        [typeof(DataInResponse<>)] = "Data In [ResourceName] Response",
        [typeof(AttributesInResponse<>)] = "Attributes In [ResourceName] Response",
        [typeof(RelationshipsInResponse<>)] = "Relationships In [ResourceName] Response",
        [typeof(IdentifierInRequest)] = "Identifier In Request",
        [typeof(IdentifierInRequest<>)] = "[ResourceName] Identifier In Request",
        [typeof(IdentifierInResponse<>)] = "[ResourceName] Identifier In Response",
        [typeof(CreateOperation<>)] = "Create [ResourceName] Operation",
        [typeof(UpdateOperation<>)] = "Update [ResourceName] Operation",
        [typeof(DeleteOperation<>)] = "Delete [ResourceName] Operation",
        [typeof(UpdateToOneRelationshipOperation<>)] = "Temporary Update [ResourceName] To One Relationship Operation",
        [typeof(UpdateToManyRelationshipOperation<>)] = "Temporary Update [ResourceName] To Many Relationship Operation",
        [typeof(AddToRelationshipOperation<>)] = "Temporary Add To [ResourceName] Relationship Operation",
        [typeof(RemoveFromRelationshipOperation<>)] = "Temporary Remove From [ResourceName] Relationship Operation"
    };

    private readonly IJsonApiOptions _options;
    private readonly IResourceGraph _resourceGraph;

    public JsonApiSchemaIdSelector(IJsonApiOptions options, IResourceGraph resourceGraph)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resourceGraph);

        _options = options;
        _resourceGraph = resourceGraph;
    }

    public string GetSchemaId(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        ResourceType? resourceType = _resourceGraph.FindResourceType(type);

        if (resourceType != null)
        {
            return resourceType.PublicName.Singularize();
        }

        Type openType = type.ConstructedToOpenType();

        if (openType != type)
        {
            if (SchemaTypeToTemplateMap.TryGetValue(openType, out string? schemaTemplate))
            {
                Type resourceClrType = type.GetGenericArguments().First();
                resourceType = _resourceGraph.GetResourceType(resourceClrType);

                return ApplySchemaTemplate(schemaTemplate, resourceType, null, null);
            }
        }
        else
        {
            if (SchemaTypeToTemplateMap.TryGetValue(type, out string? schemaTemplate))
            {
                return ApplySchemaTemplate(schemaTemplate, null, null, null);
            }
        }

        // Used for a fixed set of non-generic types, such as Jsonapi, ResourceCollectionTopLevelLinks etc.
        return ApplySchemaTemplate(type.Name, null, null, null);
    }

    private string ApplySchemaTemplate(string schemaTemplate, ResourceType? resourceType, string? relationshipName, AtomicOperationCode? operationCode)
    {
        string schemaId = schemaTemplate;

        schemaId = resourceType != null
            ? schemaId.Replace("[ResourceName]", resourceType.PublicName.Singularize()).Pascalize()
            : schemaId.Replace("[ResourceName]", "$$$").Pascalize().Replace("$$$", string.Empty);

        if (relationshipName != null)
        {
            schemaId = schemaId.Replace("[RelationshipName]", relationshipName.Pascalize());
        }

        if (operationCode != null)
        {
            schemaId = schemaId.Replace("[OperationCode]", operationCode.Value.ToString().Pascalize());
        }

        string pascalCaseSchemaId = schemaId.Pascalize();

        JsonNamingPolicy? namingPolicy = _options.SerializerOptions.PropertyNamingPolicy;
        return namingPolicy != null ? namingPolicy.ConvertName(pascalCaseSchemaId) : pascalCaseSchemaId;
    }

    public string GetResourceTypeSchemaId(ResourceType? resourceType)
    {
        return ApplySchemaTemplate(ResourceTypeSchemaIdTemplate, resourceType, null, null);
    }

    public string GetMetaSchemaId()
    {
        return ApplySchemaTemplate(MetaSchemaIdTemplate, null, null, null);
    }

    public string GetAtomicOperationCodeSchemaId(AtomicOperationCode operationCode)
    {
        return ApplySchemaTemplate("[OperationCode] Operation Code", null, null, operationCode);
    }

    public string GetRelationshipAtomicOperationSchemaId(RelationshipAttribute relationship, AtomicOperationCode operationCode)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        string schemaIdTemplate = operationCode switch
        {
            AtomicOperationCode.Add => AddToRelationshipAtomicOperationSchemaIdTemplate,
            AtomicOperationCode.Remove => RemoveFromRelationshipAtomicOperationSchemaIdTemplate,
            _ => UpdateRelationshipAtomicOperationSchemaIdTemplate
        };

        return ApplySchemaTemplate(schemaIdTemplate, relationship.LeftType, relationship.PublicName, null);
    }

    public string GetRelationshipIdentifierSchemaId(RelationshipAttribute relationship)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        return ApplySchemaTemplate(RelationshipIdentifierSchemaIdTemplate, relationship.LeftType, relationship.PublicName, null);
    }

    public string GetRelationshipNameSchemaId(RelationshipAttribute relationship)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        return ApplySchemaTemplate(RelationshipNameSchemaIdTemplate, relationship.LeftType, relationship.PublicName, null);
    }
}
