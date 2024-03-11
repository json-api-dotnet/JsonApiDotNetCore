using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using SchemaGenerator = Swashbuckle.AspNetCore.SwaggerGen.Patched.SchemaGenerator;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class DocumentSchemaGenerator
{
    private static readonly Type[] JsonApiDocumentWithNullableDataOpenTypes =
    [
        typeof(NullableSecondaryResourceResponseDocument<>),
        typeof(NullableResourceIdentifierResponseDocument<>),
        typeof(NullableToOneRelationshipInRequest<>)
    ];

    private static readonly string[] DocumentPropertyNamesInOrder =
    [
        JsonApiPropertyName.Jsonapi,
        JsonApiPropertyName.Links,
        JsonApiPropertyName.Data,
        JsonApiPropertyName.Errors,
        JsonApiPropertyName.Included,
        JsonApiPropertyName.Meta
    ];

    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly AbstractResourceDataSchemaGenerator _abstractResourceDataSchemaGenerator;
    private readonly ResourceDataSchemaGenerator _resourceDataSchemaGenerator;
    private readonly IncludeDependencyScanner _includeDependencyScanner;
    private readonly IResourceGraph _resourceGraph;
    private readonly IJsonApiOptions _options;

    public DocumentSchemaGenerator(SchemaGenerator defaultSchemaGenerator, AbstractResourceDataSchemaGenerator abstractResourceDataSchemaGenerator,
        ResourceDataSchemaGenerator resourceDataSchemaGenerator, IncludeDependencyScanner includeDependencyScanner, IResourceGraph resourceGraph,
        IJsonApiOptions options)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(abstractResourceDataSchemaGenerator);
        ArgumentGuard.NotNull(resourceDataSchemaGenerator);
        ArgumentGuard.NotNull(includeDependencyScanner);
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(options);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _abstractResourceDataSchemaGenerator = abstractResourceDataSchemaGenerator;
        _resourceDataSchemaGenerator = resourceDataSchemaGenerator;
        _includeDependencyScanner = includeDependencyScanner;
        _resourceGraph = resourceGraph;
        _options = options;
    }

    public OpenApiSchema GenerateSchema(Type modelType, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(modelType);
        ArgumentGuard.NotNull(schemaRepository);

        OpenApiSchema referenceSchemaForDocument = modelType.IsConstructedGenericType
            ? GenerateJsonApiDocumentSchema(modelType, schemaRepository)
            : _defaultSchemaGenerator.GenerateSchema(modelType, schemaRepository);

        OpenApiSchema fullSchemaForDocument = schemaRepository.Schemas[referenceSchemaForDocument.Reference.Id];

        fullSchemaForDocument.SetValuesInMetaToNullable();

        SetJsonApiVersion(fullSchemaForDocument, schemaRepository);

        return referenceSchemaForDocument;
    }

    private OpenApiSchema GenerateJsonApiDocumentSchema(Type documentType, SchemaRepository schemaRepository)
    {
        // There's no way to intercept in the Swashbuckle recursive component schema generation when using inheritance, which we need
        // to perform generic type expansions. As a workaround, we generate an empty base schema upfront. Each time the schema
        // for a derived type is generated, we'll add it to the discriminator mapping.
        _ = _abstractResourceDataSchemaGenerator.Get(schemaRepository);

        Type resourceDataConstructedType = documentType.BaseType!.GenericTypeArguments[0];

        // Ensure all reachable related resource types are available in the discriminator mapping so includes work.
        // Doing this matters when not all endpoints are exposed.
        EnsureResourceTypesAreMappedInDiscriminator(resourceDataConstructedType, schemaRepository);

        OpenApiSchema referenceSchemaForResourceData = _resourceDataSchemaGenerator.GenerateSchema(resourceDataConstructedType, schemaRepository);
        _abstractResourceDataSchemaGenerator.MapDiscriminator(resourceDataConstructedType, referenceSchemaForResourceData, schemaRepository);

        OpenApiSchema referenceSchemaForDocument = _defaultSchemaGenerator.GenerateSchema(documentType, schemaRepository);
        OpenApiSchema fullSchemaForDocument = schemaRepository.Schemas[referenceSchemaForDocument.Reference.Id];

        fullSchemaForDocument.Properties[JsonApiPropertyName.Data] = IsManyDataDocument(documentType)
            ? CreateArrayTypeDataSchema(referenceSchemaForResourceData)
            : CreateExtendedReferenceSchema(referenceSchemaForResourceData);

        if (IsDataPropertyNullableInDocument(documentType))
        {
            SetDataSchemaToNullable(fullSchemaForDocument);
        }

        fullSchemaForDocument.ReorderProperties(DocumentPropertyNamesInOrder);

        return referenceSchemaForDocument;
    }

    private void EnsureResourceTypesAreMappedInDiscriminator(Type resourceDataConstructedType, SchemaRepository schemaRepository)
    {
        Type resourceDataOpenType = resourceDataConstructedType.GetGenericTypeDefinition();

        if (resourceDataOpenType == typeof(ResourceDataInResponse<>))
        {
            Type resourceClrType = resourceDataConstructedType.GetGenericArguments()[0];
            ResourceType resourceType = _resourceGraph.GetResourceType(resourceClrType);

            foreach (ResourceType nextResourceType in _includeDependencyScanner.GetReachableRelatedTypes(resourceType))
            {
                Type nextResourceDataConstructedType = typeof(ResourceDataInResponse<>).MakeGenericType(nextResourceType.ClrType);

                OpenApiSchema nextReferenceSchemaForResourceData =
                    _resourceDataSchemaGenerator.GenerateSchema(nextResourceDataConstructedType, schemaRepository);

                _abstractResourceDataSchemaGenerator.MapDiscriminator(nextResourceDataConstructedType, nextReferenceSchemaForResourceData, schemaRepository);
            }
        }
    }

    private static bool IsManyDataDocument(Type documentType)
    {
        return documentType.BaseType!.GetGenericTypeDefinition() == typeof(ManyData<>);
    }

    private static bool IsDataPropertyNullableInDocument(Type documentType)
    {
        Type documentOpenType = documentType.GetGenericTypeDefinition();

        return JsonApiDocumentWithNullableDataOpenTypes.Contains(documentOpenType);
    }

    private static OpenApiSchema CreateArrayTypeDataSchema(OpenApiSchema referenceSchemaForResourceData)
    {
        return new OpenApiSchema
        {
            Items = referenceSchemaForResourceData,
            Type = "array"
        };
    }

    private static void SetDataSchemaToNullable(OpenApiSchema fullSchemaForDocument)
    {
        OpenApiSchema referenceSchemaForData = fullSchemaForDocument.Properties[JsonApiPropertyName.Data];
        referenceSchemaForData.Nullable = true;
        fullSchemaForDocument.Properties[JsonApiPropertyName.Data] = referenceSchemaForData;
    }

    private void SetJsonApiVersion(OpenApiSchema fullSchemaForDocument, SchemaRepository schemaRepository)
    {
        if (fullSchemaForDocument.Properties.TryGetValue(JsonApiPropertyName.Jsonapi, out OpenApiSchema? referenceSchemaForJsonapi))
        {
            string jsonapiSchemaId = referenceSchemaForJsonapi.AllOf[0].Reference.Id;

            if (!_options.IncludeJsonApiVersion)
            {
                fullSchemaForDocument.Properties.Remove(JsonApiPropertyName.Jsonapi);
                schemaRepository.Schemas.Remove(jsonapiSchemaId);
            }
            else
            {
                OpenApiSchema fullSchemaForJsonapi = schemaRepository.Schemas[jsonapiSchemaId];
                fullSchemaForJsonapi.SetValuesInMetaToNullable();
            }
        }
    }

    private static OpenApiSchema CreateExtendedReferenceSchema(OpenApiSchema referenceSchema)
    {
        return new OpenApiSchema
        {
            AllOf = new List<OpenApiSchema>
            {
                referenceSchema
            }
        };
    }
}
