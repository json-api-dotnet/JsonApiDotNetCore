using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
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

    public OpenApiSchema GenerateSchema(AtomicOperationCode operationCode, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaRepository);

        string schemaId = _schemaIdSelector.GetAtomicOperationCodeSchemaId(operationCode);

        if (schemaRepository.Schemas.ContainsKey(schemaId))
        {
            return new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Id = schemaId,
                    Type = ReferenceType.Schema
                }
            };
        }

        using IDisposable traceScope = _schemaGenerationTracer.TraceStart(this, schemaId);

        string enumValue = operationCode.ToString().ToLowerInvariant();

        var fullSchema = new OpenApiSchema
        {
            Type = "string",
            Enum = [new OpenApiString(enumValue)]
        };

        return schemaRepository.AddDefinition(schemaId, fullSchema);
    }
}
