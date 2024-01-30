using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class ResourceIdentifierSchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly ISchemaRepositoryAccessor _schemaRepositoryAccessor;

    public ResourceIdentifierSchemaGenerator(SchemaGenerator defaultSchemaGenerator, ResourceTypeSchemaGenerator resourceTypeSchemaGenerator,
        ISchemaRepositoryAccessor schemaRepositoryAccessor)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(resourceTypeSchemaGenerator);
        ArgumentGuard.NotNull(schemaRepositoryAccessor);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;
        _schemaRepositoryAccessor = schemaRepositoryAccessor;
    }

    public OpenApiSchema GenerateSchema(ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

        Type resourceIdentifierType = typeof(ResourceIdentifier<>).MakeGenericType(resourceType.ClrType);

        if (!_schemaRepositoryAccessor.Current.TryLookupByType(resourceIdentifierType, out OpenApiSchema? referenceSchemaForIdentifier))
        {
            referenceSchemaForIdentifier = _defaultSchemaGenerator.GenerateSchema(resourceIdentifierType, _schemaRepositoryAccessor.Current);
            OpenApiSchema fullSchemaForIdentifier = _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForIdentifier.Reference.Id];

            fullSchemaForIdentifier.Properties[JsonApiPropertyName.Type] = _resourceTypeSchemaGenerator.Get(resourceType);
        }

        return referenceSchemaForIdentifier;
    }
}
