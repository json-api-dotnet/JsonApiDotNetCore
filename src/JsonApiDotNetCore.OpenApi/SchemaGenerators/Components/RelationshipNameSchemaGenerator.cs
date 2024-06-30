using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SchemaGenerators.Components;

internal sealed class RelationshipNameSchemaGenerator
{
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;

    public RelationshipNameSchemaGenerator(JsonApiSchemaIdSelector schemaIdSelector)
    {
        ArgumentGuard.NotNull(schemaIdSelector);

        _schemaIdSelector = schemaIdSelector;
    }

    public OpenApiSchema GenerateSchema(RelationshipAttribute relationship, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(relationship);
        ArgumentGuard.NotNull(schemaRepository);

        string schemaId = _schemaIdSelector.GetRelationshipNameSchemaId(relationship);

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

        var fullSchema = new OpenApiSchema
        {
            Type = "string",
            Enum = [new OpenApiString(relationship.PublicName)],
            AdditionalPropertiesAllowed = false
        };

        return schemaRepository.AddDefinition(schemaId, fullSchema);
    }
}
