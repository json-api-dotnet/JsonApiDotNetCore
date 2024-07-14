using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class MetaSchemaGenerator
{
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;

    public MetaSchemaGenerator(JsonApiSchemaIdSelector schemaIdSelector)
    {
        ArgumentGuard.NotNull(schemaIdSelector);

        _schemaIdSelector = schemaIdSelector;
    }

    public OpenApiSchema GenerateSchema(SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(schemaRepository);

        if (schemaRepository.TryLookupByType(typeof(Meta), out OpenApiSchema? referenceSchema))
        {
            return referenceSchema;
        }

        var fullSchema = new OpenApiSchema
        {
            Type = "object",
            AdditionalProperties = new OpenApiSchema
            {
                Nullable = true
            }
        };

        string schemaId = _schemaIdSelector.GetMetaSchemaId();

        referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(typeof(Meta), schemaId);

        return referenceSchema;
    }
}
