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

    private const string ResourceAtomicOperationDiscriminatorValueTemplate = "[OperationCode] [ResourceName]";
    private const string UpdateRelationshipAtomicOperationDiscriminatorValueTemplate = "Update [ResourceName] [RelationshipName]";
    private const string AddToRelationshipAtomicOperationDiscriminatorValueTemplate = "Add To [ResourceName] [RelationshipName]";
    private const string RemoveFromRelationshipAtomicOperationDiscriminatorValueTemplate = "Remove From [ResourceName] [RelationshipName]";

    private const string UpdateRelationshipAtomicOperationSchemaIdTemplate = "Update [ResourceName] [RelationshipName] Relationship Operation";
    private const string AddToRelationshipAtomicOperationSchemaIdTemplate = "Add To [ResourceName] [RelationshipName] Relationship Operation";
    private const string RemoveFromRelationshipAtomicOperationSchemaIdTemplate = "Remove From [ResourceName] [RelationshipName] Relationship Operation";
    private const string RelationshipIdentifierSchemaIdTemplate = "[ResourceName] [RelationshipName] Relationship Identifier";
    private const string RelationshipNameSchemaIdTemplate = "[ResourceName] [RelationshipName] Relationship Name";

    private static readonly Dictionary<Type, string> SchemaTypeToTemplateMap = new()
    {
        [typeof(CreateResourceRequestDocument<>)] = "Create [ResourceName] Request Document",
        [typeof(UpdateResourceRequestDocument<>)] = "Update [ResourceName] Request Document",
        [typeof(DataInCreateResourceRequest<>)] = "Data In Create [ResourceName] Request",
        [typeof(AttributesInCreateResourceRequest<>)] = "Attributes In Create [ResourceName] Request",
        [typeof(RelationshipsInCreateResourceRequest<>)] = "Relationships In Create [ResourceName] Request",
        [typeof(DataInUpdateResourceRequest<>)] = "Data In Update [ResourceName] Request",
        [typeof(AttributesInUpdateResourceRequest<>)] = "Attributes In Update [ResourceName] Request",
        [typeof(RelationshipsInUpdateResourceRequest<>)] = "Relationships In Update [ResourceName] Request",
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
        [typeof(ResourceIdentifierInRequest<>)] = "[ResourceName] Identifier In Request",
        [typeof(ResourceIdentifierInResponse<>)] = "[ResourceName] Identifier In Response",
        [typeof(CreateResourceOperation<>)] = "Create [ResourceName] Operation",
        [typeof(UpdateResourceOperation<>)] = "Update [ResourceName] Operation",
        [typeof(DeleteResourceOperation<>)] = "Delete [ResourceName] Operation",
        [typeof(UpdateToOneRelationshipOperation<>)] = "Temporary Update [ResourceName] To One Relationship Operation",
        [typeof(UpdateToManyRelationshipOperation<>)] = "Temporary Update [ResourceName] To Many Relationship Operation",
        [typeof(AddToRelationshipOperation<>)] = "Temporary Add To [ResourceName] To Many Relationship Operation",
        [typeof(RemoveFromRelationshipOperation<>)] = "Temporary Remove From [ResourceName] To Many Relationship Operation"
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

        if (resourceType != null)
        {
            schemaId = schemaId.Replace("[ResourceName]", resourceType.PublicName.Singularize()).Pascalize();
        }

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

    public string GetResourceTypeSchemaId(ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

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

    public string GetAtomicOperationDiscriminatorValue(AtomicOperationCode operationCode, ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

        return ApplySchemaTemplate(ResourceAtomicOperationDiscriminatorValueTemplate, resourceType, null, operationCode);
    }

    public string GetAtomicOperationDiscriminatorValue(AtomicOperationCode operationCode, RelationshipAttribute relationship)
    {
        ArgumentGuard.NotNull(relationship);

        string schemaIdTemplate = operationCode switch
        {
            AtomicOperationCode.Add => AddToRelationshipAtomicOperationDiscriminatorValueTemplate,
            AtomicOperationCode.Remove => RemoveFromRelationshipAtomicOperationDiscriminatorValueTemplate,
            _ => UpdateRelationshipAtomicOperationDiscriminatorValueTemplate
        };

        return ApplySchemaTemplate(schemaIdTemplate, relationship.LeftType, relationship.PublicName, null);
    }

    public string GetRelationshipAtomicOperationSchemaId(RelationshipAttribute relationship, AtomicOperationCode operationCode)
    {
        ArgumentGuard.NotNull(relationship);

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
        ArgumentGuard.NotNull(relationship);

        return ApplySchemaTemplate(RelationshipIdentifierSchemaIdTemplate, relationship.LeftType, relationship.PublicName, null);
    }

    public string GetRelationshipNameSchemaId(RelationshipAttribute relationship)
    {
        ArgumentGuard.NotNull(relationship);

        return ApplySchemaTemplate(RelationshipNameSchemaIdTemplate, relationship.LeftType, relationship.PublicName, null);
    }
}
