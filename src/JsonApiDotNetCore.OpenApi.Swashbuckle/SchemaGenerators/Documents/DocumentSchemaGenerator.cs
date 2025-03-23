using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Documents;

/// <summary>
/// Generates the OpenAPI component schema for a request and/or response document.
/// </summary>
internal abstract class DocumentSchemaGenerator
{
    private readonly MetaSchemaGenerator _metaSchemaGenerator;
    private readonly LinksVisibilitySchemaGenerator _linksVisibilitySchemaGenerator;
    private readonly IJsonApiOptions _options;

    protected DocumentSchemaGenerator(MetaSchemaGenerator metaSchemaGenerator, LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator,
        IJsonApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(metaSchemaGenerator);
        ArgumentNullException.ThrowIfNull(linksVisibilitySchemaGenerator);
        ArgumentNullException.ThrowIfNull(options);

        _metaSchemaGenerator = metaSchemaGenerator;
        _linksVisibilitySchemaGenerator = linksVisibilitySchemaGenerator;
        _options = options;
    }

    public abstract bool CanGenerate(Type schemaType);

    public OpenApiSchema GenerateSchema(Type schemaType, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByType(schemaType, out OpenApiSchema? referenceSchema))
        {
            return referenceSchema;
        }

        _metaSchemaGenerator.GenerateSchema(schemaRepository);

        referenceSchema = GenerateDocumentSchema(schemaType, schemaRepository);
        OpenApiSchema fullSchema = schemaRepository.Schemas[referenceSchema.Reference.Id];

        _linksVisibilitySchemaGenerator.UpdateSchemaForTopLevel(schemaType, fullSchema, schemaRepository);

        SetJsonApiVersion(fullSchema, schemaRepository);

        return referenceSchema;
    }

    protected abstract OpenApiSchema GenerateDocumentSchema(Type schemaType, SchemaRepository schemaRepository);

    private void SetJsonApiVersion(OpenApiSchema fullSchema, SchemaRepository schemaRepository)
    {
        if (fullSchema.Properties.ContainsKey(JsonApiPropertyName.Jsonapi) && !_options.IncludeJsonApiVersion)
        {
            fullSchema.Properties.Remove(JsonApiPropertyName.Jsonapi);
            schemaRepository.Schemas.Remove(JsonApiPropertyName.Jsonapi);
        }
    }
}
