using System.Reflection;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;

internal sealed class ResourceFieldSchemaBuilder
{
    private readonly SchemaGenerationTracer _schemaGenerationTracer;
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly DataSchemaGenerator _dataSchemaGenerator;
    private readonly LinksVisibilitySchemaGenerator _linksVisibilitySchemaGenerator;
    private readonly ResourceSchemaType _resourceSchemaType;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;
    private readonly RelationshipTypeFactory _relationshipTypeFactory;

    private readonly SchemaRepository _resourceSchemaRepository = new();
    private readonly ResourceDocumentationReader _resourceDocumentationReader = new();
    private readonly IDictionary<string, IOpenApiSchema> _schemasForResourceFields;

    public ResourceFieldSchemaBuilder(SchemaGenerationTracer schemaGenerationTracer, SchemaGenerator defaultSchemaGenerator,
        DataSchemaGenerator dataSchemaGenerator, LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator,
        ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider, RelationshipTypeFactory relationshipTypeFactory,
        ResourceSchemaType resourceSchemaType)
    {
        ArgumentNullException.ThrowIfNull(schemaGenerationTracer);
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);
        ArgumentNullException.ThrowIfNull(dataSchemaGenerator);
        ArgumentNullException.ThrowIfNull(linksVisibilitySchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceSchemaType);
        ArgumentNullException.ThrowIfNull(resourceFieldValidationMetadataProvider);
        ArgumentNullException.ThrowIfNull(relationshipTypeFactory);

        _schemaGenerationTracer = schemaGenerationTracer;
        _defaultSchemaGenerator = defaultSchemaGenerator;
        _dataSchemaGenerator = dataSchemaGenerator;
        _linksVisibilitySchemaGenerator = linksVisibilitySchemaGenerator;
        _resourceSchemaType = resourceSchemaType;
        _resourceFieldValidationMetadataProvider = resourceFieldValidationMetadataProvider;
        _relationshipTypeFactory = relationshipTypeFactory;

        _schemasForResourceFields = GetFieldSchemas();
    }

    private IDictionary<string, IOpenApiSchema> GetFieldSchemas()
    {
        if (!_resourceSchemaRepository.TryLookupByType(_resourceSchemaType.ResourceType.ClrType, out var referenceSchemaForResource))
        {
            referenceSchemaForResource = (OpenApiSchemaReference)_defaultSchemaGenerator.GenerateSchema(_resourceSchemaType.ResourceType.ClrType, _resourceSchemaRepository);
        }

        var inlineSchemaForResource = _resourceSchemaRepository.Schemas[referenceSchemaForResource.Reference.Id].UnwrapLastExtendedSchema();
        return inlineSchemaForResource.Properties;
    }

    public void SetMembersOfAttributes(OpenApiSchema fullSchemaForAttributes, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(fullSchemaForAttributes);
        ArgumentNullException.ThrowIfNull(schemaRepository);
        AssertHasNoProperties(fullSchemaForAttributes);

        var requiredCapability = GetRequiredCapabilityForAttributes(_resourceSchemaType.SchemaOpenType);

        foreach ((var publicName, var schemaForResourceField) in _schemasForResourceFields)
        {
            var matchingAttribute = _resourceSchemaType.ResourceType.FindAttributeByPublicName(publicName);

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

                ((OpenApiSchema)schemaForResourceField).AllOf ??= [];
                var isInlineSchemaType = schemaForResourceField.AllOf.Count == 0;

                // Schemas for types like enum and complex attributes are handled as reference schemas.
                if (!isInlineSchemaType)
                {
                    var referenceSchemaForAttribute = (OpenApiSchemaReference)schemaForResourceField.UnwrapLastExtendedSchema();
                    EnsureAttributeSchemaIsExposed(referenceSchemaForAttribute, matchingAttribute, schemaRepository);
                }

                fullSchemaForAttributes.Properties.Add(matchingAttribute.PublicName, schemaForResourceField);

                bool isNullable = _resourceFieldValidationMetadataProvider.IsNullable(matchingAttribute);
                ((OpenApiSchema)schemaForResourceField).SetNullable(isNullable);

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
        AttrCapabilities? capabilities = null;

        if (resourceDataOpenType == typeof(DataInResponse<>))
        {
            capabilities = AttrCapabilities.AllowView;
        }
        else if (resourceDataOpenType == typeof(DataInCreateRequest<>))
        {
            capabilities = AttrCapabilities.AllowCreate;
        }
        else if (resourceDataOpenType == typeof(DataInUpdateRequest<>))
        {
            capabilities = AttrCapabilities.AllowChange;
        }

        ConsistencyGuard.ThrowIf(capabilities == null);
        return capabilities.Value;
    }

    private void EnsureAttributeSchemaIsExposed(OpenApiSchemaReference referenceSchemaForAttribute, AttrAttribute attribute, SchemaRepository schemaRepository)
    {
        var nonNullableTypeInPropertyType = GetRepresentedTypeForAttributeSchema(attribute);

        if (schemaRepository.TryLookupByType(nonNullableTypeInPropertyType, out _))
        {
            return;
        }

        using var traceScope = _schemaGenerationTracer.TraceStart(this, nonNullableTypeInPropertyType);

        var schemaId = referenceSchemaForAttribute.Reference.Id;
        var fullSchema = (OpenApiSchema)_resourceSchemaRepository.Schemas[schemaId];

        schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(nonNullableTypeInPropertyType, schemaId);

        traceScope.TraceSucceeded(schemaId);
    }

    private Type GetRepresentedTypeForAttributeSchema(AttrAttribute attribute)
    {
        NullabilityInfoContext nullabilityInfoContext = new();
        var attributeNullabilityInfo = nullabilityInfoContext.Create(attribute.Property);

        var isNullable = attributeNullabilityInfo is { ReadState: NullabilityState.Nullable, WriteState: NullabilityState.Nullable };

        var nonNullableTypeInPropertyType = isNullable
            ? Nullable.GetUnderlyingType(attribute.Property.PropertyType) ?? attribute.Property.PropertyType
            : attribute.Property.PropertyType;

        return nonNullableTypeInPropertyType;
    }

    private bool IsFieldRequired(ResourceFieldAttribute field)
    {
        var isCreateResourceSchemaType = _resourceSchemaType.SchemaOpenType == typeof(DataInCreateRequest<>);
        return isCreateResourceSchemaType && _resourceFieldValidationMetadataProvider.IsRequired(field);
    }

    public void SetMembersOfRelationships(OpenApiSchema fullSchemaForRelationships, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(fullSchemaForRelationships);
        ArgumentNullException.ThrowIfNull(schemaRepository);
        AssertHasNoProperties(fullSchemaForRelationships);

        foreach (var publicName in _schemasForResourceFields.Keys)
        {
            var matchingRelationship = _resourceSchemaType.ResourceType.FindRelationshipByPublicName(publicName);

            if (matchingRelationship != null)
            {
                var identifierSchemaOpenType = forRequestSchema ? typeof(IdentifierInRequest<>) : typeof(IdentifierInResponse<>);
                var identifierSchemaConstructedType = identifierSchemaOpenType.MakeGenericType(matchingRelationship.RightType.ClrType);

                _ = _dataSchemaGenerator.GenerateSchema(identifierSchemaConstructedType, forRequestSchema, schemaRepository);
                AddRelationshipSchemaToResourceData(matchingRelationship, fullSchemaForRelationships, schemaRepository);
            }
        }
    }

    private void AddRelationshipSchemaToResourceData(RelationshipAttribute relationship, OpenApiSchema fullSchemaForRelationships,
        SchemaRepository schemaRepository)
    {
        var relationshipSchemaType = GetRelationshipSchemaType(relationship, _resourceSchemaType.SchemaOpenType);

        var referenceSchemaForRelationship = GetReferenceSchemaForRelationship(relationshipSchemaType, schemaRepository) ??
            CreateReferenceSchemaForRelationship(relationshipSchemaType, schemaRepository);

        var extendedReferenceSchemaForRelationship = referenceSchemaForRelationship.WrapInExtendedSchema();
        extendedReferenceSchemaForRelationship.Description = _resourceDocumentationReader.GetDocumentationForRelationship(relationship);

        fullSchemaForRelationships.Properties.Add(relationship.PublicName, extendedReferenceSchemaForRelationship);

        if (IsFieldRequired(relationship))
        {
            fullSchemaForRelationships.Required.Add(relationship.PublicName);
        }
    }

    private Type GetRelationshipSchemaType(RelationshipAttribute relationship, Type openSchemaType)
    {
        var isResponseSchemaType = openSchemaType.IsAssignableTo(typeof(DataInResponse<>));
        return isResponseSchemaType ? _relationshipTypeFactory.GetForResponse(relationship) : _relationshipTypeFactory.GetForRequest(relationship);
    }

    private OpenApiSchemaReference? GetReferenceSchemaForRelationship(Type relationshipSchemaType, SchemaRepository schemaRepository)
    {
        return schemaRepository.TryLookupByType(relationshipSchemaType, out var referenceSchema) ? referenceSchema : null;
    }

    private OpenApiSchemaReference CreateReferenceSchemaForRelationship(Type relationshipSchemaType, SchemaRepository schemaRepository)
    {
        using var traceScope = _schemaGenerationTracer.TraceStart(this, relationshipSchemaType);

        var referenceSchema = (OpenApiSchemaReference)_defaultSchemaGenerator.GenerateSchema(relationshipSchemaType, schemaRepository);

        var fullSchema = (OpenApiSchema)schemaRepository.Schemas[referenceSchema.Reference.Id];

        if (JsonApiSchemaFacts.HasNullableDataProperty(relationshipSchemaType))
        {
            ((OpenApiSchema)fullSchema.Properties[JsonApiPropertyName.Data]).SetNullable(true);
        }

        if (JsonApiSchemaFacts.IsRelationshipInResponseType(relationshipSchemaType))
        {
            _linksVisibilitySchemaGenerator.UpdateSchemaForRelationship(relationshipSchemaType, fullSchema, schemaRepository);
        }

        traceScope.TraceSucceeded(referenceSchema.Reference.Id);
        return referenceSchema;
    }

    private static void AssertHasNoProperties(OpenApiSchema fullSchema)
    {
        ConsistencyGuard.ThrowIf(fullSchema.Properties.Count > 0);
    }
}
