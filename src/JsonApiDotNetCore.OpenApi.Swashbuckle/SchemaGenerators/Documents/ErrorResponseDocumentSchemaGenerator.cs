using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Documents;

/// <summary>
/// Generates the OpenAPI component schema for an error document.
/// </summary>
internal sealed class ErrorResponseDocumentSchemaGenerator : DocumentSchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly MetaSchemaGenerator _metaSchemaGenerator;

    public ErrorResponseDocumentSchemaGenerator(SchemaGenerator defaultSchemaGenerator, MetaSchemaGenerator metaSchemaGenerator,
        LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator, IJsonApiOptions options)
        : base(metaSchemaGenerator, linksVisibilitySchemaGenerator, options)
    {
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _metaSchemaGenerator = metaSchemaGenerator;
    }

    public override bool CanGenerate(Type schemaType)
    {
        return schemaType == typeof(ErrorResponseDocument);
    }

    protected override OpenApiSchema GenerateDocumentSchema(Type schemaType, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchemaForErrorObject = _defaultSchemaGenerator.GenerateSchema(typeof(ErrorObject), schemaRepository);
        OpenApiSchema fullSchemaForErrorObject = schemaRepository.Schemas[referenceSchemaForErrorObject.Reference.Id];

        OpenApiSchema referenceSchemaForMeta = _metaSchemaGenerator.GenerateSchema(schemaRepository);
        fullSchemaForErrorObject.Properties[JsonApiPropertyName.Meta] = referenceSchemaForMeta.WrapInExtendedSchema();

        return _defaultSchemaGenerator.GenerateSchema(schemaType, schemaRepository);
    }
}
