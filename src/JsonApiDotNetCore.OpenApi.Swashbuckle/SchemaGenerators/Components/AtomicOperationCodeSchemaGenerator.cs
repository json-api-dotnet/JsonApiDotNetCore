using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class AtomicOperationCodeSchemaGenerator
{
    private readonly SchemaGenerationTracer _schemaGenerationTracer;
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;

    public AtomicOperationCodeSchemaGenerator(SchemaGenerationTracer schemaGenerationTracer, JsonApiSchemaIdSelector schemaIdSelector)
    {
        ArgumentNullException.ThrowIfNull(schemaGenerationTracer);
        ArgumentNullException.ThrowIfNull(schemaIdSelector);

        _schemaGenerationTracer = schemaGenerationTracer;
        _schemaIdSelector = schemaIdSelector;
    }

    public IOpenApiSchema GenerateSchema(AtomicOperationCode operationCode, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaRepository);

        string schemaId = _schemaIdSelector.GetAtomicOperationCodeSchemaId(operationCode);

        if (schemaRepository.Schemas.ContainsKey(schemaId))
        {
            return new OpenApiSchemaReference(schemaId);
        }

        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this, operationCode);

        string enumValue = operationCode.ToString().ToLowerInvariant();

        var fullSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Enum = [enumValue]
        };

        OpenApiSchemaReference? referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);

        traceScope.TraceSucceeded(schemaId);
        return referenceSchema;
    }
}
