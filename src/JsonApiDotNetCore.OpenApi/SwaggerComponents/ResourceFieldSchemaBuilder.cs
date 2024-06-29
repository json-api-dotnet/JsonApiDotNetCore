using System.Reflection;
using JsonApiDotNetCore.OpenApi.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.SchemaGenerators.Components;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class ResourceFieldSchemaBuilder
{
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

    public void SetMembersOfAttributes(OpenApiSchema fullSchemaForAttributes, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(fullSchemaForAttributes);
        ArgumentGuard.NotNull(schemaRepository);

        AttrCapabilities requiredCapability = GetRequiredCapabilityForAttributes(_resourceTypeInfo.ResourceDataOpenType);

        foreach ((string fieldName, OpenApiSchema resourceFieldSchema) in _schemasForResourceFields)
        {
            AttrAttribute? matchingAttribute = _resourceTypeInfo.ResourceType.FindAttributeByPublicName(fieldName);

            if (matchingAttribute != null && matchingAttribute.Capabilities.HasFlag(requiredCapability))
            {
                if (forRequestSchema)
                {
                    if (matchingAttribute.Property.SetMethod == null)
                    {
                        continue;
                    }
                }
                else
                {
                    if (matchingAttribute.Property.GetMethod == null)
                    {
                        continue;
                    }
                }

                bool isInlineSchemaType = resourceFieldSchema.AllOf.Count == 0;

                // Schemas for types like enum and complex attributes are handled as reference schemas.
                if (!isInlineSchemaType)
                {
                    EnsureAttributeSchemaIsExposed(resourceFieldSchema.UnwrapLastExtendedSchema(), matchingAttribute, schemaRepository);
                }

                fullSchemaForAttributes.Properties.Add(matchingAttribute.PublicName, resourceFieldSchema);

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
            resourceDataOpenType == typeof(DataInCreateResourceRequest<>) ? AttrCapabilities.AllowCreate :
            resourceDataOpenType == typeof(DataInUpdateResourceRequest<>) ? AttrCapabilities.AllowChange : throw new UnreachableCodeException();
    }

    private void EnsureAttributeSchemaIsExposed(OpenApiSchema referenceSchemaForAttribute, AttrAttribute attribute, SchemaRepository schemaRepository)
    {
        Type nonNullableTypeInPropertyType = GetRepresentedTypeForAttributeSchema(attribute);

        if (schemaRepository.TryLookupByType(nonNullableTypeInPropertyType, out _))
        {
            return;
        }

        string schemaId = referenceSchemaForAttribute.Reference.Id;

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
        bool isCreateResourceSchemaType = _resourceTypeInfo.ResourceDataOpenType == typeof(DataInCreateResourceRequest<>);
        return isCreateResourceSchemaType && _resourceFieldValidationMetadataProvider.IsRequired(field);
    }

    public void SetMembersOfRelationships(OpenApiSchema fullSchemaForRelationships, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(fullSchemaForRelationships);
        ArgumentGuard.NotNull(schemaRepository);

        foreach (string fieldName in _schemasForResourceFields.Keys)
        {
            RelationshipAttribute? matchingRelationship = _resourceTypeInfo.ResourceType.FindRelationshipByPublicName(fieldName);

            if (matchingRelationship != null)
            {
                _ = _resourceIdentifierSchemaGenerator.GenerateSchema(matchingRelationship.RightType, forRequestSchema, schemaRepository);
                AddRelationshipSchemaToResourceData(matchingRelationship, fullSchemaForRelationships, schemaRepository);
            }
        }
    }

    private void AddRelationshipSchemaToResourceData(RelationshipAttribute relationship, OpenApiSchema fullSchemaForRelationships,
        SchemaRepository schemaRepository)
    {
        Type relationshipSchemaType = GetRelationshipSchemaType(relationship, _resourceTypeInfo.ResourceDataOpenType);

        OpenApiSchema referenceSchemaForRelationship = GetReferenceSchemaForRelationship(relationshipSchemaType, schemaRepository) ??
            CreateReferenceSchemaForRelationship(relationshipSchemaType, schemaRepository);

        OpenApiSchema extendedReferenceSchemaForRelationship = referenceSchemaForRelationship.WrapInExtendedSchema();
        extendedReferenceSchemaForRelationship.Description = _resourceDocumentationReader.GetDocumentationForRelationship(relationship);

        fullSchemaForRelationships.Properties.Add(relationship.PublicName, extendedReferenceSchemaForRelationship);

        if (IsFieldRequired(relationship))
        {
            fullSchemaForRelationships.Required.Add(relationship.PublicName);
        }
    }

    private Type GetRelationshipSchemaType(RelationshipAttribute relationship, Type resourceDataConstructedType)
    {
        bool isResponseSchemaType = resourceDataConstructedType.ConstructedToOpenType().IsAssignableTo(typeof(ResourceDataInResponse<>));
        return isResponseSchemaType ? _relationshipTypeFactory.GetForResponse(relationship) : _relationshipTypeFactory.GetForRequest(relationship);
    }

    private OpenApiSchema? GetReferenceSchemaForRelationship(Type relationshipSchemaType, SchemaRepository schemaRepository)
    {
        return schemaRepository.TryLookupByType(relationshipSchemaType, out OpenApiSchema? referenceSchema) ? referenceSchema : null;
    }

    private OpenApiSchema CreateReferenceSchemaForRelationship(Type relationshipSchemaType, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchema = _defaultSchemaGenerator.GenerateSchema(relationshipSchemaType, schemaRepository);

        OpenApiSchema fullSchema = schemaRepository.Schemas[referenceSchema.Reference.Id];

        if (JsonApiSchemaFacts.HasNullableDataProperty(relationshipSchemaType))
        {
            fullSchema.Properties[JsonApiPropertyName.Data].Nullable = true;
        }

        if (JsonApiSchemaFacts.IsRelationshipInResponseType(relationshipSchemaType))
        {
            _linksVisibilitySchemaGenerator.UpdateSchemaForRelationship(relationshipSchemaType, fullSchema, schemaRepository);

            fullSchema.Required.Remove(JsonApiPropertyName.Data);
        }

        return referenceSchema;
    }
}
