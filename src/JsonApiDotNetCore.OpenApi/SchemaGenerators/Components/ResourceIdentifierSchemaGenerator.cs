using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SchemaGenerators.Components;

internal sealed class ResourceIdentifierSchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly GenerationCacheSchemaGenerator _generationCacheSchemaGenerator;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;

    public ResourceIdentifierSchemaGenerator(SchemaGenerator defaultSchemaGenerator, GenerationCacheSchemaGenerator generationCacheSchemaGenerator,
        ResourceTypeSchemaGenerator resourceTypeSchemaGenerator)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(generationCacheSchemaGenerator);
        ArgumentGuard.NotNull(resourceTypeSchemaGenerator);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _generationCacheSchemaGenerator = generationCacheSchemaGenerator;
        _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;
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

            OpenApiSchema referenceSchemaForResourceType = _resourceTypeSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
            fullSchemaForIdentifier.Properties[JsonApiPropertyName.Type] = referenceSchemaForResourceType.WrapInExtendedSchema();
        }

        return referenceSchemaForIdentifier;
    }
}
