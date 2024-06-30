using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.SchemaGenerators.Components;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SchemaGenerators.Bodies;

/// <summary>
/// Generates the OpenAPI component schema for a request and/or response body.
/// </summary>
internal abstract class BodySchemaGenerator
{
    private readonly MetaSchemaGenerator _metaSchemaGenerator;
    private readonly LinksVisibilitySchemaGenerator _linksVisibilitySchemaGenerator;
    private readonly IJsonApiOptions _options;

    protected BodySchemaGenerator(MetaSchemaGenerator metaSchemaGenerator, LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator,
        IJsonApiOptions options)
    {
        ArgumentGuard.NotNull(metaSchemaGenerator);
        ArgumentGuard.NotNull(linksVisibilitySchemaGenerator);
        ArgumentGuard.NotNull(options);

        _metaSchemaGenerator = metaSchemaGenerator;
        _linksVisibilitySchemaGenerator = linksVisibilitySchemaGenerator;
        _options = options;
    }

    public abstract bool CanGenerate(Type modelType);

    public OpenApiSchema GenerateSchema(Type bodyType, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(bodyType);
        ArgumentGuard.NotNull(schemaRepository);

        if (schemaRepository.TryLookupByType(bodyType, out OpenApiSchema? referenceSchema))
        {
            return referenceSchema;
        }

        _metaSchemaGenerator.GenerateSchema(schemaRepository);

        referenceSchema = GenerateBodySchema(bodyType, schemaRepository);
        OpenApiSchema fullSchema = schemaRepository.Schemas[referenceSchema.Reference.Id];

        _linksVisibilitySchemaGenerator.UpdateSchemaForTopLevel(bodyType, fullSchema, schemaRepository);

        SetJsonApiVersion(fullSchema);

        return referenceSchema;
    }

    protected abstract OpenApiSchema GenerateBodySchema(Type bodyType, SchemaRepository schemaRepository);

    private void SetJsonApiVersion(OpenApiSchema fullSchema)
    {
        if (fullSchema.Properties.ContainsKey(JsonApiPropertyName.Jsonapi) && !_options.IncludeJsonApiVersion)
        {
            fullSchema.Properties.Remove(JsonApiPropertyName.Jsonapi);
        }
    }
}
