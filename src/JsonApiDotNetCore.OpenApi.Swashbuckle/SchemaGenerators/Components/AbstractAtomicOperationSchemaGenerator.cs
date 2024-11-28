using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class AbstractAtomicOperationSchemaGenerator
{
    // The discriminator only exists to guide OpenAPI codegen of request bodies. It is silently ignored by the JSON:API server.
    private const string DiscriminatorPropertyName = "openapi:discriminator";

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

        OpenApiSchema referenceSchemaForMeta = _metaSchemaGenerator.GenerateSchema(schemaRepository);

        var fullSchema = new OpenApiSchema
        {
            Type = "object",
            Required = new SortedSet<string>([DiscriminatorPropertyName]),
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [DiscriminatorPropertyName] = new()
                {
                    Type = "string"
                },
                [referenceSchemaForMeta.Reference.Id] = referenceSchemaForMeta.WrapInExtendedSchema()
            },
            AdditionalPropertiesAllowed = false,
            Discriminator = new OpenApiDiscriminator
            {
                PropertyName = DiscriminatorPropertyName,
                Mapping = new SortedDictionary<string, string>(StringComparer.Ordinal)
            },
            Extensions =
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

        if (!schemaRepository.TryLookupByType(AtomicOperationAbstractType, out OpenApiSchema? referenceSchemaForAbstractOperation))
        {
            throw new UnreachableCodeException();
        }

        OpenApiSchema fullSchemaForAbstractOperation = schemaRepository.Schemas[referenceSchemaForAbstractOperation.Reference.Id];
        fullSchemaForAbstractOperation.Discriminator.Mapping.Add(discriminatorValue, referenceSchemaForOperation.Reference.ReferenceV3);
    }
}
