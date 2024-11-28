using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class AbstractResourceDataSchemaGenerator
{
    private static readonly Type ResourceDataAbstractType = typeof(ResourceData);

    private readonly MetaSchemaGenerator _metaSchemaGenerator;
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;
    private readonly IResourceGraph _resourceGraph;

    public AbstractResourceDataSchemaGenerator(MetaSchemaGenerator metaSchemaGenerator, JsonApiSchemaIdSelector schemaIdSelector, IResourceGraph resourceGraph)
    {
        ArgumentGuard.NotNull(metaSchemaGenerator);
        ArgumentGuard.NotNull(schemaIdSelector);
        ArgumentGuard.NotNull(resourceGraph);

        _metaSchemaGenerator = metaSchemaGenerator;
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

        OpenApiSchema referenceSchemaForResourceType = GenerateEmptyResourceTypeSchema(schemaRepository);
        OpenApiSchema referenceSchemaForMeta = _metaSchemaGenerator.GenerateSchema(schemaRepository);

        var fullSchema = new OpenApiSchema
        {
            Required = new SortedSet<string>([JsonApiPropertyName.Type]),
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [JsonApiPropertyName.Type] = new()
                {
                    AllOf = [referenceSchemaForResourceType]
                },
                [referenceSchemaForMeta.Reference.Id] = referenceSchemaForMeta.WrapInExtendedSchema()
            },
            AdditionalPropertiesAllowed = false,
            Discriminator = new OpenApiDiscriminator
            {
                PropertyName = JsonApiPropertyName.Type,
                Mapping = new SortedDictionary<string, string>(StringComparer.Ordinal)
            },
            Extensions =
            {
                ["x-abstract"] = new OpenApiBoolean(true)
            }
        };

        string schemaId = _schemaIdSelector.GetSchemaId(ResourceDataAbstractType);

        referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(ResourceDataAbstractType, schemaId);

        return referenceSchema;
    }

    private OpenApiSchema GenerateEmptyResourceTypeSchema(SchemaRepository schemaRepository)
    {
        var fullSchema = new OpenApiSchema
        {
            Type = "string",
            Extensions =
            {
                [StringEnumOrderingFilter.RequiresSortKey] = new OpenApiBoolean(true)
            }
        };

        string resourceTypeSchemaId = _schemaIdSelector.GetResourceTypeSchemaId(null);
        return schemaRepository.AddDefinition(resourceTypeSchemaId, fullSchema);
    }

    public void MapDiscriminator(Type resourceDataConstructedType, OpenApiSchema referenceSchemaForResourceData, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(resourceDataConstructedType);
        ArgumentGuard.NotNull(referenceSchemaForResourceData);
        ArgumentGuard.NotNull(schemaRepository);

        var resourceTypeInfo = ResourceTypeInfo.Create(resourceDataConstructedType, _resourceGraph);

        if (resourceTypeInfo.ResourceDataOpenType == typeof(ResourceDataInResponse<>))
        {
            if (!schemaRepository.TryLookupByType(ResourceDataAbstractType, out OpenApiSchema? referenceSchemaForAbstractResourceData))
            {
                throw new UnreachableCodeException();
            }

            OpenApiSchema fullSchemaForAbstractResourceData = schemaRepository.Schemas[referenceSchemaForAbstractResourceData.Reference.Id];
            string dataSchemaId = referenceSchemaForResourceData.Reference.ReferenceV3;
            string publicName = resourceTypeInfo.ResourceType.PublicName;

            if (fullSchemaForAbstractResourceData.Discriminator.Mapping.TryAdd(publicName, dataSchemaId))
            {
                MapResourceType(publicName, schemaRepository);
            }
        }
    }

    private void MapResourceType(string publicName, SchemaRepository schemaRepository)
    {
        string schemaId = _schemaIdSelector.GetResourceTypeSchemaId(null);
        OpenApiSchema fullSchema = schemaRepository.Schemas[schemaId];
        fullSchema.Enum.Add(new OpenApiString(publicName));
    }
}
