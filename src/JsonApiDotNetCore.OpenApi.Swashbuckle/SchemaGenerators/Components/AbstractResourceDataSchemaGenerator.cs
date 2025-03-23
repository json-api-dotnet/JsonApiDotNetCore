using System.Diagnostics;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class AbstractResourceDataSchemaGenerator
{
    private static readonly Type ResourceInResponseAbstractType = typeof(ResourceInResponse);

    private readonly MetaSchemaGenerator _metaSchemaGenerator;
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;
    private readonly IResourceGraph _resourceGraph;

    public AbstractResourceDataSchemaGenerator(MetaSchemaGenerator metaSchemaGenerator, JsonApiSchemaIdSelector schemaIdSelector, IResourceGraph resourceGraph)
    {
        ArgumentNullException.ThrowIfNull(metaSchemaGenerator);
        ArgumentNullException.ThrowIfNull(schemaIdSelector);
        ArgumentNullException.ThrowIfNull(resourceGraph);

        _metaSchemaGenerator = metaSchemaGenerator;
        _schemaIdSelector = schemaIdSelector;
        _resourceGraph = resourceGraph;
    }

    public OpenApiSchema GenerateSchema(SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByType(ResourceInResponseAbstractType, out OpenApiSchema? referenceSchema))
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

        string schemaId = _schemaIdSelector.GetSchemaId(ResourceInResponseAbstractType);

        referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(ResourceInResponseAbstractType, schemaId);

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
        ArgumentNullException.ThrowIfNull(resourceDataConstructedType);
        ArgumentNullException.ThrowIfNull(referenceSchemaForResourceData);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        var resourceSchemaType = ResourceSchemaType.Create(resourceDataConstructedType, _resourceGraph);

        if (resourceSchemaType.SchemaOpenType == typeof(DataInResponse<>))
        {
            if (!schemaRepository.TryLookupByType(ResourceInResponseAbstractType, out OpenApiSchema? referenceSchemaForAbstractResourceData))
            {
                throw new UnreachableException();
            }

            OpenApiSchema fullSchemaForAbstractResourceData = schemaRepository.Schemas[referenceSchemaForAbstractResourceData.Reference.Id];
            string dataSchemaId = referenceSchemaForResourceData.Reference.ReferenceV3;
            string publicName = resourceSchemaType.ResourceType.PublicName;

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
