using System.Diagnostics;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Bodies;

/// <summary>
/// Generates the OpenAPI component schema for an atomic:operations request/response body.
/// </summary>
internal sealed class AtomicOperationsBodySchemaGenerator : BodySchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly AtomicOperationCodeSchemaGenerator _atomicOperationCodeSchemaGenerator;
    private readonly ResourceIdentifierSchemaGenerator _resourceIdentifierSchemaGenerator;
    private readonly RelationshipIdentifierSchemaGenerator _relationshipIdentifierSchemaGenerator;
    private readonly AbstractAtomicOperationSchemaGenerator _abstractAtomicOperationSchemaGenerator;
    private readonly DataContainerSchemaGenerator _dataContainerSchemaGenerator;
    private readonly IAtomicOperationFilter _atomicOperationFilter;
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;
    private readonly IResourceGraph _resourceGraph;

    public AtomicOperationsBodySchemaGenerator(SchemaGenerator defaultSchemaGenerator, AtomicOperationCodeSchemaGenerator atomicOperationCodeSchemaGenerator,
        ResourceIdentifierSchemaGenerator resourceIdentifierSchemaGenerator, RelationshipIdentifierSchemaGenerator relationshipIdentifierSchemaGenerator,
        AbstractAtomicOperationSchemaGenerator abstractAtomicOperationSchemaGenerator, DataContainerSchemaGenerator dataContainerSchemaGenerator,
        MetaSchemaGenerator metaSchemaGenerator, LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator, IAtomicOperationFilter atomicOperationFilter,
        JsonApiSchemaIdSelector schemaIdSelector, ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider, IJsonApiOptions options,
        IResourceGraph resourceGraph)
        : base(metaSchemaGenerator, linksVisibilitySchemaGenerator, options)
    {
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);
        ArgumentNullException.ThrowIfNull(atomicOperationCodeSchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceIdentifierSchemaGenerator);
        ArgumentNullException.ThrowIfNull(relationshipIdentifierSchemaGenerator);
        ArgumentNullException.ThrowIfNull(abstractAtomicOperationSchemaGenerator);
        ArgumentNullException.ThrowIfNull(dataContainerSchemaGenerator);
        ArgumentNullException.ThrowIfNull(atomicOperationFilter);
        ArgumentNullException.ThrowIfNull(schemaIdSelector);
        ArgumentNullException.ThrowIfNull(resourceFieldValidationMetadataProvider);
        ArgumentNullException.ThrowIfNull(resourceGraph);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _atomicOperationCodeSchemaGenerator = atomicOperationCodeSchemaGenerator;
        _resourceIdentifierSchemaGenerator = resourceIdentifierSchemaGenerator;
        _relationshipIdentifierSchemaGenerator = relationshipIdentifierSchemaGenerator;
        _abstractAtomicOperationSchemaGenerator = abstractAtomicOperationSchemaGenerator;
        _dataContainerSchemaGenerator = dataContainerSchemaGenerator;
        _atomicOperationFilter = atomicOperationFilter;
        _schemaIdSelector = schemaIdSelector;
        _resourceFieldValidationMetadataProvider = resourceFieldValidationMetadataProvider;
        _resourceGraph = resourceGraph;
    }

    public override bool CanGenerate(Type modelType)
    {
        return modelType == typeof(OperationsRequestDocument) || modelType == typeof(OperationsResponseDocument);
    }

    protected override OpenApiSchema GenerateBodySchema(Type bodyType, SchemaRepository schemaRepository)
    {
        bool isRequestSchema = bodyType == typeof(OperationsRequestDocument);

        if (isRequestSchema)
        {
            GenerateSchemasForRequestBody(schemaRepository);
        }
        else
        {
            GenerateSchemasForResponseBody(schemaRepository);
        }

        return _defaultSchemaGenerator.GenerateSchema(bodyType, schemaRepository);
    }

    private void GenerateSchemasForRequestBody(SchemaRepository schemaRepository)
    {
        // There's no way to intercept in the Swashbuckle recursive component schema generation when using schema inheritance, which we need
        // to perform generic type expansions. As a workaround, we generate an empty base schema upfront. And each time the schema
        // for a derived type is generated, we'll add it to the discriminator mapping.
        _ = _abstractAtomicOperationSchemaGenerator.GenerateSchema(schemaRepository);

        foreach (ResourceType resourceType in _resourceGraph.GetResourceTypes())
        {
            GenerateSchemaForOperation(resourceType, schemaRepository);
        }
    }

    private void GenerateSchemaForOperation(ResourceType resourceType, SchemaRepository schemaRepository)
    {
        GenerateSchemaForResourceOperation(typeof(CreateResourceOperation<>), resourceType, AtomicOperationCode.Add, schemaRepository);
        GenerateSchemaForResourceOperation(typeof(UpdateResourceOperation<>), resourceType, AtomicOperationCode.Update, schemaRepository);
        GenerateSchemaForResourceOperation(typeof(DeleteResourceOperation<>), resourceType, AtomicOperationCode.Remove, schemaRepository);

        foreach (RelationshipAttribute relationship in resourceType.Relationships)
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
        WriteOperationKind writeOperation = operationCode switch
        {
            AtomicOperationCode.Add => WriteOperationKind.CreateResource,
            AtomicOperationCode.Update => WriteOperationKind.UpdateResource,
            AtomicOperationCode.Remove => WriteOperationKind.DeleteResource,
            _ => throw new UnreachableException()
        };

        if (!_atomicOperationFilter.IsEnabled(resourceType, writeOperation))
        {
            return;
        }

        _ = _resourceIdentifierSchemaGenerator.GenerateSchema(resourceType, true, schemaRepository);

        Type operationConstructedType = operationOpenType.MakeGenericType(resourceType.ClrType);
        bool hasDataProperty = operationOpenType != typeof(DeleteResourceOperation<>);

        if (hasDataProperty)
        {
            _ = _dataContainerSchemaGenerator.GenerateSchema(operationConstructedType, resourceType, true, schemaRepository);
        }

        OpenApiSchema referenceSchemaForOperation = _defaultSchemaGenerator.GenerateSchema(operationConstructedType, schemaRepository);
        OpenApiSchema fullSchemaForOperation = schemaRepository.Schemas[referenceSchemaForOperation.Reference.Id];
        fullSchemaForOperation.AdditionalPropertiesAllowed = false;

        OpenApiSchema fullSchemaForDerivedType = fullSchemaForOperation.UnwrapLastExtendedSchema();
        SetOperationCode(fullSchemaForDerivedType, operationCode, schemaRepository);

        string discriminatorValue = _schemaIdSelector.GetAtomicOperationDiscriminatorValue(operationCode, resourceType);
        _abstractAtomicOperationSchemaGenerator.MapDiscriminator(referenceSchemaForOperation, discriminatorValue, schemaRepository);
    }

    private void GenerateSchemaForRelationshipOperation(Type operationOpenType, RelationshipAttribute relationship, AtomicOperationCode operationCode,
        SchemaRepository schemaRepository)
    {
        WriteOperationKind writeOperation = operationCode switch
        {
            AtomicOperationCode.Add => WriteOperationKind.AddToRelationship,
            AtomicOperationCode.Update => WriteOperationKind.SetRelationship,
            AtomicOperationCode.Remove => WriteOperationKind.RemoveFromRelationship,
            _ => throw new UnreachableException()
        };

        if (!_atomicOperationFilter.IsEnabled(relationship.LeftType, writeOperation))
        {
            return;
        }

        if (relationship is HasOneAttribute hasOneRelationship && !IsToOneRelationshipEnabled(hasOneRelationship, writeOperation))
        {
            return;
        }

        if (relationship is HasManyAttribute hasManyRelationship && !IsToManyRelationshipEnabled(hasManyRelationship, writeOperation))
        {
            return;
        }

        _ = _resourceIdentifierSchemaGenerator.GenerateSchema(relationship.LeftType, true, schemaRepository);
        _ = _resourceIdentifierSchemaGenerator.GenerateSchema(relationship.RightType, true, schemaRepository);

        OpenApiSchema referenceSchemaForRelationshipIdentifier = _relationshipIdentifierSchemaGenerator.GenerateSchema(relationship, schemaRepository);

        Type operationConstructedType = operationOpenType.MakeGenericType(relationship.RightType.ClrType);
        _ = _dataContainerSchemaGenerator.GenerateSchema(operationConstructedType, relationship.RightType, true, schemaRepository);

        // This complicated implementation that generates a temporary schema stems from the fact that GetSchemaId takes a Type.
        // We could feed it a constructed type with TLeftResource and TRightResource, but there's no way to include
        // the relationship name because there's no runtime Type available for it.
        string schemaId = _schemaIdSelector.GetRelationshipAtomicOperationSchemaId(relationship, operationCode);

        OpenApiSchema referenceSchemaForOperation = _defaultSchemaGenerator.GenerateSchema(operationConstructedType, schemaRepository);
        OpenApiSchema fullSchemaForOperation = schemaRepository.Schemas[referenceSchemaForOperation.Reference.Id];
        fullSchemaForOperation.AdditionalPropertiesAllowed = false;

        OpenApiSchema fullSchemaForDerivedType = fullSchemaForOperation.UnwrapLastExtendedSchema();
        SetOperationCode(fullSchemaForDerivedType, operationCode, schemaRepository);

        fullSchemaForDerivedType.Properties[JsonApiPropertyName.Ref] = referenceSchemaForRelationshipIdentifier.WrapInExtendedSchema();
        fullSchemaForDerivedType.Properties[JsonApiPropertyName.Data].Nullable = _resourceFieldValidationMetadataProvider.IsNullable(relationship);

        schemaRepository.ReplaceSchemaId(operationConstructedType, schemaId);
        referenceSchemaForOperation.Reference.Id = schemaId;

        string discriminatorValue = _schemaIdSelector.GetAtomicOperationDiscriminatorValue(operationCode, relationship);
        _abstractAtomicOperationSchemaGenerator.MapDiscriminator(referenceSchemaForOperation, discriminatorValue, schemaRepository);
    }

    private static bool IsToOneRelationshipEnabled(HasOneAttribute relationship, WriteOperationKind writeOperation)
    {
        return writeOperation switch
        {
            WriteOperationKind.SetRelationship => relationship.Capabilities.HasFlag(HasOneCapabilities.AllowSet),
            _ => throw new UnreachableException()
        };
    }

    private static bool IsToManyRelationshipEnabled(HasManyAttribute relationship, WriteOperationKind writeOperation)
    {
        return writeOperation switch
        {
            WriteOperationKind.SetRelationship => relationship.Capabilities.HasFlag(HasManyCapabilities.AllowSet),
            WriteOperationKind.AddToRelationship => relationship.Capabilities.HasFlag(HasManyCapabilities.AllowAdd),
            WriteOperationKind.RemoveFromRelationship => relationship.Capabilities.HasFlag(HasManyCapabilities.AllowRemove),
            _ => throw new UnreachableException()
        };
    }

    private void SetOperationCode(OpenApiSchema fullSchema, AtomicOperationCode operationCode, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchema = _atomicOperationCodeSchemaGenerator.GenerateSchema(operationCode, schemaRepository);
        fullSchema.Properties[JsonApiPropertyName.Op] = referenceSchema.WrapInExtendedSchema();
    }

    private void GenerateSchemasForResponseBody(SchemaRepository schemaRepository)
    {
        foreach (ResourceType resourceType in _resourceGraph.GetResourceTypes())
        {
            _ = _dataContainerSchemaGenerator.GenerateSchema(typeof(AtomicResult), resourceType, false, schemaRepository);
        }
    }
}
