using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class MetaSchemaGenerator
{
    private static readonly Type SchemaType = typeof(Meta);
    private readonly SchemaGenerationTracer _schemaGenerationTracer;
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;

    public MetaSchemaGenerator(SchemaGenerationTracer schemaGenerationTracer, JsonApiSchemaIdSelector schemaIdSelector)
    {
        ArgumentNullException.ThrowIfNull(schemaGenerationTracer);
        ArgumentNullException.ThrowIfNull(schemaIdSelector);

        _schemaGenerationTracer = schemaGenerationTracer;
        _schemaIdSelector = schemaIdSelector;
    }

    public OpenApiSchema GenerateSchema(SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByType(SchemaType, out OpenApiSchema? referenceSchema))
        {
            return referenceSchema;
        }

        using IDisposable traceScope = _schemaGenerationTracer.TraceStart(this, SchemaType);

        string schemaId = _schemaIdSelector.GetMetaSchemaId();

        var fullSchema = new OpenApiSchema
        {
            Type = "object",
            AdditionalProperties = new OpenApiSchema
            {
                Nullable = true
            }
        };

        referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(SchemaType, schemaId);

        return referenceSchema;
    }
}
