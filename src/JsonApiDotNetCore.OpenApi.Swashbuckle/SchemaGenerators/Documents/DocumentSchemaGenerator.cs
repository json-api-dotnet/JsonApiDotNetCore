using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Documents;

/// <summary>
/// Generates the OpenAPI component schema for a request and/or response document.
/// </summary>
internal abstract class DocumentSchemaGenerator
{
    private readonly SchemaGenerationTracer _schemaGenerationTracer;
    private readonly MetaSchemaGenerator _metaSchemaGenerator;
    private readonly LinksVisibilitySchemaGenerator _linksVisibilitySchemaGenerator;
    private readonly IJsonApiOptions _options;

    protected DocumentSchemaGenerator(SchemaGenerationTracer schemaGenerationTracer, MetaSchemaGenerator metaSchemaGenerator,
        LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator, IJsonApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(schemaGenerationTracer);
        ArgumentNullException.ThrowIfNull(metaSchemaGenerator);
        ArgumentNullException.ThrowIfNull(linksVisibilitySchemaGenerator);
        ArgumentNullException.ThrowIfNull(options);

        _schemaGenerationTracer = schemaGenerationTracer;
        _metaSchemaGenerator = metaSchemaGenerator;
        _linksVisibilitySchemaGenerator = linksVisibilitySchemaGenerator;
        _options = options;
    }

    public abstract bool CanGenerate(Type schemaType);

    public IOpenApiSchema GenerateSchema(Type schemaType, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByType(schemaType, out OpenApiSchemaReference? referenceSchema))
        {
            return referenceSchema;
        }

        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this, schemaType);

        _metaSchemaGenerator.GenerateSchema(schemaRepository);

        referenceSchema = GenerateDocumentSchema(schemaType, schemaRepository);
        var fullSchema = (OpenApiSchema)schemaRepository.Schemas[referenceSchema.Reference.Id!];

        _linksVisibilitySchemaGenerator.UpdateSchemaForTopLevel(schemaType, fullSchema, schemaRepository);

        SetJsonApiVersion(fullSchema, schemaRepository);

        traceScope.TraceSucceeded(referenceSchema.Reference.Id!);
        return referenceSchema;
    }

    protected abstract OpenApiSchemaReference GenerateDocumentSchema(Type schemaType, SchemaRepository schemaRepository);

    private void SetJsonApiVersion(OpenApiSchema fullSchema, SchemaRepository schemaRepository)
    {
        if (fullSchema.Properties != null && fullSchema.Properties.ContainsKey(JsonApiPropertyName.Jsonapi) && !_options.IncludeJsonApiVersion)
        {
            fullSchema.Properties.Remove(JsonApiPropertyName.Jsonapi);
            schemaRepository.Schemas.Remove(JsonApiPropertyName.Jsonapi);
        }
    }
}
