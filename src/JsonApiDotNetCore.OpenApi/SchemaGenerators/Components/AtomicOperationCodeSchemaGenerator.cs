using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SchemaGenerators.Components;

internal sealed class AtomicOperationCodeSchemaGenerator
{
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;

    public AtomicOperationCodeSchemaGenerator(JsonApiSchemaIdSelector schemaIdSelector)
    {
        ArgumentGuard.NotNull(schemaIdSelector);

        _schemaIdSelector = schemaIdSelector;
    }

    public OpenApiSchema GenerateSchema(AtomicOperationCode operationCode, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(schemaRepository);

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

        string enumValue = operationCode.ToString().ToLowerInvariant();

        var fullSchema = new OpenApiSchema
        {
            Type = "string",
            Enum = [new OpenApiString(enumValue)]
        };

        return schemaRepository.AddDefinition(schemaId, fullSchema);
    }
}
