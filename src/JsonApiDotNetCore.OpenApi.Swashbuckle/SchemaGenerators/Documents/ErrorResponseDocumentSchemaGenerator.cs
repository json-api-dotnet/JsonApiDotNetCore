using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Documents;

/// <summary>
/// Generates the OpenAPI component schema for an error document.
/// </summary>
internal sealed class ErrorResponseDocumentSchemaGenerator : DocumentSchemaGenerator
{
    private static readonly Type ErrorObjectType = typeof(ErrorObject);

    private readonly SchemaGenerationTracer _schemaGenerationTracer;
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly MetaSchemaGenerator _metaSchemaGenerator;

    public ErrorResponseDocumentSchemaGenerator(SchemaGenerationTracer schemaGenerationTracer, SchemaGenerator defaultSchemaGenerator,
        MetaSchemaGenerator metaSchemaGenerator, LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator, IJsonApiOptions options)
        : base(schemaGenerationTracer, metaSchemaGenerator, linksVisibilitySchemaGenerator, options)
    {
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);

        _schemaGenerationTracer = schemaGenerationTracer;
        _defaultSchemaGenerator = defaultSchemaGenerator;
        _metaSchemaGenerator = metaSchemaGenerator;
    }

    public override bool CanGenerate(Type schemaType)
    {
        return schemaType == typeof(ErrorResponseDocument);
    }

    protected override OpenApiSchemaReference GenerateDocumentSchema(Type schemaType, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        OpenApiSchemaReference referenceSchemaForErrorObject = GenerateSchemaForErrorObject(schemaRepository);
        var fullSchemaForErrorObject = (OpenApiSchema)schemaRepository.Schemas[referenceSchemaForErrorObject.Reference.Id!];

        OpenApiSchemaReference referenceSchemaForMeta = _metaSchemaGenerator.GenerateSchema(schemaRepository);
        fullSchemaForErrorObject.Properties ??= new Dictionary<string, IOpenApiSchema>();
        fullSchemaForErrorObject.Properties[JsonApiPropertyName.Meta] = referenceSchemaForMeta.WrapInExtendedSchema();

        return (OpenApiSchemaReference)_defaultSchemaGenerator.GenerateSchema(schemaType, schemaRepository);
    }

    private OpenApiSchemaReference GenerateSchemaForErrorObject(SchemaRepository schemaRepository)
    {
        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this, ErrorObjectType);

        var referenceSchema = (OpenApiSchemaReference)_defaultSchemaGenerator.GenerateSchema(ErrorObjectType, schemaRepository);

        traceScope.TraceSucceeded(referenceSchema.Reference.Id!);
        return referenceSchema;
    }
}
