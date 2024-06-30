using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.SwaggerComponents;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SchemaGenerators.Components;

internal sealed class AbstractResourceDataSchemaGenerator
{
    private static readonly Type ResourceDataAbstractType = typeof(ResourceData);

    private readonly JsonApiSchemaIdSelector _schemaIdSelector;
    private readonly IResourceGraph _resourceGraph;

    public AbstractResourceDataSchemaGenerator(JsonApiSchemaIdSelector schemaIdSelector, IResourceGraph resourceGraph)
    {
        ArgumentGuard.NotNull(schemaIdSelector);
        ArgumentGuard.NotNull(resourceGraph);

        _schemaIdSelector = schemaIdSelector;
        _resourceGraph = resourceGraph;
    }

    public OpenApiSchema GenerateSchema(SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(schemaRepository);

        if (schemaRepository.TryLookupByType(ResourceDataAbstractType, out OpenApiSchema? referenceSchema))
        {
            return referenceSchema;
        }

        var fullSchema = new OpenApiSchema
        {
            Required = new HashSet<string>
            {
                "type"
            },
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new()
                {
                    MinLength = 1,
                    Type = "string"
                }
            },
            AdditionalPropertiesAllowed = false,
            Discriminator = new OpenApiDiscriminator
            {
                PropertyName = "type",
                Mapping = new SortedDictionary<string, string>(StringComparer.Ordinal)
            },
            Extensions = new Dictionary<string, IOpenApiExtension>
            {
                ["x-abstract"] = new OpenApiBoolean(true)
            }
        };

        string schemaId = _schemaIdSelector.GetSchemaId(ResourceDataAbstractType);

        referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(ResourceDataAbstractType, schemaId);

        return referenceSchema;
    }

    public void MapDiscriminator(Type resourceDataConstructedType, OpenApiSchema referenceSchemaForResourceData, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(resourceDataConstructedType);
        ArgumentGuard.NotNull(referenceSchemaForResourceData);
        ArgumentGuard.NotNull(schemaRepository);

        var resourceTypeInfo = ResourceTypeInfo.Create(resourceDataConstructedType, _resourceGraph);

        if (resourceTypeInfo.ResourceDataOpenType == typeof(ResourceDataInResponse<>))
        {
            if (schemaRepository.TryLookupByType(ResourceDataAbstractType, out OpenApiSchema? referenceSchemaForAbstractResourceData))
            {
                OpenApiSchema fullSchemaForAbstractResourceData = schemaRepository.Schemas[referenceSchemaForAbstractResourceData.Reference.Id];

                fullSchemaForAbstractResourceData.Discriminator.Mapping[resourceTypeInfo.ResourceType.PublicName] =
                    referenceSchemaForResourceData.Reference.ReferenceV3;
            }
        }
    }
}
