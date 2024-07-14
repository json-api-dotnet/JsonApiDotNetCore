using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class DataSchemaGenerator
{
#if NET6_0
    private static readonly string[] ResourceDataPropertyNamesInOrder =
    [
        JsonApiPropertyName.Type,
        JsonApiPropertyName.Id,
        JsonApiPropertyName.Lid,
        JsonApiPropertyName.Attributes,
        JsonApiPropertyName.Relationships,
        JsonApiPropertyName.Links,
        JsonApiPropertyName.Meta
    ];
#endif

    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly GenerationCacheSchemaGenerator _generationCacheSchemaGenerator;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly ResourceIdSchemaGenerator _resourceIdSchemaGenerator;
    private readonly ResourceIdentifierSchemaGenerator _resourceIdentifierSchemaGenerator;
    private readonly LinksVisibilitySchemaGenerator _linksVisibilitySchemaGenerator;
    private readonly IResourceGraph _resourceGraph;
    private readonly IJsonApiOptions _options;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;
    private readonly RelationshipTypeFactory _relationshipTypeFactory;
    private readonly ResourceDocumentationReader _resourceDocumentationReader;

    public DataSchemaGenerator(SchemaGenerator defaultSchemaGenerator, GenerationCacheSchemaGenerator generationCacheSchemaGenerator,
        ResourceTypeSchemaGenerator resourceTypeSchemaGenerator, ResourceIdSchemaGenerator resourceIdSchemaGenerator,
        ResourceIdentifierSchemaGenerator resourceIdentifierSchemaGenerator, LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator,
        IResourceGraph resourceGraph, IJsonApiOptions options, ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider,
        RelationshipTypeFactory relationshipTypeFactory, ResourceDocumentationReader resourceDocumentationReader)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(generationCacheSchemaGenerator);
        ArgumentGuard.NotNull(resourceTypeSchemaGenerator);
        ArgumentGuard.NotNull(resourceIdSchemaGenerator);
        ArgumentGuard.NotNull(resourceIdentifierSchemaGenerator);
        ArgumentGuard.NotNull(linksVisibilitySchemaGenerator);
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(resourceFieldValidationMetadataProvider);
        ArgumentGuard.NotNull(relationshipTypeFactory);
        ArgumentGuard.NotNull(resourceDocumentationReader);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _generationCacheSchemaGenerator = generationCacheSchemaGenerator;
        _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;
        _resourceIdSchemaGenerator = resourceIdSchemaGenerator;
        _resourceIdentifierSchemaGenerator = resourceIdentifierSchemaGenerator;
        _linksVisibilitySchemaGenerator = linksVisibilitySchemaGenerator;
        _resourceGraph = resourceGraph;
        _options = options;
        _resourceFieldValidationMetadataProvider = resourceFieldValidationMetadataProvider;
        _relationshipTypeFactory = relationshipTypeFactory;
        _resourceDocumentationReader = resourceDocumentationReader;
    }

    public OpenApiSchema GenerateSchema(Type resourceDataConstructedType, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(resourceDataConstructedType);
        ArgumentGuard.NotNull(schemaRepository);

        if (schemaRepository.TryLookupByType(resourceDataConstructedType, out OpenApiSchema referenceSchemaForResourceData))
        {
            return referenceSchemaForResourceData;
        }

        referenceSchemaForResourceData = _defaultSchemaGenerator.GenerateSchema(resourceDataConstructedType, schemaRepository);
        OpenApiSchema fullSchemaForResourceData = schemaRepository.Schemas[referenceSchemaForResourceData.Reference.Id];
        fullSchemaForResourceData.AdditionalPropertiesAllowed = false;

        var resourceTypeInfo = ResourceTypeInfo.Create(resourceDataConstructedType, _resourceGraph);

        OpenApiSchema fullSchemaForDerivedType = fullSchemaForResourceData.UnwrapLastExtendedSchema();
        bool isRequestSchema = fullSchemaForDerivedType == fullSchemaForResourceData;

        if (isRequestSchema)
        {
            bool hasAtomicOperationsEndpoint = _generationCacheSchemaGenerator.HasAtomicOperationsEndpoint(schemaRepository);
            AdaptResourceIdentity(resourceTypeInfo, fullSchemaForResourceData, hasAtomicOperationsEndpoint);
            SetResourceType(fullSchemaForResourceData, resourceTypeInfo.ResourceType, schemaRepository);
        }

        SetResourceId(fullSchemaForDerivedType, resourceTypeInfo.ResourceType, schemaRepository);

        fullSchemaForResourceData.Description = _resourceDocumentationReader.GetDocumentationForType(resourceTypeInfo.ResourceType);

        var fieldSchemaBuilder = new ResourceFieldSchemaBuilder(_defaultSchemaGenerator, _resourceIdentifierSchemaGenerator, _linksVisibilitySchemaGenerator,
            _resourceFieldValidationMetadataProvider, _relationshipTypeFactory, resourceTypeInfo);

        if (fullSchemaForDerivedType.Properties.ContainsKey(JsonApiPropertyName.Attributes))
        {
            SetResourceAttributes(fullSchemaForDerivedType, isRequestSchema, fieldSchemaBuilder, schemaRepository);
        }

        if (fullSchemaForDerivedType.Properties.ContainsKey(JsonApiPropertyName.Relationships))
        {
            SetResourceRelationships(fullSchemaForDerivedType, isRequestSchema, fieldSchemaBuilder, schemaRepository);
        }

        _linksVisibilitySchemaGenerator.UpdateSchemaForResource(resourceTypeInfo, fullSchemaForDerivedType, schemaRepository);

#if NET6_0
        fullSchemaForDerivedType.ReorderProperties(ResourceDataPropertyNamesInOrder);
#endif

        return referenceSchemaForResourceData;
    }

    private void AdaptResourceIdentity(ResourceTypeInfo resourceTypeInfo, OpenApiSchema fullSchemaForResourceData, bool hasAtomicOperationsEndpoint)
    {
        if (!hasAtomicOperationsEndpoint)
        {
            fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Lid);
        }

        if (resourceTypeInfo.ResourceDataOpenType == typeof(DataInCreateResourceRequest<>))
        {
            ClientIdGenerationMode clientIdGeneration = resourceTypeInfo.ResourceType.ClientIdGeneration ?? _options.ClientIdGeneration;

            if (hasAtomicOperationsEndpoint)
            {
                if (clientIdGeneration == ClientIdGenerationMode.Forbidden)
                {
                    fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Id);
                }
                else if (clientIdGeneration == ClientIdGenerationMode.Required)
                {
                    fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Lid);
                    fullSchemaForResourceData.Required.Add(JsonApiPropertyName.Id);
                }
            }
            else
            {
                if (clientIdGeneration == ClientIdGenerationMode.Forbidden)
                {
                    fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Id);
                }
                else if (clientIdGeneration == ClientIdGenerationMode.Required)
                {
                    fullSchemaForResourceData.Required.Add(JsonApiPropertyName.Id);
                }
            }
        }
        else
        {
            if (!hasAtomicOperationsEndpoint)
            {
                fullSchemaForResourceData.Required.Add(JsonApiPropertyName.Id);
            }
        }
    }

    private void SetResourceType(OpenApiSchema fullSchemaForResourceData, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchema = _resourceTypeSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
        fullSchemaForResourceData.Properties[JsonApiPropertyName.Type] = referenceSchema.WrapInExtendedSchema();
    }

    private void SetResourceId(OpenApiSchema fullSchemaForResourceData, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        if (fullSchemaForResourceData.Properties.ContainsKey(JsonApiPropertyName.Id))
        {
            OpenApiSchema idSchema = _resourceIdSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
            fullSchemaForResourceData.Properties[JsonApiPropertyName.Id] = idSchema;
        }
    }

    private void SetResourceAttributes(OpenApiSchema fullSchemaForResourceData, bool forRequestSchema, ResourceFieldSchemaBuilder builder,
        SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchemaForAttributes = fullSchemaForResourceData.Properties[JsonApiPropertyName.Attributes].UnwrapLastExtendedSchema();
        OpenApiSchema fullSchemaForAttributes = schemaRepository.Schemas[referenceSchemaForAttributes.Reference.Id];

        builder.SetMembersOfAttributes(fullSchemaForAttributes, forRequestSchema, schemaRepository);

        if (!fullSchemaForAttributes.Properties.Any())
        {
            fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Attributes);
        }
        else
        {
            fullSchemaForAttributes.AdditionalPropertiesAllowed = false;
        }
    }

    private void SetResourceRelationships(OpenApiSchema fullSchemaForResourceData, bool forRequestSchema, ResourceFieldSchemaBuilder builder,
        SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchemaForRelationships = fullSchemaForResourceData.Properties[JsonApiPropertyName.Relationships].UnwrapLastExtendedSchema();
        OpenApiSchema fullSchemaForRelationships = schemaRepository.Schemas[referenceSchemaForRelationships.Reference.Id];

        builder.SetMembersOfRelationships(fullSchemaForRelationships, forRequestSchema, schemaRepository);

        if (!fullSchemaForRelationships.Properties.Any())
        {
            fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Relationships);
        }
        else
        {
            fullSchemaForRelationships.AdditionalPropertiesAllowed = false;
        }
    }
}
