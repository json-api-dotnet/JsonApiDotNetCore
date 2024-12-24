using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Bodies;

/// <summary>
/// Generates the OpenAPI component schema for an error document.
/// </summary>
internal sealed class ErrorResponseBodySchemaGenerator : BodySchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly MetaSchemaGenerator _metaSchemaGenerator;

    public ErrorResponseBodySchemaGenerator(SchemaGenerator defaultSchemaGenerator, MetaSchemaGenerator metaSchemaGenerator,
        LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator, IJsonApiOptions options)
        : base(metaSchemaGenerator, linksVisibilitySchemaGenerator, options)
    {
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _metaSchemaGenerator = metaSchemaGenerator;
    }

    public override bool CanGenerate(Type modelType)
    {
        return modelType == typeof(ErrorResponseDocument);
    }

    protected override OpenApiSchema GenerateBodySchema(Type bodyType, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchemaForErrorObject = _defaultSchemaGenerator.GenerateSchema(typeof(ErrorObject), schemaRepository);
        OpenApiSchema fullSchemaForErrorObject = schemaRepository.Schemas[referenceSchemaForErrorObject.Reference.Id];

        OpenApiSchema referenceSchemaForMeta = _metaSchemaGenerator.GenerateSchema(schemaRepository);
        fullSchemaForErrorObject.Properties[JsonApiPropertyName.Meta] = referenceSchemaForMeta.WrapInExtendedSchema();

        return _defaultSchemaGenerator.GenerateSchema(bodyType, schemaRepository);
    }
}
