using System.Reflection;
using JsonApiDotNetCore.OpenApi.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using SchemaGenerator = Swashbuckle.AspNetCore.SwaggerGen.Patched.SchemaGenerator;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class ResourceFieldSchemaBuilder
{
    private static readonly Type[] RelationshipSchemaInResponseOpenTypes =
    [
        typeof(ToOneRelationshipInResponse<>),
        typeof(ToManyRelationshipInResponse<>),
        typeof(NullableToOneRelationshipInResponse<>)
    ];

    private static readonly Type[] NullableRelationshipSchemaOpenTypes =
    [
        typeof(NullableToOneRelationshipInRequest<>),
        typeof(NullableToOneRelationshipInResponse<>)
    ];

    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly ResourceIdentifierSchemaGenerator _resourceIdentifierSchemaGenerator;
    private readonly LinksVisibilitySchemaGenerator _linksVisibilitySchemaGenerator;
    private readonly ResourceTypeInfo _resourceTypeInfo;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;
    private readonly RelationshipTypeFactory _relationshipTypeFactory;

    private readonly SchemaRepository _resourceSchemaRepository = new();
    private readonly ResourceDocumentationReader _resourceDocumentationReader = new();
    private readonly IDictionary<string, OpenApiSchema> _schemasForResourceFields;

    public ResourceFieldSchemaBuilder(SchemaGenerator defaultSchemaGenerator, ResourceIdentifierSchemaGenerator resourceIdentifierSchemaGenerator,
        LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator, ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider,
        RelationshipTypeFactory relationshipTypeFactory, ResourceTypeInfo resourceTypeInfo)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(resourceIdentifierSchemaGenerator);
        ArgumentGuard.NotNull(linksVisibilitySchemaGenerator);
        ArgumentGuard.NotNull(resourceTypeInfo);
        ArgumentGuard.NotNull(resourceFieldValidationMetadataProvider);
        ArgumentGuard.NotNull(relationshipTypeFactory);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _resourceIdentifierSchemaGenerator = resourceIdentifierSchemaGenerator;
        _linksVisibilitySchemaGenerator = linksVisibilitySchemaGenerator;
        _resourceTypeInfo = resourceTypeInfo;
        _resourceFieldValidationMetadataProvider = resourceFieldValidationMetadataProvider;
        _relationshipTypeFactory = relationshipTypeFactory;

        _schemasForResourceFields = GetFieldSchemas();
    }

    private IDictionary<string, OpenApiSchema> GetFieldSchemas()
    {
        if (!_resourceSchemaRepository.TryLookupByType(_resourceTypeInfo.ResourceType.ClrType, out OpenApiSchema referenceSchemaForResource))
        {
            referenceSchemaForResource = _defaultSchemaGenerator.GenerateSchema(_resourceTypeInfo.ResourceType.ClrType, _resourceSchemaRepository);
        }

        OpenApiSchema fullSchemaForResource = _resourceSchemaRepository.Schemas[referenceSchemaForResource.Reference.Id];
        return fullSchemaForResource.Properties;
    }

    public void SetMembersOfAttributes(OpenApiSchema fullSchemaForAttributes, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(fullSchemaForAttributes);
        ArgumentGuard.NotNull(schemaRepository);

        AttrCapabilities requiredCapability = GetRequiredCapabilityForAttributes(_resourceTypeInfo.ResourceDataOpenType);

        foreach ((string fieldName, OpenApiSchema resourceFieldSchema) in _schemasForResourceFields)
        {
            AttrAttribute? matchingAttribute = _resourceTypeInfo.ResourceType.FindAttributeByPublicName(fieldName);

            if (matchingAttribute != null && matchingAttribute.Capabilities.HasFlag(requiredCapability))
            {
                bool isPrimitiveOpenApiType = resourceFieldSchema.AllOf.IsNullOrEmpty();

                // Types like enum and complex attributes are not primitive and handled as reference schemas.
                if (!isPrimitiveOpenApiType)
                {
                    EnsureAttributeSchemaIsExposed(resourceFieldSchema, matchingAttribute, schemaRepository);
                }

                fullSchemaForAttributes.Properties[matchingAttribute.PublicName] = resourceFieldSchema;

                resourceFieldSchema.Nullable = _resourceFieldValidationMetadataProvider.IsNullable(matchingAttribute);

                if (IsFieldRequired(matchingAttribute))
                {
                    fullSchemaForAttributes.Required.Add(matchingAttribute.PublicName);
                }

                resourceFieldSchema.Description = _resourceDocumentationReader.GetDocumentationForAttribute(matchingAttribute);
            }
        }
    }

    private static AttrCapabilities GetRequiredCapabilityForAttributes(Type resourceDataOpenType)
    {
        return resourceDataOpenType == typeof(ResourceDataInResponse<>) ? AttrCapabilities.AllowView :
            resourceDataOpenType == typeof(ResourceDataInPostRequest<>) ? AttrCapabilities.AllowCreate :
            resourceDataOpenType == typeof(ResourceDataInPatchRequest<>) ? AttrCapabilities.AllowChange : throw new UnreachableCodeException();
    }

    private void EnsureAttributeSchemaIsExposed(OpenApiSchema attributeReferenceSchema, AttrAttribute attribute, SchemaRepository schemaRepository)
    {
        Type nonNullableTypeInPropertyType = GetRepresentedTypeForAttributeSchema(attribute);

        if (schemaRepository.TryLookupByType(nonNullableTypeInPropertyType, out _))
        {
            return;
        }

        string schemaId = attributeReferenceSchema.UnwrapExtendedReferenceSchema().Reference.Id;

        OpenApiSchema fullSchema = _resourceSchemaRepository.Schemas[schemaId];
        schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(nonNullableTypeInPropertyType, schemaId);
    }

    private Type GetRepresentedTypeForAttributeSchema(AttrAttribute attribute)
    {
        NullabilityInfoContext nullabilityInfoContext = new();
        NullabilityInfo attributeNullabilityInfo = nullabilityInfoContext.Create(attribute.Property);

        bool isNullable = attributeNullabilityInfo is { ReadState: NullabilityState.Nullable, WriteState: NullabilityState.Nullable };

        Type nonNullableTypeInPropertyType = isNullable
            ? Nullable.GetUnderlyingType(attribute.Property.PropertyType) ?? attribute.Property.PropertyType
            : attribute.Property.PropertyType;

        return nonNullableTypeInPropertyType;
    }

    private bool IsFieldRequired(ResourceFieldAttribute field)
    {
        bool isSchemaForPostResourceRequest = _resourceTypeInfo.ResourceDataOpenType == typeof(ResourceDataInPostRequest<>);
        return isSchemaForPostResourceRequest && _resourceFieldValidationMetadataProvider.IsRequired(field);
    }

    public void SetMembersOfRelationships(OpenApiSchema fullSchemaForRelationships, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(fullSchemaForRelationships);
        ArgumentGuard.NotNull(schemaRepository);

        foreach (string fieldName in _schemasForResourceFields.Keys)
        {
            RelationshipAttribute? matchingRelationship = _resourceTypeInfo.ResourceType.FindRelationshipByPublicName(fieldName);

            if (matchingRelationship != null)
            {
                _ = _resourceIdentifierSchemaGenerator.GenerateSchema(matchingRelationship.RightType, schemaRepository);
                AddRelationshipSchemaToResourceData(matchingRelationship, fullSchemaForRelationships, schemaRepository);
            }
        }
    }

    private void AddRelationshipSchemaToResourceData(RelationshipAttribute relationship, OpenApiSchema fullSchemaForRelationships,
        SchemaRepository schemaRepository)
    {
        Type relationshipSchemaType = GetRelationshipSchemaType(relationship, _resourceTypeInfo.ResourceDataOpenType);

        OpenApiSchema referenceSchemaForRelationship = GetReferenceSchemaForRelationship(relationshipSchemaType, schemaRepository) ??
            CreateRelationshipReferenceSchema(relationshipSchemaType, schemaRepository);

        var extendedReferenceSchemaForRelationship = new OpenApiSchema
        {
            AllOf = new List<OpenApiSchema>
            {
                referenceSchemaForRelationship
            },
            Description = _resourceDocumentationReader.GetDocumentationForRelationship(relationship)
        };

        fullSchemaForRelationships.Properties.Add(relationship.PublicName, extendedReferenceSchemaForRelationship);

        if (IsFieldRequired(relationship))
        {
            fullSchemaForRelationships.Required.Add(relationship.PublicName);
        }
    }

    private Type GetRelationshipSchemaType(RelationshipAttribute relationship, Type resourceDataConstructedType)
    {
        return resourceDataConstructedType.GetGenericTypeDefinition().IsAssignableTo(typeof(ResourceDataInResponse<>))
            ? _relationshipTypeFactory.GetForResponse(relationship)
            : _relationshipTypeFactory.GetForRequest(relationship);
    }

    private OpenApiSchema? GetReferenceSchemaForRelationship(Type relationshipSchemaType, SchemaRepository schemaRepository)
    {
        return schemaRepository.TryLookupByType(relationshipSchemaType, out OpenApiSchema? referenceSchema) ? referenceSchema : null;
    }

    private OpenApiSchema CreateRelationshipReferenceSchema(Type relationshipSchemaType, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchema = _defaultSchemaGenerator.GenerateSchema(relationshipSchemaType, schemaRepository);

        OpenApiSchema fullSchema = schemaRepository.Schemas[referenceSchema.Reference.Id];

        if (IsDataPropertyNullableInRelationshipSchemaType(relationshipSchemaType))
        {
            fullSchema.Properties[JsonApiPropertyName.Data].Nullable = true;
        }

        if (IsRelationshipInResponseType(relationshipSchemaType))
        {
            _linksVisibilitySchemaGenerator.UpdateSchemaForRelationship(relationshipSchemaType, fullSchema, schemaRepository);

            fullSchema.Required.Remove(JsonApiPropertyName.Data);

            fullSchema.SetValuesInMetaToNullable();
        }

        return referenceSchema;
    }

    private static bool IsRelationshipInResponseType(Type relationshipSchemaType)
    {
        Type relationshipSchemaOpenType = relationshipSchemaType.GetGenericTypeDefinition();

        return RelationshipSchemaInResponseOpenTypes.Contains(relationshipSchemaOpenType);
    }

    private static bool IsDataPropertyNullableInRelationshipSchemaType(Type relationshipSchemaType)
    {
        Type relationshipSchemaOpenType = relationshipSchemaType.GetGenericTypeDefinition();
        return NullableRelationshipSchemaOpenTypes.Contains(relationshipSchemaOpenType);
    }
}
