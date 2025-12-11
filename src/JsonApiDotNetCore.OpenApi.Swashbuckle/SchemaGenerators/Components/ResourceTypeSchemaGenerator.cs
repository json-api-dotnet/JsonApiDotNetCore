using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using Microsoft.OpenApi;
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

        if (schemaRepository.TryLookupByTypeSafe(resourceType.ClrType, out OpenApiSchemaReference? referenceSchema))
        {
            return referenceSchema;
        }

        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this, resourceType.ClrType);

        var inlineSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Enum = resourceType.ClrType.IsAbstract ? [] : [resourceType.PublicName],
            Extensions = new SortedDictionary<string, IOpenApiExtension>(StringComparer.Ordinal)
            {
                [StringEnumOrderingFilter.RequiresSortKey] = new JsonNodeExtension(true)
            }
        };

        foreach (ResourceType derivedType in resourceType.GetAllConcreteDerivedTypes())
        {
            inlineSchema.Enum.Add(derivedType.PublicName);
        }

        string schemaId = _schemaIdSelector.GetResourceTypeSchemaId(resourceType);

        referenceSchema = schemaRepository.AddDefinition(schemaId, inlineSchema);
        schemaRepository.RegisterType(resourceType.ClrType, schemaId);

        traceScope.TraceSucceeded(schemaId);
        return referenceSchema;
    }

    public OpenApiSchemaReference GenerateSchema(SchemaRepository schemaRepository)
    {
        string schemaId = _schemaIdSelector.GetResourceTypeSchemaId(null);

        if (schemaRepository.Schemas.ContainsKey(schemaId))
        {
            return new OpenApiSchemaReference(schemaId);
        }

        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this);

        var inlineSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Extensions = new SortedDictionary<string, IOpenApiExtension>(StringComparer.Ordinal)
            {
                [StringEnumOrderingFilter.RequiresSortKey] = new JsonNodeExtension(true)
            }
        };

        OpenApiSchemaReference referenceSchema = schemaRepository.AddDefinition(schemaId, inlineSchema);

        traceScope.TraceSucceeded(schemaId);
        return referenceSchema;
    }
}
