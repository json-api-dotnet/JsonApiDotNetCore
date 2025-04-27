using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
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

    public OpenApiSchemaReference GenerateSchema(AtomicOperationCode operationCode, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaRepository);

        var schemaId = _schemaIdSelector.GetAtomicOperationCodeSchemaId(operationCode);

        if (schemaRepository.Schemas.ContainsKey(schemaId))
        {
            return new OpenApiSchemaReference(schemaId);
        }

        using var traceScope = _schemaGenerationTracer.TraceStart(this, operationCode);

        var enumValue = operationCode.ToString().ToLowerInvariant();

        var fullSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Enum = [enumValue]
        };

        var referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);

        traceScope.TraceSucceeded(schemaId);
        return referenceSchema;
    }
}
