using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Bodies;

// TODO: Rename to DocumentSchemaGenerator, for consistency?

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
        ArgumentNullException.ThrowIfNull(metaSchemaGenerator);
        ArgumentNullException.ThrowIfNull(linksVisibilitySchemaGenerator);
        ArgumentNullException.ThrowIfNull(options);

        _metaSchemaGenerator = metaSchemaGenerator;
        _linksVisibilitySchemaGenerator = linksVisibilitySchemaGenerator;
        _options = options;
    }

    // TODO: Rename to schemaType everywhere (search for: Type modelType)
    public abstract bool CanGenerate(Type modelType);

    // TODO: Verify consistency in GenerateSchema() parameters/variables for xxxSchema[Open|Constructed]Type
    public OpenApiSchema GenerateSchema(Type bodyType, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(bodyType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByType(bodyType, out OpenApiSchema? referenceSchema))
        {
            return referenceSchema;
        }

        _metaSchemaGenerator.GenerateSchema(schemaRepository);

        referenceSchema = GenerateBodySchema(bodyType, schemaRepository);
        OpenApiSchema fullSchema = schemaRepository.Schemas[referenceSchema.Reference.Id];

        _linksVisibilitySchemaGenerator.UpdateSchemaForTopLevel(bodyType, fullSchema, schemaRepository);

        SetJsonApiVersion(fullSchema, schemaRepository);

        return referenceSchema;
    }

    protected abstract OpenApiSchema GenerateBodySchema(Type bodyType, SchemaRepository schemaRepository);

    private void SetJsonApiVersion(OpenApiSchema fullSchema, SchemaRepository schemaRepository)
    {
        if (fullSchema.Properties.ContainsKey(JsonApiPropertyName.Jsonapi) && !_options.IncludeJsonApiVersion)
        {
            fullSchema.Properties.Remove(JsonApiPropertyName.Jsonapi);
            schemaRepository.Schemas.Remove(JsonApiPropertyName.Jsonapi);
        }
    }
}
