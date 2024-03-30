using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using SchemaGenerator = Swashbuckle.AspNetCore.SwaggerGen.Patched.SchemaGenerator;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class ResourceDataSchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly ResourceIdentifierSchemaGenerator _resourceIdentifierSchemaGenerator;
    private readonly LinksVisibilitySchemaGenerator _linksVisibilitySchemaGenerator;
    private readonly IResourceGraph _resourceGraph;
    private readonly IJsonApiOptions _options;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;
    private readonly RelationshipTypeFactory _relationshipTypeFactory;
    private readonly ResourceDocumentationReader _resourceDocumentationReader;

    public ResourceDataSchemaGenerator(SchemaGenerator defaultSchemaGenerator, ResourceTypeSchemaGenerator resourceTypeSchemaGenerator,
        ResourceIdentifierSchemaGenerator resourceIdentifierSchemaGenerator, LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator,
        IResourceGraph resourceGraph, IJsonApiOptions options, ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider,
        RelationshipTypeFactory relationshipTypeFactory, ResourceDocumentationReader resourceDocumentationReader)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(resourceTypeSchemaGenerator);
        ArgumentGuard.NotNull(resourceIdentifierSchemaGenerator);
        ArgumentGuard.NotNull(linksVisibilitySchemaGenerator);
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(resourceFieldValidationMetadataProvider);
        ArgumentGuard.NotNull(relationshipTypeFactory);
        ArgumentGuard.NotNull(resourceDocumentationReader);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;
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

        var fieldSchemaBuilder = new ResourceFieldSchemaBuilder(_defaultSchemaGenerator, _resourceIdentifierSchemaGenerator, _linksVisibilitySchemaGenerator,
            _resourceFieldValidationMetadataProvider, _relationshipTypeFactory, resourceTypeInfo);

        OpenApiSchema effectiveFullSchemaForResourceData =
            fullSchemaForResourceData.AllOf.Count == 0 ? fullSchemaForResourceData : fullSchemaForResourceData.AllOf[1];

        if (effectiveFullSchemaForResourceData == fullSchemaForResourceData)
        {
            RemoveResourceIdIfPostResource(resourceTypeInfo, fullSchemaForResourceData);
            SetResourceType(fullSchemaForResourceData, resourceTypeInfo.ResourceType, schemaRepository);
        }

        fullSchemaForResourceData.Description = _resourceDocumentationReader.GetDocumentationForType(resourceTypeInfo.ResourceType);

        SetResourceAttributes(effectiveFullSchemaForResourceData, fieldSchemaBuilder, schemaRepository);
        SetResourceRelationships(effectiveFullSchemaForResourceData, fieldSchemaBuilder, schemaRepository);

        _linksVisibilitySchemaGenerator.UpdateSchemaForResource(resourceTypeInfo, effectiveFullSchemaForResourceData, schemaRepository);

        effectiveFullSchemaForResourceData.SetValuesInMetaToNullable();

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
        }
        else
        {
            fullSchemaForRelationships.AdditionalPropertiesAllowed = false;
        }
    }
}
