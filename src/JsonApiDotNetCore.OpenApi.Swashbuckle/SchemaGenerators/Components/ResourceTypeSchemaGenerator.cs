using JsonApiDotNetCore.Configuration;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class ResourceTypeSchemaGenerator
{
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;

    public ResourceTypeSchemaGenerator(JsonApiSchemaIdSelector schemaIdSelector)
    {
        ArgumentGuard.NotNull(schemaIdSelector);

        _schemaIdSelector = schemaIdSelector;
    }

    public OpenApiSchema GenerateSchema(ResourceType resourceType, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(resourceType);
        ArgumentGuard.NotNull(schemaRepository);

        if (schemaRepository.TryLookupByType(resourceType.ClrType, out OpenApiSchema? referenceSchema))
        {
            return referenceSchema;
        }

        var fullSchema = new OpenApiSchema
        {
            Type = "string",
            Enum = [new OpenApiString(resourceType.PublicName)],
            AdditionalPropertiesAllowed = false
        };

        string schemaId = _schemaIdSelector.GetResourceTypeSchemaId(resourceType);

        referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(resourceType.ClrType, schemaId);

        return referenceSchema;
    }
}
