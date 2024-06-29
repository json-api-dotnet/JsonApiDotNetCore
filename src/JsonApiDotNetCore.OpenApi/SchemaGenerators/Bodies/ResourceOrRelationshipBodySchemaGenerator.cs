using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using JsonApiDotNetCore.OpenApi.SchemaGenerators.Components;
using JsonApiDotNetCore.OpenApi.SwaggerComponents;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SchemaGenerators.Bodies;

/// <summary>
/// Generates the OpenAPI component schema for a resource/relationship request/response body.
/// </summary>
internal sealed class ResourceOrRelationshipBodySchemaGenerator : BodySchemaGenerator
{
    private static readonly Type[] RequestBodySchemaTypes =
    [
        typeof(CreateResourceRequestDocument<>),
        typeof(UpdateResourceRequestDocument<>),
        typeof(ToOneRelationshipInRequest<>),
        typeof(NullableToOneRelationshipInRequest<>),
        typeof(ToManyRelationshipInRequest<>)
    ];

    private static readonly Type[] ResponseBodySchemaTypes =
    [
        typeof(ResourceCollectionResponseDocument<>),
        typeof(PrimaryResourceResponseDocument<>),
        typeof(SecondaryResourceResponseDocument<>),
        typeof(NullableSecondaryResourceResponseDocument<>),
        typeof(ResourceIdentifierResponseDocument<>),
        typeof(NullableResourceIdentifierResponseDocument<>),
        typeof(ResourceIdentifierCollectionResponseDocument<>)
    ];

    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly DataContainerSchemaGenerator _dataContainerSchemaGenerator;
    private readonly IResourceGraph _resourceGraph;

    public ResourceOrRelationshipBodySchemaGenerator(SchemaGenerator defaultSchemaGenerator, DataContainerSchemaGenerator dataContainerSchemaGenerator,
        MetaSchemaGenerator metaSchemaGenerator, LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator, IResourceGraph resourceGraph,
        IJsonApiOptions options)
        : base(metaSchemaGenerator, linksVisibilitySchemaGenerator, options)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(dataContainerSchemaGenerator);
        ArgumentGuard.NotNull(resourceGraph);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _dataContainerSchemaGenerator = dataContainerSchemaGenerator;
        _resourceGraph = resourceGraph;
    }

    public override bool CanGenerate(Type modelType)
    {
        Type modelOpenType = modelType.ConstructedToOpenType();
        return RequestBodySchemaTypes.Contains(modelOpenType) || ResponseBodySchemaTypes.Contains(modelOpenType);
    }

    protected override OpenApiSchema GenerateBodySchema(Type bodyType, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(bodyType);
        ArgumentGuard.NotNull(schemaRepository);

        if (schemaRepository.TryLookupByType(bodyType, out OpenApiSchema? referenceSchemaForBody))
        {
            return referenceSchemaForBody;
        }

        var resourceTypeInfo = ResourceTypeInfo.Create(bodyType, _resourceGraph);
        bool isRequestSchema = RequestBodySchemaTypes.Contains(bodyType.ConstructedToOpenType());

        _ = _dataContainerSchemaGenerator.GenerateSchema(bodyType, resourceTypeInfo.ResourceType, isRequestSchema, schemaRepository);

        referenceSchemaForBody = _defaultSchemaGenerator.GenerateSchema(bodyType, schemaRepository);
        OpenApiSchema fullSchemaForBody = schemaRepository.Schemas[referenceSchemaForBody.Reference.Id].UnwrapLastExtendedSchema();

        if (JsonApiSchemaFacts.HasNullableDataProperty(bodyType))
        {
            fullSchemaForBody.Properties[JsonApiPropertyName.Data].Nullable = true;
        }

        return referenceSchemaForBody;
    }
}
