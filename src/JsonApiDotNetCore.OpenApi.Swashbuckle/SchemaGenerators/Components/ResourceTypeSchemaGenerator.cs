using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class ResourceTypeSchemaGenerator
{
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;

    public ResourceTypeSchemaGenerator(JsonApiSchemaIdSelector schemaIdSelector)
    {
        ArgumentNullException.ThrowIfNull(schemaIdSelector);

        _schemaIdSelector = schemaIdSelector;
    }

    public OpenApiSchema GenerateSchema(ResourceType resourceType, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByType(resourceType.ClrType, out OpenApiSchema? referenceSchema))
        {
            return referenceSchema;
        }

        var fullSchema = new OpenApiSchema
        {
            Type = "string",
            Enum = resourceType.ClrType.IsAbstract ? [] : [new OpenApiString(resourceType.PublicName)],
            Extensions =
            {
                [StringEnumOrderingFilter.RequiresSortKey] = new OpenApiBoolean(true)
            }
        };

        foreach (ResourceType derivedType in resourceType.GetAllConcreteDerivedTypes())
        {
            fullSchema.Enum.Add(new OpenApiString(derivedType.PublicName));
        }

        string schemaId = _schemaIdSelector.GetResourceTypeSchemaId(resourceType);

        referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(resourceType.ClrType, schemaId);

        return referenceSchema;
    }

    public OpenApiSchema GenerateSchema(SchemaRepository schemaRepository)
    {
        string schemaId = _schemaIdSelector.GetResourceTypeSchemaId(null);

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
            Extensions =
            {
                [StringEnumOrderingFilter.RequiresSortKey] = new OpenApiBoolean(true)
            }
        };

        return schemaRepository.AddDefinition(schemaId, fullSchema);
    }
}
