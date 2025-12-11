using System.Reflection;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.OpenApi;
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
        if (!_resourceSchemaRepository.TryLookupByTypeSafe(_resourceSchemaType.ResourceType.ClrType, out OpenApiSchemaReference? referenceSchemaForResource))
        {
            referenceSchemaForResource = _defaultSchemaGenerator.GenerateSchema(_resourceSchemaType.ResourceType.ClrType, _resourceSchemaRepository)
                .AsReferenceSchema();
        }

        OpenApiSchema inlineSchemaForResource =
            _resourceSchemaRepository.Schemas[referenceSchemaForResource.GetReferenceId()].UnwrapLastExtendedSchema().AsInlineSchema();

        return inlineSchemaForResource.Properties ?? new Dictionary<string, IOpenApiSchema>();
    }

    public void SetMembersOfAttributes(OpenApiSchema inlineSchemaForAttributes, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(inlineSchemaForAttributes);
        ArgumentNullException.ThrowIfNull(schemaRepository);
        AssertHasNoProperties(inlineSchemaForAttributes);

        AttrCapabilities requiredCapability = GetRequiredCapabilityForAttributes(_resourceSchemaType.SchemaOpenType);

        foreach ((string publicName, IOpenApiSchema schemaForResourceField) in _schemasForResourceFields)
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

                bool isInlineSchemaType = schemaForResourceField.AllOf == null || schemaForResourceField.AllOf.Count == 0;

                // Schemas for types like enum and complex attributes are handled as reference schemas.
                if (!isInlineSchemaType)
                {
                    OpenApiSchemaReference referenceSchemaForAttribute = schemaForResourceField.UnwrapLastExtendedSchema().AsReferenceSchema();
                    EnsureAttributeSchemaIsExposed(referenceSchemaForAttribute, matchingAttribute, schemaRepository);
                }

                inlineSchemaForAttributes.Properties ??= new Dictionary<string, IOpenApiSchema>();
                inlineSchemaForAttributes.Properties.Add(matchingAttribute.PublicName, schemaForResourceField);

                if (schemaForResourceField is OpenApiSchema inlineSchemaForResourceField)
                {
                    bool isNullable = _resourceFieldValidationMetadataProvider.IsNullable(matchingAttribute);
                    inlineSchemaForResourceField.SetNullable(isNullable);
                }

                if (IsFieldRequired(matchingAttribute))
                {
                    inlineSchemaForAttributes.Required ??= new SortedSet<string>(StringComparer.Ordinal);
                    inlineSchemaForAttributes.Required.Add(matchingAttribute.PublicName);
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
        Type nonNullableTypeInPropertyType = GetRepresentedTypeForAttributeSchema(attribute);

        if (schemaRepository.TryLookupByTypeSafe(nonNullableTypeInPropertyType, out _))
        {
            return;
        }

        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this, nonNullableTypeInPropertyType);

        string schemaId = referenceSchemaForAttribute.GetReferenceId();
        OpenApiSchema inlineSchema = _resourceSchemaRepository.Schemas[schemaId].AsInlineSchema();

        schemaRepository.AddDefinition(schemaId, inlineSchema);
        schemaRepository.RegisterType(nonNullableTypeInPropertyType, schemaId);

        traceScope.TraceSucceeded(schemaId);
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
        bool isCreateResourceSchemaType = _resourceSchemaType.SchemaOpenType == typeof(DataInCreateRequest<>);
        return isCreateResourceSchemaType && _resourceFieldValidationMetadataProvider.IsRequired(field);
    }

    public void SetMembersOfRelationships(OpenApiSchema inlineSchemaForRelationships, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(inlineSchemaForRelationships);
        ArgumentNullException.ThrowIfNull(schemaRepository);
        AssertHasNoProperties(inlineSchemaForRelationships);

        foreach (string publicName in _schemasForResourceFields.Keys)
        {
            RelationshipAttribute? matchingRelationship = _resourceSchemaType.ResourceType.FindRelationshipByPublicName(publicName);

            if (matchingRelationship != null)
            {
                Type identifierSchemaOpenType = forRequestSchema ? typeof(IdentifierInRequest<>) : typeof(IdentifierInResponse<>);
                Type identifierSchemaConstructedType = identifierSchemaOpenType.MakeGenericType(matchingRelationship.RightType.ClrType);

                _ = _dataSchemaGenerator.GenerateSchema(identifierSchemaConstructedType, forRequestSchema, schemaRepository);
                AddRelationshipSchemaToResourceData(matchingRelationship, inlineSchemaForRelationships, schemaRepository);
            }
        }
    }

    private void AddRelationshipSchemaToResourceData(RelationshipAttribute relationship, OpenApiSchema inlineSchemaForRelationships,
        SchemaRepository schemaRepository)
    {
        Type relationshipSchemaType = GetRelationshipSchemaType(relationship, _resourceSchemaType.SchemaOpenType);

        OpenApiSchemaReference referenceSchemaForRelationship = GetReferenceSchemaForRelationship(relationshipSchemaType, schemaRepository) ??
            CreateReferenceSchemaForRelationship(relationshipSchemaType, schemaRepository);

        OpenApiSchema extendedReferenceSchemaForRelationship = referenceSchemaForRelationship.WrapInExtendedSchema();
        extendedReferenceSchemaForRelationship.Description = _resourceDocumentationReader.GetDocumentationForRelationship(relationship);

        inlineSchemaForRelationships.Properties ??= new Dictionary<string, IOpenApiSchema>();
        inlineSchemaForRelationships.Properties.Add(relationship.PublicName, extendedReferenceSchemaForRelationship);

        if (IsFieldRequired(relationship))
        {
            inlineSchemaForRelationships.Required ??= new SortedSet<string>(StringComparer.Ordinal);
            inlineSchemaForRelationships.Required.Add(relationship.PublicName);
        }
    }

    private Type GetRelationshipSchemaType(RelationshipAttribute relationship, Type openSchemaType)
    {
        bool isResponseSchemaType = openSchemaType.IsAssignableTo(typeof(DataInResponse<>));
        return isResponseSchemaType ? _relationshipTypeFactory.GetForResponse(relationship) : _relationshipTypeFactory.GetForRequest(relationship);
    }

    private OpenApiSchemaReference? GetReferenceSchemaForRelationship(Type relationshipSchemaType, SchemaRepository schemaRepository)
    {
        return schemaRepository.TryLookupByTypeSafe(relationshipSchemaType, out OpenApiSchemaReference? referenceSchema) ? referenceSchema : null;
    }

    private OpenApiSchemaReference CreateReferenceSchemaForRelationship(Type relationshipSchemaType, SchemaRepository schemaRepository)
    {
        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this, relationshipSchemaType);

        OpenApiSchemaReference referenceSchema = _defaultSchemaGenerator.GenerateSchema(relationshipSchemaType, schemaRepository).AsReferenceSchema();
        OpenApiSchema inlineSchema = schemaRepository.Schemas[referenceSchema.GetReferenceId()].AsInlineSchema();

        if (JsonApiSchemaFacts.HasNullableDataProperty(relationshipSchemaType))
        {
            inlineSchema.Properties ??= new Dictionary<string, IOpenApiSchema>();
            inlineSchema.Properties[JsonApiPropertyName.Data].AsInlineSchema().SetNullable(true);
        }

        if (JsonApiSchemaFacts.IsRelationshipInResponseType(relationshipSchemaType))
        {
            _linksVisibilitySchemaGenerator.UpdateSchemaForRelationship(relationshipSchemaType, inlineSchema, schemaRepository);
        }

        traceScope.TraceSucceeded(referenceSchema.GetReferenceId());
        return referenceSchema;
    }

    private static void AssertHasNoProperties(OpenApiSchema inlineSchema)
    {
        ConsistencyGuard.ThrowIf(inlineSchema.Properties is { Count: > 0 });
    }
}
