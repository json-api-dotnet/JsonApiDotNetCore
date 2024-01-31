using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

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
    private readonly ResourceDataSchemaGenerator _resourceDataSchemaGenerator;
    private readonly IJsonApiOptions _options;

    public DocumentSchemaGenerator(SchemaGenerator defaultSchemaGenerator, ResourceDataSchemaGenerator resourceDataSchemaGenerator, IJsonApiOptions options)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(resourceDataSchemaGenerator);
        ArgumentGuard.NotNull(options);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _resourceDataSchemaGenerator = resourceDataSchemaGenerator;
        _options = options;
    }

    public OpenApiSchema GenerateSchema(Type modelType, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(modelType);
        ArgumentGuard.NotNull(schemaRepository);

        OpenApiSchema referenceSchemaForDocument = GenerateJsonApiDocumentSchema(modelType, schemaRepository);
        OpenApiSchema fullSchemaForDocument = schemaRepository.Schemas[referenceSchemaForDocument.Reference.Id];

        if (IsDataPropertyNullableInDocument(modelType))
        {
            SetDataSchemaToNullable(fullSchemaForDocument);
        }

        fullSchemaForDocument.SetValuesInMetaToNullable();

        SetJsonApiVersion(fullSchemaForDocument, schemaRepository);

        return referenceSchemaForDocument;
    }

    private OpenApiSchema GenerateJsonApiDocumentSchema(Type documentType, SchemaRepository schemaRepository)
    {
        Type resourceDataType = documentType.BaseType!.GenericTypeArguments[0];

        if (!schemaRepository.TryLookupByType(resourceDataType, out OpenApiSchema referenceSchemaForResourceData))
        {
            referenceSchemaForResourceData = _resourceDataSchemaGenerator.GenerateSchema(resourceDataType, schemaRepository);
        }

        OpenApiSchema referenceSchemaForDocument = _defaultSchemaGenerator.GenerateSchema(documentType, schemaRepository);
        OpenApiSchema fullSchemaForDocument = schemaRepository.Schemas[referenceSchemaForDocument.Reference.Id];

        fullSchemaForDocument.Properties[JsonApiPropertyName.Data] = IsManyDataDocument(documentType)
            ? CreateArrayTypeDataSchema(referenceSchemaForResourceData)
            : CreateExtendedReferenceSchema(referenceSchemaForResourceData);

        fullSchemaForDocument.ReorderProperties(DocumentPropertyNamesInOrder);

        return referenceSchemaForDocument;
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
