using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class AbstractAtomicOperationSchemaGenerator
{
    private static readonly Type AtomicOperationAbstractType = typeof(AtomicOperation);

    private readonly MetaSchemaGenerator _metaSchemaGenerator;
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;

    public AbstractAtomicOperationSchemaGenerator(MetaSchemaGenerator metaSchemaGenerator, JsonApiSchemaIdSelector schemaIdSelector)
    {
        ArgumentGuard.NotNull(metaSchemaGenerator);
        ArgumentGuard.NotNull(schemaIdSelector);

        _metaSchemaGenerator = metaSchemaGenerator;
        _schemaIdSelector = schemaIdSelector;
    }

    public OpenApiSchema GenerateSchema(SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(schemaRepository);

        if (schemaRepository.TryLookupByType(AtomicOperationAbstractType, out OpenApiSchema? referenceSchema))
        {
            return referenceSchema;
        }

        // The discriminator only exists to guide OpenAPI codegen. The property is ignored by JsonApiDotNetCore.
        string discriminatorPropertyName = _schemaIdSelector.GetAtomicOperationDiscriminatorName();

        referenceSchema = _metaSchemaGenerator.GenerateSchema(schemaRepository);
        string metaSchemaId = referenceSchema.Reference.Id;

        var fullSchema = new OpenApiSchema
        {
            Type = "object",
            Required = new SortedSet<string>([discriminatorPropertyName]),
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [metaSchemaId] = referenceSchema.WrapInExtendedSchema()
            },
            AdditionalPropertiesAllowed = false,
            Discriminator = new OpenApiDiscriminator
            {
                PropertyName = discriminatorPropertyName,
                Mapping = new SortedDictionary<string, string>(StringComparer.Ordinal)
            },
            Extensions = new Dictionary<string, IOpenApiExtension>
            {
                ["x-abstract"] = new OpenApiBoolean(true)
            }
        };

        string schemaId = _schemaIdSelector.GetSchemaId(AtomicOperationAbstractType);

        referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(AtomicOperationAbstractType, schemaId);

        return referenceSchema;
    }

    public void MapDiscriminator(OpenApiSchema referenceSchemaForOperation, string discriminatorValue, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(referenceSchemaForOperation);
        ArgumentGuard.NotNull(schemaRepository);

        if (schemaRepository.TryLookupByType(AtomicOperationAbstractType, out OpenApiSchema? referenceSchemaForAbstractOperation))
        {
            OpenApiSchema fullSchemaForAbstractOperation = schemaRepository.Schemas[referenceSchemaForAbstractOperation.Reference.Id];
            fullSchemaForAbstractOperation.Discriminator.Mapping[discriminatorValue] = referenceSchemaForOperation.Reference.ReferenceV3;
        }
    }
}
