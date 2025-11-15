using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Documents;

/// <summary>
/// Generates the OpenAPI component schema for a resource/relationship request/response document.
/// </summary>
internal sealed class ResourceOrRelationshipDocumentSchemaGenerator : DocumentSchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly DataContainerSchemaGenerator _dataContainerSchemaGenerator;
    private readonly IResourceGraph _resourceGraph;

    public ResourceOrRelationshipDocumentSchemaGenerator(SchemaGenerationTracer schemaGenerationTracer, SchemaGenerator defaultSchemaGenerator,
        DataContainerSchemaGenerator dataContainerSchemaGenerator, MetaSchemaGenerator metaSchemaGenerator,
        LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator, IJsonApiOptions options, IResourceGraph resourceGraph)
        : base(schemaGenerationTracer, metaSchemaGenerator, linksVisibilitySchemaGenerator, options)
    {
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);
        ArgumentNullException.ThrowIfNull(dataContainerSchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceGraph);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _dataContainerSchemaGenerator = dataContainerSchemaGenerator;
        _resourceGraph = resourceGraph;
    }

    public override bool CanGenerate(Type schemaType)
    {
        return JsonApiSchemaFacts.IsRequestDocumentSchemaType(schemaType) || JsonApiSchemaFacts.IsResponseDocumentSchemaType(schemaType);
    }

    protected override OpenApiSchemaReference GenerateDocumentSchema(Type schemaType, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        var resourceSchemaType = ResourceSchemaType.Create(schemaType, _resourceGraph);
        bool isRequestSchema = JsonApiSchemaFacts.IsRequestDocumentSchemaType(resourceSchemaType.SchemaOpenType);

        _dataContainerSchemaGenerator.GenerateSchema(schemaType, resourceSchemaType.ResourceType, isRequestSchema, !isRequestSchema, schemaRepository);

        OpenApiSchemaReference referenceSchemaForDocument = _defaultSchemaGenerator.GenerateSchema(schemaType, schemaRepository).AsReferenceSchema();

        OpenApiSchema inlineSchemaForDocument =
            schemaRepository.Schemas[referenceSchemaForDocument.GetReferenceId()].UnwrapLastExtendedSchema().AsInlineSchema();

        if (JsonApiSchemaFacts.HasNullableDataProperty(resourceSchemaType.SchemaOpenType))
        {
            inlineSchemaForDocument.Properties ??= new Dictionary<string, IOpenApiSchema>();
            inlineSchemaForDocument.Properties[JsonApiPropertyName.Data].AsInlineSchema().SetNullable(true);
        }

        return referenceSchemaForDocument;
    }
}
