using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class ResourceIdentifierObjectSchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly ISchemaRepositoryAccessor _schemaRepositoryAccessor;

    public ResourceIdentifierObjectSchemaGenerator(SchemaGenerator defaultSchemaGenerator, ResourceTypeSchemaGenerator resourceTypeSchemaGenerator,
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

        Type resourceIdentifierObjectType = typeof(ResourceIdentifierObject<>).MakeGenericType(resourceType.ClrType);

        if (!_schemaRepositoryAccessor.Current.TryLookupByType(resourceIdentifierObjectType, out OpenApiSchema? referenceSchemaForResourceIdentifierObject))
        {
            referenceSchemaForResourceIdentifierObject =
                _defaultSchemaGenerator.GenerateSchema(resourceIdentifierObjectType, _schemaRepositoryAccessor.Current);

            OpenApiSchema fullSchemaForResourceIdentifierObject =
                _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForResourceIdentifierObject.Reference.Id];

            fullSchemaForResourceIdentifierObject.Properties[JsonApiPropertyName.Type] = _resourceTypeSchemaGenerator.Get(resourceType);
        }

        return referenceSchemaForResourceIdentifierObject;
    }
}
