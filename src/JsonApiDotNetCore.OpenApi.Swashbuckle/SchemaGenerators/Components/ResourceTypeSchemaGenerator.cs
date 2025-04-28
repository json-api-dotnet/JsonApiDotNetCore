using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class ResourceTypeSchemaGenerator
{
    private readonly SchemaGenerationTracer _schemaGenerationTracer;
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;

    public ResourceTypeSchemaGenerator(SchemaGenerationTracer schemaGenerationTracer, JsonApiSchemaIdSelector schemaIdSelector)
    {
        ArgumentNullException.ThrowIfNull(schemaGenerationTracer);
        ArgumentNullException.ThrowIfNull(schemaIdSelector);

        _schemaGenerationTracer = schemaGenerationTracer;
        _schemaIdSelector = schemaIdSelector;
    }

    public OpenApiSchemaReference GenerateSchema(ResourceType resourceType, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByType(resourceType.ClrType, out var referenceSchema))
        {
            return referenceSchema;
        }

        using var traceScope = _schemaGenerationTracer.TraceStart(this, resourceType.ClrType);

        var fullSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Enum = resourceType.ClrType.IsAbstract ? [] : [resourceType.PublicName],
            Extensions = new Dictionary<string, IOpenApiExtension>()
            {
                [StringEnumOrderingFilter.RequiresSortKey] = new OpenApiAny(true)
            }
        };

        foreach (var derivedType in resourceType.GetAllConcreteDerivedTypes())
        {
            fullSchema.Enum.Add(derivedType.PublicName);
        }

        var schemaId = _schemaIdSelector.GetResourceTypeSchemaId(resourceType);

        referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(resourceType.ClrType, schemaId);

        traceScope.TraceSucceeded(schemaId);
        return referenceSchema;
    }

    public OpenApiSchemaReference GenerateSchema(SchemaRepository schemaRepository)
    {
        var schemaId = _schemaIdSelector.GetResourceTypeSchemaId(null);

        if (schemaRepository.Schemas.ContainsKey(schemaId))
        {
            return new OpenApiSchemaReference(schemaId);
        }

        using var traceScope = _schemaGenerationTracer.TraceStart(this);

        var fullSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Extensions = new Dictionary<string, IOpenApiExtension>()
            {
                [StringEnumOrderingFilter.RequiresSortKey] = new OpenApiAny(true)
            }
        };

        var referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);

        traceScope.TraceSucceeded(schemaId);
        return referenceSchema;
    }
}
