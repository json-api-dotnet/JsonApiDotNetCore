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

    public OpenApiSchemaReference GenerateSchema(Type schemaType, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByTypeSafe(schemaType, out OpenApiSchemaReference? referenceSchema))
        {
            return referenceSchema;
        }

        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this, schemaType);

        _metaSchemaGenerator.GenerateSchema(schemaRepository);

        referenceSchema = GenerateDocumentSchema(schemaType, schemaRepository);
        OpenApiSchema inlineSchema = schemaRepository.Schemas[referenceSchema.GetReferenceId()].AsInlineSchema();

        _linksVisibilitySchemaGenerator.UpdateSchemaForTopLevel(schemaType, inlineSchema, schemaRepository);

        SetJsonApiVersion(inlineSchema, schemaRepository);

        traceScope.TraceSucceeded(referenceSchema.GetReferenceId());
        return referenceSchema;
    }

    protected abstract OpenApiSchemaReference GenerateDocumentSchema(Type schemaType, SchemaRepository schemaRepository);

    private void SetJsonApiVersion(OpenApiSchema inlineSchema, SchemaRepository schemaRepository)
    {
        if (!_options.IncludeJsonApiVersion && inlineSchema.Properties != null && inlineSchema.Properties.Remove(JsonApiPropertyName.Jsonapi))
        {
            schemaRepository.Schemas.Remove(JsonApiPropertyName.Jsonapi);
        }
    }
}
