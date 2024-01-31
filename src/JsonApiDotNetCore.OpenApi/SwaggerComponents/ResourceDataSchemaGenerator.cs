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
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly ResourceIdentifierSchemaGenerator _resourceIdentifierSchemaGenerator;
    private readonly IResourceGraph _resourceGraph;
    private readonly IJsonApiOptions _options;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;
    private readonly ResourceDocumentationReader _resourceDocumentationReader;

    public ResourceDataSchemaGenerator(SchemaGenerator defaultSchemaGenerator, ResourceTypeSchemaGenerator resourceTypeSchemaGenerator,
        ResourceIdentifierSchemaGenerator resourceIdentifierSchemaGenerator, IResourceGraph resourceGraph, IJsonApiOptions options,
        ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider, ResourceDocumentationReader resourceDocumentationReader)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(resourceTypeSchemaGenerator);
        ArgumentGuard.NotNull(resourceIdentifierSchemaGenerator);
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(resourceFieldValidationMetadataProvider);
        ArgumentGuard.NotNull(resourceDocumentationReader);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;
        _resourceIdentifierSchemaGenerator = resourceIdentifierSchemaGenerator;
        _resourceGraph = resourceGraph;
        _options = options;
        _resourceFieldValidationMetadataProvider = resourceFieldValidationMetadataProvider;

        _resourceDocumentationReader = resourceDocumentationReader;
    }

    public OpenApiSchema GenerateSchema(Type resourceDataType, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(resourceDataType);
        ArgumentGuard.NotNull(schemaRepository);

        if (schemaRepository.TryLookupByType(resourceDataType, out OpenApiSchema referenceSchemaForResourceData))
        {
            return referenceSchemaForResourceData;
        }

        referenceSchemaForResourceData = _defaultSchemaGenerator.GenerateSchema(resourceDataType, schemaRepository);
        OpenApiSchema fullSchemaForResourceData = schemaRepository.Schemas[referenceSchemaForResourceData.Reference.Id];

        var resourceTypeInfo = ResourceTypeInfo.Create(resourceDataType, _resourceGraph);

        var fieldSchemaBuilder = new ResourceFieldSchemaBuilder(_defaultSchemaGenerator, _resourceIdentifierSchemaGenerator,
            _resourceFieldValidationMetadataProvider, resourceTypeInfo);

        RemoveResourceIdIfPostResource(resourceTypeInfo, fullSchemaForResourceData);
        SetResourceType(fullSchemaForResourceData, resourceTypeInfo.ResourceType, schemaRepository);

        fullSchemaForResourceData.Description = _resourceDocumentationReader.GetDocumentationForType(resourceTypeInfo.ResourceType);

        fullSchemaForResourceData.SetValuesInMetaToNullable();

        SetResourceAttributes(fullSchemaForResourceData, fieldSchemaBuilder, schemaRepository);
        SetResourceRelationships(fullSchemaForResourceData, fieldSchemaBuilder, schemaRepository);

        fullSchemaForResourceData.ReorderProperties(ResourceDataPropertyNamesInOrder);

        return referenceSchemaForResourceData;
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

    private void SetResourceType(OpenApiSchema fullSchemaForResourceData, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        fullSchemaForResourceData.Properties[JsonApiPropertyName.Type] = _resourceTypeSchemaGenerator.Get(resourceType, schemaRepository);
    }

    private void SetResourceAttributes(OpenApiSchema fullSchemaForResourceData, ResourceFieldSchemaBuilder builder, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchemaForAttributes = fullSchemaForResourceData.Properties[JsonApiPropertyName.Attributes].UnwrapExtendedReferenceSchema();
        OpenApiSchema fullSchemaForAttributes = schemaRepository.Schemas[referenceSchemaForAttributes.Reference.Id];

        builder.SetMembersOfAttributes(fullSchemaForAttributes, schemaRepository);

        if (!fullSchemaForAttributes.Properties.Any())
        {
            fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Attributes);
            schemaRepository.Schemas.Remove(referenceSchemaForAttributes.Reference.Id);
        }
        else
        {
            fullSchemaForAttributes.AdditionalPropertiesAllowed = false;
        }
    }

    private void SetResourceRelationships(OpenApiSchema fullSchemaForResourceData, ResourceFieldSchemaBuilder builder, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchemaForRelationships = fullSchemaForResourceData.Properties[JsonApiPropertyName.Relationships].UnwrapExtendedReferenceSchema();
        OpenApiSchema fullSchemaForRelationships = schemaRepository.Schemas[referenceSchemaForRelationships.Reference.Id];

        builder.SetMembersOfRelationships(fullSchemaForRelationships, schemaRepository);

        if (!fullSchemaForRelationships.Properties.Any())
        {
            fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Relationships);
            schemaRepository.Schemas.Remove(referenceSchemaForRelationships.Reference.Id);
        }
        else
        {
            fullSchemaForRelationships.AdditionalPropertiesAllowed = false;
        }
    }
}
