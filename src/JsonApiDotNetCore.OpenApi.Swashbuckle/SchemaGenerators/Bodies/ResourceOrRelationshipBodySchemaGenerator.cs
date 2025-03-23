using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Relationships;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Bodies;

/// <summary>
/// Generates the OpenAPI component schema for a resource/relationship request/response body.
/// </summary>
internal sealed class ResourceOrRelationshipBodySchemaGenerator : BodySchemaGenerator
{
    private static readonly Type[] RequestBodySchemaTypes =
    [
        typeof(CreateRequestDocument<>),
        typeof(UpdateRequestDocument<>),
        typeof(ToOneInRequest<>),
        typeof(NullableToOneInRequest<>),
        typeof(ToManyInRequest<>)
    ];

    private static readonly Type[] ResponseBodySchemaTypes =
    [
        typeof(CollectionResponseDocument<>),
        typeof(PrimaryResponseDocument<>),
        typeof(SecondaryResponseDocument<>),
        typeof(NullableSecondaryResponseDocument<>),
        typeof(IdentifierResponseDocument<>),
        typeof(NullableIdentifierResponseDocument<>),
        typeof(IdentifierCollectionResponseDocument<>)
    ];

    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly DataContainerSchemaGenerator _dataContainerSchemaGenerator;
    private readonly IResourceGraph _resourceGraph;

    public ResourceOrRelationshipBodySchemaGenerator(SchemaGenerator defaultSchemaGenerator, DataContainerSchemaGenerator dataContainerSchemaGenerator,
        MetaSchemaGenerator metaSchemaGenerator, LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator, IJsonApiOptions options,
        IResourceGraph resourceGraph)
        : base(metaSchemaGenerator, linksVisibilitySchemaGenerator, options)
    {
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);
        ArgumentNullException.ThrowIfNull(dataContainerSchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceGraph);

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
        ArgumentNullException.ThrowIfNull(bodyType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByType(bodyType, out OpenApiSchema? referenceSchemaForBody))
        {
            return referenceSchemaForBody;
        }

        var resourceSchemaType = ResourceSchemaType.Create(bodyType, _resourceGraph);
        bool isRequestSchema = RequestBodySchemaTypes.Contains(resourceSchemaType.SchemaOpenType);

        _ = _dataContainerSchemaGenerator.GenerateSchema(bodyType, resourceSchemaType.ResourceType, isRequestSchema, schemaRepository);

        referenceSchemaForBody = _defaultSchemaGenerator.GenerateSchema(bodyType, schemaRepository);
        OpenApiSchema fullSchemaForBody = schemaRepository.Schemas[referenceSchemaForBody.Reference.Id].UnwrapLastExtendedSchema();

        if (JsonApiSchemaFacts.HasNullableDataProperty(resourceSchemaType.SchemaOpenType))
        {
            fullSchemaForBody.Properties[JsonApiPropertyName.Data].Nullable = true;
        }

        return referenceSchemaForBody;
    }
}
