using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class ResourceDataSchemaGenerator
{
    private static readonly string[] ResourceDataPropertyNamesInOrder =
    [
        JsonApiPropertyName.Type,
        JsonApiPropertyName.Id,
        JsonApiPropertyName.Attributes,
        JsonApiPropertyName.Relationships,
        JsonApiPropertyName.Links,
        JsonApiPropertyName.Meta
    ];

    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly ISchemaRepositoryAccessor _schemaRepositoryAccessor;
    private readonly IResourceGraph _resourceGraph;
    private readonly IJsonApiOptions _options;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly Func<ResourceTypeInfo, ResourceFieldSchemaBuilder> _resourceFieldSchemaBuilderFactory;
    private readonly ResourceDocumentationReader _resourceDocumentationReader;

    public ResourceDataSchemaGenerator(SchemaGenerator defaultSchemaGenerator, ISchemaRepositoryAccessor schemaRepositoryAccessor, IResourceGraph resourceGraph,
        IJsonApiOptions options, ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(schemaRepositoryAccessor);
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(resourceFieldValidationMetadataProvider);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _schemaRepositoryAccessor = schemaRepositoryAccessor;
        _resourceGraph = resourceGraph;
        _options = options;
        _resourceTypeSchemaGenerator = new ResourceTypeSchemaGenerator(schemaRepositoryAccessor, options.SerializerOptions.PropertyNamingPolicy);

        var resourceIdentifierSchemaGenerator =
            new ResourceIdentifierSchemaGenerator(defaultSchemaGenerator, _resourceTypeSchemaGenerator, schemaRepositoryAccessor);

        _resourceFieldSchemaBuilderFactory = resourceTypeInfo => new ResourceFieldSchemaBuilder(defaultSchemaGenerator, resourceIdentifierSchemaGenerator,
            schemaRepositoryAccessor, resourceTypeInfo, resourceFieldValidationMetadataProvider);

        _resourceDocumentationReader = new ResourceDocumentationReader();
    }

    public OpenApiSchema GenerateSchema(Type resourceDataType)
    {
        ArgumentGuard.NotNull(resourceDataType);

        (OpenApiSchema fullSchemaForResourceData, OpenApiSchema referenceSchemaForResourceData) = EnsureSchemasExist(resourceDataType);

        var resourceTypeInfo = ResourceTypeInfo.Create(resourceDataType, _resourceGraph);
        ResourceFieldSchemaBuilder fieldSchemaBuilder = _resourceFieldSchemaBuilderFactory(resourceTypeInfo);

        RemoveResourceIdIfPostResource(resourceTypeInfo, fullSchemaForResourceData);

        SetResourceType(fullSchemaForResourceData, resourceTypeInfo.ResourceType);
        fullSchemaForResourceData.SetValuesInMetaToNullable();

        SetResourceAttributes(fullSchemaForResourceData, fieldSchemaBuilder);
        SetResourceRelationships(fullSchemaForResourceData, fieldSchemaBuilder);

        fullSchemaForResourceData.ReorderProperties(ResourceDataPropertyNamesInOrder);

        return referenceSchemaForResourceData;
    }

    private (OpenApiSchema fullSchema, OpenApiSchema referenceSchema) EnsureSchemasExist(Type resourceDataType)
    {
        if (!_schemaRepositoryAccessor.Current.TryLookupByType(resourceDataType, out OpenApiSchema referenceSchema))
        {
            referenceSchema = _defaultSchemaGenerator.GenerateSchema(resourceDataType, _schemaRepositoryAccessor.Current);
        }

        OpenApiSchema fullSchema = _schemaRepositoryAccessor.Current.Schemas[referenceSchema.Reference.Id];

        return (fullSchema, referenceSchema);
    }

    private void RemoveResourceIdIfPostResource(ResourceTypeInfo resourceTypeInfo, OpenApiSchema fullSchemaForResourceData)
    {
        if (resourceTypeInfo.ResourceDataOpenType == typeof(ResourceDataInPostRequest<>))
        {
            ClientIdGenerationMode clientIdGeneration = resourceTypeInfo.ResourceType.ClientIdGeneration ?? _options.ClientIdGeneration;

            if (clientIdGeneration == ClientIdGenerationMode.Forbidden)
            {
                fullSchemaForResourceData.Required.Remove(JsonApiPropertyName.Id);
                fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Id);
            }
            else if (clientIdGeneration == ClientIdGenerationMode.Allowed)
            {
                fullSchemaForResourceData.Required.Remove(JsonApiPropertyName.Id);
            }
        }
    }

    private void SetResourceType(OpenApiSchema fullSchemaForResourceData, ResourceType resourceType)
    {
        fullSchemaForResourceData.Properties[JsonApiPropertyName.Type] = _resourceTypeSchemaGenerator.Get(resourceType);

        fullSchemaForResourceData.Description = _resourceDocumentationReader.GetDocumentationForType(resourceType);
    }

    private void SetResourceAttributes(OpenApiSchema fullSchemaForResourceData, ResourceFieldSchemaBuilder builder)
    {
        OpenApiSchema referenceSchemaForAttributes = fullSchemaForResourceData.Properties[JsonApiPropertyName.Attributes].UnwrapExtendedReferenceSchema();
        OpenApiSchema fullSchemaForAttributes = _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForAttributes.Reference.Id];

        builder.SetMembersOfAttributes(fullSchemaForAttributes);

        if (!fullSchemaForAttributes.Properties.Any())
        {
            fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Attributes);
            _schemaRepositoryAccessor.Current.Schemas.Remove(referenceSchemaForAttributes.Reference.Id);
        }
        else
        {
            fullSchemaForAttributes.AdditionalPropertiesAllowed = false;
        }
    }

    private void SetResourceRelationships(OpenApiSchema fullSchemaForResourceData, ResourceFieldSchemaBuilder builder)
    {
        OpenApiSchema referenceSchemaForRelationships = fullSchemaForResourceData.Properties[JsonApiPropertyName.Relationships].UnwrapExtendedReferenceSchema();
        OpenApiSchema fullSchemaForRelationships = _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForRelationships.Reference.Id];

        builder.SetMembersOfRelationships(fullSchemaForRelationships);

        if (!fullSchemaForRelationships.Properties.Any())
        {
            fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Relationships);
            _schemaRepositoryAccessor.Current.Schemas.Remove(referenceSchemaForRelationships.Reference.Id);
        }
        else
        {
            fullSchemaForRelationships.AdditionalPropertiesAllowed = false;
        }
    }
}
