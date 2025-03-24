using System.Diagnostics;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Bodies;

/// <summary>
/// Generates the OpenAPI component schema for an atomic:operations request/response body.
/// </summary>
internal sealed class AtomicOperationsBodySchemaGenerator : BodySchemaGenerator
{
    private static readonly Type AtomicOperationAbstractType = typeof(AtomicOperation);

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

    public AtomicOperationsBodySchemaGenerator(SchemaGenerator defaultSchemaGenerator, AtomicOperationCodeSchemaGenerator atomicOperationCodeSchemaGenerator,
        DataSchemaGenerator dataSchemaGenerator, RelationshipIdentifierSchemaGenerator relationshipIdentifierSchemaGenerator,
        DataContainerSchemaGenerator dataContainerSchemaGenerator, MetaSchemaGenerator metaSchemaGenerator,
        LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator, IAtomicOperationFilter atomicOperationFilter, JsonApiSchemaIdSelector schemaIdSelector,
        ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider, IJsonApiOptions options, IResourceGraph resourceGraph)
        : base(metaSchemaGenerator, linksVisibilitySchemaGenerator, options)
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
        _ = GenerateSchemaForAbstractOperation(schemaRepository);

        foreach (ResourceType resourceType in _resourceGraph.GetResourceTypes().Where(resourceType => resourceType.BaseType == null))
        {
            GenerateSchemaForOperation(resourceType, schemaRepository);
        }
    }

    private OpenApiSchema GenerateSchemaForAbstractOperation(SchemaRepository schemaRepository)
    {
        if (schemaRepository.TryLookupByType(AtomicOperationAbstractType, out OpenApiSchema? referenceSchema))
        {
            return referenceSchema;
        }

        OpenApiSchema referenceSchemaForMeta = _metaSchemaGenerator.GenerateSchema(schemaRepository);

        var fullSchema = new OpenApiSchema
        {
            Type = "object",
            Required = new SortedSet<string>([OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName]),
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName] = new()
                {
                    Type = "string"
                },
                [referenceSchemaForMeta.Reference.Id] = referenceSchemaForMeta.WrapInExtendedSchema()
            },
            AdditionalPropertiesAllowed = false,
            Discriminator = new OpenApiDiscriminator
            {
                PropertyName = OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName,
                Mapping = new SortedDictionary<string, string>(StringComparer.Ordinal)
            },
            Extensions =
            {
                ["x-abstract"] = new OpenApiBoolean(true)
            }
        };

        string schemaId = _schemaIdSelector.GetSchemaId(AtomicOperationAbstractType);

        referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(AtomicOperationAbstractType, schemaId);

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
        WriteOperationKind writeOperation = operationCode switch
        {
            AtomicOperationCode.Add => WriteOperationKind.CreateResource,
            AtomicOperationCode.Update => WriteOperationKind.UpdateResource,
            AtomicOperationCode.Remove => WriteOperationKind.DeleteResource,
            _ => throw new UnreachableException()
        };

        if (IsResourceTypeEnabled(resourceType, writeOperation))
        {
            Type operationConstructedType = ChangeResourceTypeInSchemaType(operationOpenType, resourceType);
            bool needsEmptyDerivedSchema = resourceType.BaseType != null && _atomicOperationFilter.IsEnabled(resourceType.BaseType, writeOperation);

            if (!needsEmptyDerivedSchema)
            {
                Type identifierSchemaType = typeof(IdentifierInRequest<>).MakeGenericType(resourceType.ClrType);
                _ = _dataSchemaGenerator.GenerateSchema(identifierSchemaType, true, schemaRepository);

                bool hasDataProperty = operationOpenType != typeof(DeleteOperation<>);

                if (hasDataProperty)
                {
                    _ = _dataContainerSchemaGenerator.GenerateSchema(operationConstructedType, resourceType, true, false, schemaRepository);
                }
            }

            OpenApiSchema referenceSchemaForOperation = _defaultSchemaGenerator.GenerateSchema(operationConstructedType, schemaRepository);
            OpenApiSchema fullSchemaForOperation = schemaRepository.Schemas[referenceSchemaForOperation.Reference.Id];
            fullSchemaForOperation.AdditionalPropertiesAllowed = false;
            OpenApiSchema inlineSchemaForOperation = fullSchemaForOperation.UnwrapLastExtendedSchema();

            if (needsEmptyDerivedSchema)
            {
                Type baseOperationSchemaType = ChangeResourceTypeInSchemaType(operationOpenType, resourceType.BaseType!);
                OpenApiSchema referenceSchemaForBaseOperation = schemaRepository.LookupByType(baseOperationSchemaType);

                RemoveProperties(inlineSchemaForOperation);
                fullSchemaForOperation.AllOf[0] = referenceSchemaForBaseOperation;
            }
            else
            {
                SetOperationCode(inlineSchemaForOperation, operationCode, schemaRepository);
            }

            string discriminatorValue = _schemaIdSelector.GetAtomicOperationDiscriminatorValue(operationCode, resourceType);
            MapInDiscriminator(referenceSchemaForOperation, discriminatorValue, schemaRepository);
        }

        foreach (ResourceType derivedType in resourceType.DirectlyDerivedTypes)
        {
            GenerateSchemaForResourceOperation(operationOpenType, derivedType, operationCode, schemaRepository);
        }
    }

    private bool IsResourceTypeEnabled(ResourceType resourceType, WriteOperationKind writeOperation)
    {
        return _atomicOperationFilter.IsEnabled(resourceType, writeOperation);
    }

    private static Type ChangeResourceTypeInSchemaType(Type schemaOpenType, ResourceType resourceType)
    {
        return schemaOpenType.MakeGenericType(resourceType.ClrType);
    }

    private static void RemoveProperties(OpenApiSchema fullSchema)
    {
        foreach (string propertyName in fullSchema.Properties.Keys)
        {
            fullSchema.Properties.Remove(propertyName);
            fullSchema.Required.Remove(propertyName);
        }
    }

    private void SetOperationCode(OpenApiSchema fullSchema, AtomicOperationCode operationCode, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchema = _atomicOperationCodeSchemaGenerator.GenerateSchema(operationCode, schemaRepository);
        fullSchema.Properties[JsonApiPropertyName.Op] = referenceSchema.WrapInExtendedSchema();
    }

    private static void MapInDiscriminator(OpenApiSchema referenceSchemaForOperation, string discriminatorValue, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchemaForAbstractOperation = schemaRepository.LookupByType(AtomicOperationAbstractType);
        OpenApiSchema fullSchemaForAbstractOperation = schemaRepository.Schemas[referenceSchemaForAbstractOperation.Reference.Id];
        fullSchemaForAbstractOperation.Discriminator.Mapping.Add(discriminatorValue, referenceSchemaForOperation.Reference.ReferenceV3);
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
        WriteOperationKind writeOperation = operationCode switch
        {
            AtomicOperationCode.Add => WriteOperationKind.AddToRelationship,
            AtomicOperationCode.Update => WriteOperationKind.SetRelationship,
            AtomicOperationCode.Remove => WriteOperationKind.RemoveFromRelationship,
            _ => throw new UnreachableException()
        };

        if (!IsRelationshipEnabled(relationship, writeOperation))
        {
            return;
        }

        RelationshipAttribute? relationshipInAnyBaseResourceType = GetRelationshipEnabledInAnyBase(relationship, writeOperation);

        OpenApiSchema? referenceSchemaForRelationshipIdentifier;

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
        _ = _dataContainerSchemaGenerator.GenerateSchema(operationConstructedType, relationship.RightType, true, false, schemaRepository);

        // This complicated implementation that generates a temporary schema stems from the fact that GetSchemaId takes a Type.
        // We could feed it a constructed type with TLeftResource and TRightResource, but there's no way to include
        // the relationship name because there's no runtime Type available for it.
        string schemaId = _schemaIdSelector.GetRelationshipAtomicOperationSchemaId(relationship, operationCode);

        OpenApiSchema referenceSchemaForOperation = _defaultSchemaGenerator.GenerateSchema(operationConstructedType, schemaRepository);
        OpenApiSchema fullSchemaForOperation = schemaRepository.Schemas[referenceSchemaForOperation.Reference.Id];
        fullSchemaForOperation.AdditionalPropertiesAllowed = false;

        OpenApiSchema inlineSchemaForOperation = fullSchemaForOperation.UnwrapLastExtendedSchema();
        SetOperationCode(inlineSchemaForOperation, operationCode, schemaRepository);

        if (referenceSchemaForRelationshipIdentifier != null)
        {
            inlineSchemaForOperation.Properties[JsonApiPropertyName.Ref] = referenceSchemaForRelationshipIdentifier.WrapInExtendedSchema();
        }

        inlineSchemaForOperation.Properties[JsonApiPropertyName.Data].Nullable = _resourceFieldValidationMetadataProvider.IsNullable(relationship);

        schemaRepository.ReplaceSchemaId(operationConstructedType, schemaId);
        referenceSchemaForOperation.Reference.Id = schemaId;

        if (relationshipInAnyBaseResourceType != null)
        {
            RemoveProperties(inlineSchemaForOperation);

            string baseRelationshipSchemaId = _schemaIdSelector.GetRelationshipAtomicOperationSchemaId(relationshipInAnyBaseResourceType, operationCode);

            if (!schemaRepository.Schemas.ContainsKey(baseRelationshipSchemaId))
            {
                throw new UnreachableException();
            }

            fullSchemaForOperation.AllOf[0] = new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Id = baseRelationshipSchemaId,
                    Type = ReferenceType.Schema
                }
            };
        }

        string discriminatorValue = _schemaIdSelector.GetAtomicOperationDiscriminatorValue(operationCode, relationship);
        MapInDiscriminator(referenceSchemaForOperation, discriminatorValue, schemaRepository);
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

    private void GenerateSchemasForResponseBody(SchemaRepository schemaRepository)
    {
        _ = _dataContainerSchemaGenerator.GenerateSchemaForCommonResourceDataInResponse(schemaRepository);

        foreach (ResourceType resourceType in _resourceGraph.GetResourceTypes())
        {
            if (IsResourceTypeEnabled(resourceType, WriteOperationKind.CreateResource) ||
                IsResourceTypeEnabled(resourceType, WriteOperationKind.UpdateResource))
            {
                _ = _dataContainerSchemaGenerator.GenerateSchema(typeof(AtomicResult), resourceType, false, false, schemaRepository);
            }
        }
    }
}
