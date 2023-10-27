using System.Reflection;
using JsonApiDotNetCore.OpenApi.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class ResourceFieldObjectSchemaBuilder
{
    private static readonly Type[] RelationshipSchemaInResponseOpenTypes =
    {
        typeof(ToOneRelationshipInResponse<>),
        typeof(ToManyRelationshipInResponse<>),
        typeof(NullableToOneRelationshipInResponse<>)
    };

    private static readonly Type[] NullableRelationshipSchemaOpenTypes =
    {
        typeof(NullableToOneRelationshipInRequest<>),
        typeof(NullableToOneRelationshipInResponse<>)
    };

    private static readonly string[] RelationshipObjectPropertyNamesInOrder =
    {
        JsonApiPropertyName.Links,
        JsonApiPropertyName.Data,
        JsonApiPropertyName.Meta
    };

    private readonly ResourceTypeInfo _resourceTypeInfo;
    private readonly ISchemaRepositoryAccessor _schemaRepositoryAccessor;
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly SchemaRepository _resourceSchemaRepository = new();
    private readonly IDictionary<string, OpenApiSchema> _schemasForResourceFields;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;
    private readonly RelationshipTypeFactory _relationshipTypeFactory;
    private readonly NullabilityInfoContext _nullabilityInfoContext = new();
    private readonly ResourceObjectDocumentationReader _resourceObjectDocumentationReader;

    public ResourceFieldObjectSchemaBuilder(ResourceTypeInfo resourceTypeInfo, ISchemaRepositoryAccessor schemaRepositoryAccessor,
        SchemaGenerator defaultSchemaGenerator, ResourceTypeSchemaGenerator resourceTypeSchemaGenerator,
        ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider)
    {
        ArgumentGuard.NotNull(resourceTypeInfo);
        ArgumentGuard.NotNull(schemaRepositoryAccessor);
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(resourceTypeSchemaGenerator);
        ArgumentGuard.NotNull(resourceFieldValidationMetadataProvider);

        _resourceTypeInfo = resourceTypeInfo;
        _schemaRepositoryAccessor = schemaRepositoryAccessor;
        _defaultSchemaGenerator = defaultSchemaGenerator;
        _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;
        _resourceFieldValidationMetadataProvider = resourceFieldValidationMetadataProvider;

        _relationshipTypeFactory = new RelationshipTypeFactory(resourceFieldValidationMetadataProvider);
        _schemasForResourceFields = GetFieldSchemas();
        _resourceObjectDocumentationReader = new ResourceObjectDocumentationReader();
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

    public void SetMembersOfAttributesObject(OpenApiSchema fullSchemaForAttributesObject)
    {
        ArgumentGuard.NotNull(fullSchemaForAttributesObject);

        AttrCapabilities requiredCapability = GetRequiredCapabilityForAttributes(_resourceTypeInfo.ResourceObjectOpenType);

        foreach ((string fieldName, OpenApiSchema resourceFieldSchema) in _schemasForResourceFields)
        {
            AttrAttribute? matchingAttribute = _resourceTypeInfo.ResourceType.FindAttributeByPublicName(fieldName);

            if (matchingAttribute != null && matchingAttribute.Capabilities.HasFlag(requiredCapability))
            {
                bool isPrimitiveOpenApiType = resourceFieldSchema.AllOf.IsNullOrEmpty();

                // Types like enum and complex attributes are not primitive and handled as reference schemas.
                if (!isPrimitiveOpenApiType)
                {
                    EnsureAttributeSchemaIsExposed(resourceFieldSchema, matchingAttribute);
                }

                fullSchemaForAttributesObject.Properties[matchingAttribute.PublicName] = resourceFieldSchema;

                resourceFieldSchema.Nullable = _resourceFieldValidationMetadataProvider.IsNullable(matchingAttribute);

                if (IsFieldRequired(matchingAttribute))
                {
                    fullSchemaForAttributesObject.Required.Add(matchingAttribute.PublicName);
                }

                resourceFieldSchema.Description = _resourceObjectDocumentationReader.GetDocumentationForAttribute(matchingAttribute);
            }
        }
    }

    private static AttrCapabilities GetRequiredCapabilityForAttributes(Type resourceObjectOpenType)
    {
        return resourceObjectOpenType == typeof(ResourceObjectInResponse<>) ? AttrCapabilities.AllowView :
            resourceObjectOpenType == typeof(ResourceObjectInPostRequest<>) ? AttrCapabilities.AllowCreate :
            resourceObjectOpenType == typeof(ResourceObjectInPatchRequest<>) ? AttrCapabilities.AllowChange : throw new UnreachableCodeException();
    }

    private void EnsureAttributeSchemaIsExposed(OpenApiSchema attributeReferenceSchema, AttrAttribute attribute)
    {
        Type nonNullableTypeInPropertyType = GetRepresentedTypeForAttributeSchema(attribute);

        if (_schemaRepositoryAccessor.Current.TryLookupByType(nonNullableTypeInPropertyType, out _))
        {
            return;
        }

        string schemaId = attributeReferenceSchema.UnwrapExtendedReferenceSchema().Reference.Id;

        OpenApiSchema fullSchema = _resourceSchemaRepository.Schemas[schemaId];
        _schemaRepositoryAccessor.Current.AddDefinition(schemaId, fullSchema);
        _schemaRepositoryAccessor.Current.RegisterType(nonNullableTypeInPropertyType, schemaId);
    }

    private Type GetRepresentedTypeForAttributeSchema(AttrAttribute attribute)
    {
        NullabilityInfo attributeNullabilityInfo = _nullabilityInfoContext.Create(attribute.Property);

        bool isNullable = attributeNullabilityInfo is { ReadState: NullabilityState.Nullable, WriteState: NullabilityState.Nullable };

        Type nonNullableTypeInPropertyType = isNullable
            ? Nullable.GetUnderlyingType(attribute.Property.PropertyType) ?? attribute.Property.PropertyType
            : attribute.Property.PropertyType;

        return nonNullableTypeInPropertyType;
    }

    private bool IsFieldRequired(ResourceFieldAttribute field)
    {
        bool isSchemaForUpdateResourceEndpoint = _resourceTypeInfo.ResourceObjectOpenType == typeof(ResourceObjectInPatchRequest<>);

        return !isSchemaForUpdateResourceEndpoint && _resourceFieldValidationMetadataProvider.IsRequired(field);
    }

    public void SetMembersOfRelationshipsObject(OpenApiSchema fullSchemaForRelationshipsObject)
    {
        ArgumentGuard.NotNull(fullSchemaForRelationshipsObject);

        foreach (string fieldName in _schemasForResourceFields.Keys)
        {
            RelationshipAttribute? matchingRelationship = _resourceTypeInfo.ResourceType.FindRelationshipByPublicName(fieldName);

            if (matchingRelationship != null)
            {
                EnsureResourceIdentifierObjectSchemaExists(matchingRelationship);
                AddRelationshipSchemaToResourceObject(matchingRelationship, fullSchemaForRelationshipsObject);
            }
        }
    }

    private void EnsureResourceIdentifierObjectSchemaExists(RelationshipAttribute relationship)
    {
        Type resourceIdentifierObjectType = typeof(ResourceIdentifierObject<>).MakeGenericType(relationship.RightType.ClrType);

        if (!ResourceIdentifierObjectSchemaExists(resourceIdentifierObjectType))
        {
            GenerateResourceIdentifierObjectSchema(resourceIdentifierObjectType);
        }
    }

    private bool ResourceIdentifierObjectSchemaExists(Type resourceIdentifierObjectType)
    {
        return _schemaRepositoryAccessor.Current.TryLookupByType(resourceIdentifierObjectType, out _);
    }

    private void GenerateResourceIdentifierObjectSchema(Type resourceIdentifierObjectType)
    {
        OpenApiSchema referenceSchemaForResourceIdentifierObject =
            _defaultSchemaGenerator.GenerateSchema(resourceIdentifierObjectType, _schemaRepositoryAccessor.Current);

        OpenApiSchema fullSchemaForResourceIdentifierObject =
            _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForResourceIdentifierObject.Reference.Id];

        Type resourceType = resourceIdentifierObjectType.GetGenericArguments()[0];
        fullSchemaForResourceIdentifierObject.Properties[JsonApiPropertyName.Type] = _resourceTypeSchemaGenerator.Get(resourceType);
    }

    private void AddRelationshipSchemaToResourceObject(RelationshipAttribute relationship, OpenApiSchema fullSchemaForRelationshipsObject)
    {
        Type relationshipSchemaType = GetRelationshipSchemaType(relationship, _resourceTypeInfo.ResourceObjectOpenType);

        OpenApiSchema referenceSchemaForRelationship =
            GetReferenceSchemaForRelationship(relationshipSchemaType) ?? CreateRelationshipReferenceSchema(relationshipSchemaType);

        var extendedReferenceSchemaForRelationship = new OpenApiSchema
        {
            AllOf = new List<OpenApiSchema>
            {
                referenceSchemaForRelationship
            },
            Description = _resourceObjectDocumentationReader.GetDocumentationForRelationship(relationship)
        };

        fullSchemaForRelationshipsObject.Properties.Add(relationship.PublicName, extendedReferenceSchemaForRelationship);

        if (IsFieldRequired(relationship))
        {
            fullSchemaForRelationshipsObject.Required.Add(relationship.PublicName);
        }
    }

    private Type GetRelationshipSchemaType(RelationshipAttribute relationship, Type resourceObjectType)
    {
        return resourceObjectType.GetGenericTypeDefinition().IsAssignableTo(typeof(ResourceObjectInResponse<>))
            ? _relationshipTypeFactory.GetForResponse(relationship)
            : _relationshipTypeFactory.GetForRequest(relationship);
    }

    private OpenApiSchema? GetReferenceSchemaForRelationship(Type relationshipSchemaType)
    {
        _schemaRepositoryAccessor.Current.TryLookupByType(relationshipSchemaType, out OpenApiSchema? referenceSchema);
        return referenceSchema;
    }

    private OpenApiSchema CreateRelationshipReferenceSchema(Type relationshipSchemaType)
    {
        OpenApiSchema referenceSchema = _defaultSchemaGenerator.GenerateSchema(relationshipSchemaType, _schemaRepositoryAccessor.Current);

        OpenApiSchema fullSchema = _schemaRepositoryAccessor.Current.Schemas[referenceSchema.Reference.Id];

        if (IsDataPropertyNullableInRelationshipSchemaType(relationshipSchemaType))
        {
            fullSchema.Properties[JsonApiPropertyName.Data].Nullable = true;
        }

        if (IsRelationshipInResponseType(relationshipSchemaType))
        {
            fullSchema.Required.Remove(JsonApiPropertyName.Data);

            fullSchema.ReorderProperties(RelationshipObjectPropertyNamesInOrder);
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
