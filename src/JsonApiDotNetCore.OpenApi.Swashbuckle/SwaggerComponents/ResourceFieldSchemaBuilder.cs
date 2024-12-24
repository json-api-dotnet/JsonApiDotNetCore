using System.Reflection;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;

internal sealed class ResourceFieldSchemaBuilder
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly ResourceIdentifierSchemaGenerator _resourceIdentifierSchemaGenerator;
    private readonly LinksVisibilitySchemaGenerator _linksVisibilitySchemaGenerator;
    private readonly ResourceSchemaType _resourceSchemaType;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;
    private readonly RelationshipTypeFactory _relationshipTypeFactory;

    private readonly SchemaRepository _resourceSchemaRepository = new();
    private readonly ResourceDocumentationReader _resourceDocumentationReader = new();
    private readonly IDictionary<string, OpenApiSchema> _schemasForResourceFields;

    public ResourceFieldSchemaBuilder(SchemaGenerator defaultSchemaGenerator, ResourceIdentifierSchemaGenerator resourceIdentifierSchemaGenerator,
        LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator, ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider,
        RelationshipTypeFactory relationshipTypeFactory, ResourceSchemaType resourceSchemaType)
    {
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceIdentifierSchemaGenerator);
        ArgumentNullException.ThrowIfNull(linksVisibilitySchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceSchemaType);
        ArgumentNullException.ThrowIfNull(resourceFieldValidationMetadataProvider);
        ArgumentNullException.ThrowIfNull(relationshipTypeFactory);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _resourceIdentifierSchemaGenerator = resourceIdentifierSchemaGenerator;
        _linksVisibilitySchemaGenerator = linksVisibilitySchemaGenerator;
        _resourceSchemaType = resourceSchemaType;
        _resourceFieldValidationMetadataProvider = resourceFieldValidationMetadataProvider;
        _relationshipTypeFactory = relationshipTypeFactory;

        _schemasForResourceFields = GetFieldSchemas();
    }

    private IDictionary<string, OpenApiSchema> GetFieldSchemas()
    {
        if (!_resourceSchemaRepository.TryLookupByType(_resourceSchemaType.ResourceType.ClrType, out OpenApiSchema referenceSchemaForResource))
        {
            referenceSchemaForResource = _defaultSchemaGenerator.GenerateSchema(_resourceSchemaType.ResourceType.ClrType, _resourceSchemaRepository);
        }

        OpenApiSchema fullSchemaForResource = _resourceSchemaRepository.Schemas[referenceSchemaForResource.Reference.Id];
        return fullSchemaForResource.Properties;
    }

    public void SetMembersOfAttributes(OpenApiSchema fullSchemaForAttributes, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(fullSchemaForAttributes);
        ArgumentNullException.ThrowIfNull(schemaRepository);
        AssertHasNoProperties(fullSchemaForAttributes);

        AttrCapabilities requiredCapability = GetRequiredCapabilityForAttributes(_resourceSchemaType.SchemaOpenType);

        foreach ((string publicName, OpenApiSchema schemaForResourceField) in _schemasForResourceFields)
        {
            AttrAttribute? matchingAttribute = _resourceSchemaType.ResourceType.FindAttributeByPublicName(publicName);

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

                bool isInlineSchemaType = schemaForResourceField.AllOf.Count == 0;

                // Schemas for types like enum and complex attributes are handled as reference schemas.
                if (!isInlineSchemaType)
                {
                    OpenApiSchema referenceSchemaForAttribute = schemaForResourceField.UnwrapLastExtendedSchema();
                    EnsureAttributeSchemaIsExposed(referenceSchemaForAttribute, matchingAttribute, schemaRepository);
                }

                fullSchemaForAttributes.Properties.Add(matchingAttribute.PublicName, schemaForResourceField);

                schemaForResourceField.Nullable = _resourceFieldValidationMetadataProvider.IsNullable(matchingAttribute);

                if (IsFieldRequired(matchingAttribute))
                {
                    fullSchemaForAttributes.Required.Add(matchingAttribute.PublicName);
                }

                schemaForResourceField.Description = _resourceDocumentationReader.GetDocumentationForAttribute(matchingAttribute);
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
        bool isCreateResourceSchemaType = _resourceSchemaType.SchemaOpenType == typeof(DataInCreateResourceRequest<>);
        return isCreateResourceSchemaType && _resourceFieldValidationMetadataProvider.IsRequired(field);
    }

    public void SetMembersOfRelationships(OpenApiSchema fullSchemaForRelationships, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(fullSchemaForRelationships);
        ArgumentNullException.ThrowIfNull(schemaRepository);
        AssertHasNoProperties(fullSchemaForRelationships);

        foreach (string publicName in _schemasForResourceFields.Keys)
        {
            RelationshipAttribute? matchingRelationship = _resourceSchemaType.ResourceType.FindRelationshipByPublicName(publicName);

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
        Type relationshipSchemaType = GetRelationshipSchemaType(relationship, _resourceSchemaType.SchemaOpenType);

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

    private Type GetRelationshipSchemaType(RelationshipAttribute relationship, Type openSchemaType)
    {
        bool isResponseSchemaType = openSchemaType.IsAssignableTo(typeof(ResourceDataInResponse<>));
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
        }

        return referenceSchema;
    }

    private static void AssertHasNoProperties(OpenApiSchema fullSchema)
    {
        if (fullSchema.Properties.Count > 0)
        {
            throw new UnreachableCodeException();
        }
    }
}
