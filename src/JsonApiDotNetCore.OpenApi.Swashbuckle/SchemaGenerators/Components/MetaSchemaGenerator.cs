using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
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

    public OpenApiSchemaReference GenerateSchema(SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByType(SchemaType, out var referenceSchema))
        {
            return referenceSchema;
        }

        using var traceScope = _schemaGenerationTracer.TraceStart(this, SchemaType);

        var fullSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            AdditionalProperties = new OpenApiSchema
            {
                Type = JsonSchemaType.Null
            }
        };

        var schemaId = _schemaIdSelector.GetMetaSchemaId();

        referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(SchemaType, schemaId);

        traceScope.TraceSucceeded(schemaId);
        return referenceSchema;
    }
}
