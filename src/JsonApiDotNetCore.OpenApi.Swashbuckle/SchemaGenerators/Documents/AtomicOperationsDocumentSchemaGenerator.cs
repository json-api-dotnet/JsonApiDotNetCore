using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Documents;

/// <summary>
/// Generates the OpenAPI component schema for an atomic:operations request/response document.
/// </summary>
internal sealed class AtomicOperationsDocumentSchemaGenerator : DocumentSchemaGenerator
{
    private static readonly Type AtomicOperationAbstractType = typeof(AtomicOperation);

    private readonly SchemaGenerationTracer _schemaGenerationTracer;
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly AtomicOperationCodeSchemaGenerator _atomicOperationCodeSchemaGenerator;
    private readonly DataSchemaGenerator _dataSchemaGenerator;
    private readonly RelationshipIdentifierSchemaGenerator _relationshipIdentifierSchemaGenerator;
    private readonly DataContainerSchemaGenerator _dataContainerSchemaGenerator;
    private readonly MetaSchemaGenerator _metaSchemaGenerator;
    private readonly IAtomicOperationFilter _atomicOperationFilter;
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;
    private readonly IResourceGraph _resourceGraph;

    public AtomicOperationsDocumentSchemaGenerator(SchemaGenerationTracer schemaGenerationTracer, SchemaGenerator defaultSchemaGenerator,
        AtomicOperationCodeSchemaGenerator atomicOperationCodeSchemaGenerator, DataSchemaGenerator dataSchemaGenerator,
        RelationshipIdentifierSchemaGenerator relationshipIdentifierSchemaGenerator, DataContainerSchemaGenerator dataContainerSchemaGenerator,
        MetaSchemaGenerator metaSchemaGenerator, LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator, IAtomicOperationFilter atomicOperationFilter,
        JsonApiSchemaIdSelector schemaIdSelector, ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider, IJsonApiOptions options,
        IResourceGraph resourceGraph)
        : base(schemaGenerationTracer, metaSchemaGenerator, linksVisibilitySchemaGenerator, options)
    {
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);
        ArgumentNullException.ThrowIfNull(atomicOperationCodeSchemaGenerator);
        ArgumentNullException.ThrowIfNull(dataSchemaGenerator);
        ArgumentNullException.ThrowIfNull(relationshipIdentifierSchemaGenerator);
        ArgumentNullException.ThrowIfNull(dataContainerSchemaGenerator);
        ArgumentNullException.ThrowIfNull(atomicOperationFilter);
        ArgumentNullException.ThrowIfNull(schemaIdSelector);
        ArgumentNullException.ThrowIfNull(resourceFieldValidationMetadataProvider);
        ArgumentNullException.ThrowIfNull(resourceGraph);

        _schemaGenerationTracer = schemaGenerationTracer;
        _defaultSchemaGenerator = defaultSchemaGenerator;
        _atomicOperationCodeSchemaGenerator = atomicOperationCodeSchemaGenerator;
        _dataSchemaGenerator = dataSchemaGenerator;
        _relationshipIdentifierSchemaGenerator = relationshipIdentifierSchemaGenerator;
        _dataContainerSchemaGenerator = dataContainerSchemaGenerator;
        _metaSchemaGenerator = metaSchemaGenerator;
        _atomicOperationFilter = atomicOperationFilter;
        _schemaIdSelector = schemaIdSelector;
        _resourceFieldValidationMetadataProvider = resourceFieldValidationMetadataProvider;
        _resourceGraph = resourceGraph;
    }

    public override bool CanGenerate(Type schemaType)
    {
        return schemaType == typeof(OperationsRequestDocument) || schemaType == typeof(OperationsResponseDocument);
    }

    protected override OpenApiSchemaReference GenerateDocumentSchema(Type schemaType, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        bool isRequestSchema = schemaType == typeof(OperationsRequestDocument);

        if (isRequestSchema)
        {
            GenerateSchemasForRequestDocument(schemaRepository);
        }
        else
        {
            GenerateSchemasForResponseDocument(schemaRepository);
        }

        return _defaultSchemaGenerator.GenerateSchema(schemaType, schemaRepository).AsReferenceSchema();
    }

    private void GenerateSchemasForRequestDocument(SchemaRepository schemaRepository)
    {
        _ = GenerateSchemaForAbstractOperation(schemaRepository);

        foreach (ResourceType resourceType in _resourceGraph.GetResourceTypes().Where(resourceType => resourceType.BaseType == null))
        {
            GenerateSchemaForOperation(resourceType, schemaRepository);
        }
    }

    private OpenApiSchemaReference GenerateSchemaForAbstractOperation(SchemaRepository schemaRepository)
    {
        if (schemaRepository.TryLookupByTypeSafe(AtomicOperationAbstractType, out OpenApiSchemaReference? referenceSchema))
        {
            return referenceSchema;
        }

        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this, AtomicOperationAbstractType);

        OpenApiSchemaReference referenceSchemaForMeta = _metaSchemaGenerator.GenerateSchema(schemaRepository);

        var inlineSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = new SortedSet<string>([OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName], StringComparer.Ordinal),
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                [OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String
                },
                [referenceSchemaForMeta.GetReferenceId()] = referenceSchemaForMeta.WrapInExtendedSchema()
            },
            AdditionalPropertiesAllowed = false,
            Discriminator = new OpenApiDiscriminator
            {
                PropertyName = OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName
            },
            Extensions = new SortedDictionary<string, IOpenApiExtension>(StringComparer.Ordinal)
            {
                ["x-abstract"] = new JsonNodeExtension(true)
            }
        };

        string schemaId = _schemaIdSelector.GetSchemaId(AtomicOperationAbstractType);

        referenceSchema = schemaRepository.AddDefinition(schemaId, inlineSchema);
        schemaRepository.RegisterType(AtomicOperationAbstractType, schemaId);

        traceScope.TraceSucceeded(schemaId);
        return referenceSchema;
    }

    private void GenerateSchemaForOperation(ResourceType resourceType, SchemaRepository schemaRepository)
    {
        GenerateSchemaForResourceOperation(typeof(CreateOperation<>), resourceType, AtomicOperationCode.Add, schemaRepository);
        GenerateSchemaForResourceOperation(typeof(UpdateOperation<>), resourceType, AtomicOperationCode.Update, schemaRepository);
        GenerateSchemaForResourceOperation(typeof(DeleteOperation<>), resourceType, AtomicOperationCode.Remove, schemaRepository);

        foreach (RelationshipAttribute relationship in GetRelationshipsInTypeHierarchy(resourceType))
        {
            if (relationship is HasOneAttribute)
            {
                GenerateSchemaForRelationshipOperation(typeof(UpdateToOneRelationshipOperation<>), relationship, AtomicOperationCode.Update, schemaRepository);
            }
            else
            {
                GenerateSchemaForRelationshipOperation(typeof(AddToRelationshipOperation<>), relationship, AtomicOperationCode.Add, schemaRepository);
                GenerateSchemaForRelationshipOperation(typeof(UpdateToManyRelationshipOperation<>), relationship, AtomicOperationCode.Update, schemaRepository);
                GenerateSchemaForRelationshipOperation(typeof(RemoveFromRelationshipOperation<>), relationship, AtomicOperationCode.Remove, schemaRepository);
            }
        }
    }

    private void GenerateSchemaForResourceOperation(Type operationOpenType, ResourceType resourceType, AtomicOperationCode operationCode,
        SchemaRepository schemaRepository)
    {
        WriteOperationKind writeOperation = GetKindOfResourceOperation(operationCode);

        if (IsResourceTypeEnabled(resourceType, writeOperation))
        {
            Type operationConstructedType = ChangeResourceTypeInSchemaType(operationOpenType, resourceType);

            using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this, operationConstructedType);

            bool needsEmptyDerivedSchema = resourceType.BaseType != null && _atomicOperationFilter.IsEnabled(resourceType.BaseType, writeOperation);

            if (!needsEmptyDerivedSchema)
            {
                Type identifierSchemaType = typeof(IdentifierInRequest<>).MakeGenericType(resourceType.ClrType);
                _ = _dataSchemaGenerator.GenerateSchema(identifierSchemaType, true, schemaRepository);

                bool hasDataProperty = operationOpenType != typeof(DeleteOperation<>);

                if (hasDataProperty)
                {
                    _dataContainerSchemaGenerator.GenerateSchema(operationConstructedType, resourceType, true, false, schemaRepository);
                }
            }

            OpenApiSchemaReference referenceSchemaForOperation =
                _defaultSchemaGenerator.GenerateSchema(operationConstructedType, schemaRepository).AsReferenceSchema();

            OpenApiSchema compositeInlineSchemaForOperation = schemaRepository.Schemas[referenceSchemaForOperation.GetReferenceId()].AsInlineSchema();
            compositeInlineSchemaForOperation.AdditionalPropertiesAllowed = false;
            OpenApiSchema inlineSchemaForOperation = compositeInlineSchemaForOperation.UnwrapLastExtendedSchema().AsInlineSchema();

            if (needsEmptyDerivedSchema)
            {
                Type baseOperationSchemaType = ChangeResourceTypeInSchemaType(operationOpenType, resourceType.BaseType!);
                OpenApiSchemaReference referenceSchemaForBaseOperation = schemaRepository.LookupByType(baseOperationSchemaType);

                RemoveProperties(inlineSchemaForOperation);
                compositeInlineSchemaForOperation.AllOf ??= new List<IOpenApiSchema>();
                compositeInlineSchemaForOperation.AllOf[0] = referenceSchemaForBaseOperation;
            }
            else
            {
                SetOperationCode(inlineSchemaForOperation, operationCode, schemaRepository);
            }

            MapInDiscriminator(referenceSchemaForOperation, schemaRepository);

            traceScope.TraceSucceeded(referenceSchemaForOperation.GetReferenceId());
        }

        foreach (ResourceType derivedType in resourceType.DirectlyDerivedTypes)
        {
            GenerateSchemaForResourceOperation(operationOpenType, derivedType, operationCode, schemaRepository);
        }
    }

    private static WriteOperationKind GetKindOfResourceOperation(AtomicOperationCode operationCode)
    {
        WriteOperationKind? writeOperation = null;

        if (operationCode == AtomicOperationCode.Add)
        {
            writeOperation = WriteOperationKind.CreateResource;
        }
        else if (operationCode == AtomicOperationCode.Update)
        {
            writeOperation = WriteOperationKind.UpdateResource;
        }
        else if (operationCode == AtomicOperationCode.Remove)
        {
            writeOperation = WriteOperationKind.DeleteResource;
        }

        ConsistencyGuard.ThrowIf(writeOperation == null);
        return writeOperation.Value;
    }

    private bool IsResourceTypeEnabled(ResourceType resourceType, WriteOperationKind writeOperation)
    {
        return _atomicOperationFilter.IsEnabled(resourceType, writeOperation);
    }

    private static Type ChangeResourceTypeInSchemaType(Type schemaOpenType, ResourceType resourceType)
    {
        return schemaOpenType.MakeGenericType(resourceType.ClrType);
    }

    private static void RemoveProperties(OpenApiSchema inlineSchema)
    {
        if (inlineSchema.Properties != null)
        {
            foreach (string propertyName in inlineSchema.Properties.Keys)
            {
                inlineSchema.Properties.Remove(propertyName);
                inlineSchema.Required?.Remove(propertyName);
            }
        }
    }

    private void SetOperationCode(OpenApiSchema inlineSchema, AtomicOperationCode operationCode, SchemaRepository schemaRepository)
    {
        OpenApiSchemaReference referenceSchema = _atomicOperationCodeSchemaGenerator.GenerateSchema(operationCode, schemaRepository);
        inlineSchema.Properties ??= new Dictionary<string, IOpenApiSchema>();
        inlineSchema.Properties[JsonApiPropertyName.Op] = referenceSchema.WrapInExtendedSchema();
    }

    private static void MapInDiscriminator(OpenApiSchemaReference referenceSchemaForOperation, SchemaRepository schemaRepository)
    {
        OpenApiSchemaReference referenceSchemaForAbstractOperation = schemaRepository.LookupByType(AtomicOperationAbstractType);
        OpenApiSchema inlineSchemaForAbstractOperation = schemaRepository.Schemas[referenceSchemaForAbstractOperation.GetReferenceId()].AsInlineSchema();
        AddToDiscriminatorMapping(inlineSchemaForAbstractOperation, referenceSchemaForOperation.GetReferenceId(), referenceSchemaForOperation);
    }

    private static void AddToDiscriminatorMapping(OpenApiSchema inlineSchema, string schemaId, OpenApiSchemaReference mappingValueReferenceSchema)
    {
        ConsistencyGuard.ThrowIf(inlineSchema.Discriminator is null);

        inlineSchema.Discriminator.Mapping ??= new SortedDictionary<string, OpenApiSchemaReference>(StringComparer.Ordinal);
        inlineSchema.Discriminator.Mapping.Add(schemaId, mappingValueReferenceSchema);
    }

    private static HashSet<RelationshipAttribute> GetRelationshipsInTypeHierarchy(ResourceType baseType)
    {
        HashSet<RelationshipAttribute> relationships = baseType.Relationships.ToHashSet();

        if (baseType.IsPartOfTypeHierarchy())
        {
            IncludeRelationshipsInDirectlyDerivedTypes(baseType, relationships);
        }

        return relationships;
    }

    private static void IncludeRelationshipsInDirectlyDerivedTypes(ResourceType baseType, HashSet<RelationshipAttribute> relationships)
    {
        foreach (ResourceType derivedType in baseType.DirectlyDerivedTypes)
        {
            IncludeRelationshipsInDerivedType(derivedType, relationships);
        }
    }

    private static void IncludeRelationshipsInDerivedType(ResourceType derivedType, HashSet<RelationshipAttribute> relationships)
    {
        foreach (RelationshipAttribute relationshipInDerivedType in derivedType.Relationships)
        {
            relationships.Add(relationshipInDerivedType);
        }

        IncludeRelationshipsInDirectlyDerivedTypes(derivedType, relationships);
    }

    private void GenerateSchemaForRelationshipOperation(Type operationOpenType, RelationshipAttribute relationship, AtomicOperationCode operationCode,
        SchemaRepository schemaRepository)
    {
        WriteOperationKind writeOperation = GetKindOfRelationshipOperation(operationCode);

        if (!IsRelationshipEnabled(relationship, writeOperation))
        {
            return;
        }

        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this, operationOpenType, relationship);

        RelationshipAttribute? relationshipInAnyBaseResourceType = GetRelationshipEnabledInAnyBase(relationship, writeOperation);

        OpenApiSchemaReference? referenceSchemaForRelationshipIdentifier;

        if (relationshipInAnyBaseResourceType == null)
        {
            Type rightSchemaType = typeof(IdentifierInRequest<>).MakeGenericType(relationship.RightType.ClrType);
            _ = _dataSchemaGenerator.GenerateSchema(rightSchemaType, true, schemaRepository);

            referenceSchemaForRelationshipIdentifier = _relationshipIdentifierSchemaGenerator.GenerateSchema(relationship, schemaRepository);
        }
        else
        {
            referenceSchemaForRelationshipIdentifier = null;
        }

        Type operationConstructedType = ChangeResourceTypeInSchemaType(operationOpenType, relationship.RightType);
        _dataContainerSchemaGenerator.GenerateSchema(operationConstructedType, relationship.RightType, true, false, schemaRepository);

        // This complicated implementation that generates a temporary schema stems from the fact that GetSchemaId takes a Type.
        // We could feed it a constructed type with TLeftResource and TRightResource, but there's no way to include
        // the relationship name because there's no runtime Type available for it.
        string schemaId = _schemaIdSelector.GetRelationshipAtomicOperationSchemaId(relationship, operationCode);

        OpenApiSchemaReference referenceSchemaForOperation =
            _defaultSchemaGenerator.GenerateSchema(operationConstructedType, schemaRepository).AsReferenceSchema();

        OpenApiSchema compositeInlineSchemaForOperation = schemaRepository.Schemas[referenceSchemaForOperation.GetReferenceId()].AsInlineSchema();
        compositeInlineSchemaForOperation.AdditionalPropertiesAllowed = false;

        OpenApiSchema inlineSchemaForOperation = compositeInlineSchemaForOperation.UnwrapLastExtendedSchema().AsInlineSchema();
        SetOperationCode(inlineSchemaForOperation, operationCode, schemaRepository);
        inlineSchemaForOperation.Properties ??= new Dictionary<string, IOpenApiSchema>();

        if (referenceSchemaForRelationshipIdentifier != null)
        {
            inlineSchemaForOperation.Properties[JsonApiPropertyName.Ref] = referenceSchemaForRelationshipIdentifier.WrapInExtendedSchema();
        }

        bool isNullable = _resourceFieldValidationMetadataProvider.IsNullable(relationship);
        ((OpenApiSchema)inlineSchemaForOperation.Properties[JsonApiPropertyName.Data]).SetNullable(isNullable);

        schemaRepository.ReplaceSchemaId(operationConstructedType, schemaId);
        referenceSchemaForOperation = new OpenApiSchemaReference(schemaId);

        if (relationshipInAnyBaseResourceType != null)
        {
            RemoveProperties(inlineSchemaForOperation);

            string baseRelationshipSchemaId = _schemaIdSelector.GetRelationshipAtomicOperationSchemaId(relationshipInAnyBaseResourceType, operationCode);
            ConsistencyGuard.ThrowIf(!schemaRepository.Schemas.ContainsKey(baseRelationshipSchemaId));

            compositeInlineSchemaForOperation.AllOf ??= new List<IOpenApiSchema>();
            compositeInlineSchemaForOperation.AllOf[0] = new OpenApiSchemaReference(baseRelationshipSchemaId);
        }

        MapInDiscriminator(referenceSchemaForOperation, schemaRepository);

        traceScope.TraceSucceeded(schemaId);
    }

    private static WriteOperationKind GetKindOfRelationshipOperation(AtomicOperationCode operationCode)
    {
        WriteOperationKind? writeOperation = null;

        if (operationCode == AtomicOperationCode.Add)
        {
            writeOperation = WriteOperationKind.AddToRelationship;
        }
        else if (operationCode == AtomicOperationCode.Update)
        {
            writeOperation = WriteOperationKind.SetRelationship;
        }
        else if (operationCode == AtomicOperationCode.Remove)
        {
            writeOperation = WriteOperationKind.RemoveFromRelationship;
        }

        ConsistencyGuard.ThrowIf(writeOperation == null);
        return writeOperation.Value;
    }

    private bool IsRelationshipEnabled(RelationshipAttribute relationship, WriteOperationKind writeOperation)
    {
        if (!_atomicOperationFilter.IsEnabled(relationship.LeftType, writeOperation))
        {
            return false;
        }

        if (relationship is HasOneAttribute hasOneRelationship && !IsToOneRelationshipEnabled(hasOneRelationship, writeOperation))
        {
            return false;
        }

        if (relationship is HasManyAttribute hasManyRelationship && !IsToManyRelationshipEnabled(hasManyRelationship, writeOperation))
        {
            return false;
        }

        return true;
    }

    private static bool IsToOneRelationshipEnabled(HasOneAttribute relationship, WriteOperationKind writeOperation)
    {
        bool? isEnabled = null;

        if (writeOperation == WriteOperationKind.SetRelationship)
        {
            isEnabled = relationship.Capabilities.HasFlag(HasOneCapabilities.AllowSet);
        }

        ConsistencyGuard.ThrowIf(isEnabled == null);
        return isEnabled.Value;
    }

    private static bool IsToManyRelationshipEnabled(HasManyAttribute relationship, WriteOperationKind writeOperation)
    {
        bool? isEnabled = null;

        if (writeOperation == WriteOperationKind.SetRelationship)
        {
            isEnabled = relationship.Capabilities.HasFlag(HasManyCapabilities.AllowSet);
        }
        else if (writeOperation == WriteOperationKind.AddToRelationship)
        {
            isEnabled = relationship.Capabilities.HasFlag(HasManyCapabilities.AllowAdd);
        }
        else if (writeOperation == WriteOperationKind.RemoveFromRelationship)
        {
            isEnabled = relationship.Capabilities.HasFlag(HasManyCapabilities.AllowRemove);
        }

        ConsistencyGuard.ThrowIf(isEnabled == null);
        return isEnabled.Value;
    }

    private RelationshipAttribute? GetRelationshipEnabledInAnyBase(RelationshipAttribute relationship, WriteOperationKind writeOperation)
    {
        RelationshipAttribute? relationshipInBaseResourceType = relationship.LeftType.BaseType?.FindRelationshipByPublicName(relationship.PublicName);

        while (relationshipInBaseResourceType != null)
        {
            if (IsRelationshipEnabled(relationshipInBaseResourceType, writeOperation))
            {
                return relationshipInBaseResourceType;
            }

            relationshipInBaseResourceType = relationshipInBaseResourceType.LeftType.BaseType?.FindRelationshipByPublicName(relationship.PublicName);
        }

        return null;
    }

    private void GenerateSchemasForResponseDocument(SchemaRepository schemaRepository)
    {
        _ = _dataContainerSchemaGenerator.GenerateSchemaForCommonResourceDataInResponse(schemaRepository);

        foreach (ResourceType resourceType in _resourceGraph.GetResourceTypes())
        {
            if (IsResourceTypeEnabled(resourceType, WriteOperationKind.CreateResource) ||
                IsResourceTypeEnabled(resourceType, WriteOperationKind.UpdateResource))
            {
                _dataContainerSchemaGenerator.GenerateSchema(typeof(AtomicResult), resourceType, false, false, schemaRepository);
            }
        }
    }
}
