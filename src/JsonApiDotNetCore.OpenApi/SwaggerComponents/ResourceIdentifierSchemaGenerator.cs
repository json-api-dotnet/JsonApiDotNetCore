using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class ResourceIdentifierSchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;

    public ResourceIdentifierSchemaGenerator(SchemaGenerator defaultSchemaGenerator, ResourceTypeSchemaGenerator resourceTypeSchemaGenerator)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(resourceTypeSchemaGenerator);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;
    }

    public OpenApiSchema GenerateSchema(ResourceType resourceType, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(resourceType);
        ArgumentGuard.NotNull(schemaRepository);

        Type resourceIdentifierConstructedType = typeof(ResourceIdentifier<>).MakeGenericType(resourceType.ClrType);

        if (!schemaRepository.TryLookupByType(resourceIdentifierConstructedType, out OpenApiSchema? referenceSchemaForIdentifier))
        {
            referenceSchemaForIdentifier = _defaultSchemaGenerator.GenerateSchema(resourceIdentifierConstructedType, schemaRepository);
            OpenApiSchema fullSchemaForIdentifier = schemaRepository.Schemas[referenceSchemaForIdentifier.Reference.Id];

            fullSchemaForIdentifier.Properties[JsonApiPropertyName.Type] = _resourceTypeSchemaGenerator.Get(resourceType, schemaRepository);
        }

        return referenceSchemaForIdentifier;
    }
}
