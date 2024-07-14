using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class ResourceIdentifierSchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly GenerationCacheSchemaGenerator _generationCacheSchemaGenerator;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly ResourceIdSchemaGenerator _resourceIdSchemaGenerator;

    public ResourceIdentifierSchemaGenerator(SchemaGenerator defaultSchemaGenerator, GenerationCacheSchemaGenerator generationCacheSchemaGenerator,
        ResourceTypeSchemaGenerator resourceTypeSchemaGenerator, ResourceIdSchemaGenerator resourceIdSchemaGenerator)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(generationCacheSchemaGenerator);
        ArgumentGuard.NotNull(resourceTypeSchemaGenerator);
        ArgumentGuard.NotNull(resourceIdSchemaGenerator);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _generationCacheSchemaGenerator = generationCacheSchemaGenerator;
        _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;
        _resourceIdSchemaGenerator = resourceIdSchemaGenerator;
    }

    public OpenApiSchema GenerateSchema(ResourceType resourceType, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(resourceType);
        ArgumentGuard.NotNull(schemaRepository);

        Type resourceIdentifierOpenType = forRequestSchema ? typeof(ResourceIdentifierInRequest<>) : typeof(ResourceIdentifierInResponse<>);
        Type resourceIdentifierConstructedType = resourceIdentifierOpenType.MakeGenericType(resourceType.ClrType);

        if (!schemaRepository.TryLookupByType(resourceIdentifierConstructedType, out OpenApiSchema? referenceSchemaForIdentifier))
        {
            referenceSchemaForIdentifier = _defaultSchemaGenerator.GenerateSchema(resourceIdentifierConstructedType, schemaRepository);
            OpenApiSchema fullSchemaForIdentifier = schemaRepository.Schemas[referenceSchemaForIdentifier.Reference.Id];

            if (forRequestSchema && !_generationCacheSchemaGenerator.HasAtomicOperationsEndpoint(schemaRepository))
            {
                fullSchemaForIdentifier.Properties.Remove(JsonApiPropertyName.Lid);
                fullSchemaForIdentifier.Required.Add(JsonApiPropertyName.Id);
            }

            SetResourceType(fullSchemaForIdentifier, resourceType, schemaRepository);
            SetResourceId(fullSchemaForIdentifier, resourceType, schemaRepository);
        }

        return referenceSchemaForIdentifier;
    }

    private void SetResourceType(OpenApiSchema fullSchemaForIdentifier, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchema = _resourceTypeSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
        fullSchemaForIdentifier.Properties[JsonApiPropertyName.Type] = referenceSchema.WrapInExtendedSchema();
    }

    private void SetResourceId(OpenApiSchema fullSchemaForResourceData, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        OpenApiSchema idSchema = _resourceIdSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
        fullSchemaForResourceData.Properties[JsonApiPropertyName.Id] = idSchema;
    }
}
