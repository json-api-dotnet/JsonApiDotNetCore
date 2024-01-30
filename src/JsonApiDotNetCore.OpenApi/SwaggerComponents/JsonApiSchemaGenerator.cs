using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.OpenApi.JsonApiObjects;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class JsonApiSchemaGenerator : ISchemaGenerator
{
    private static readonly Type[] JsonApiDocumentOpenTypes =
    [
        typeof(ResourceCollectionResponseDocument<>),
        typeof(PrimaryResourceResponseDocument<>),
        typeof(SecondaryResourceResponseDocument<>),
        typeof(NullableSecondaryResourceResponseDocument<>),
        typeof(ResourcePostRequestDocument<>),
        typeof(ResourcePatchRequestDocument<>),
        typeof(ResourceIdentifierCollectionResponseDocument<>),
        typeof(ResourceIdentifierResponseDocument<>),
        typeof(NullableResourceIdentifierResponseDocument<>)
    ];

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

    private static readonly OpenApiSchema IdTypeSchema = new()
    {
        Type = "string"
    };

    private readonly ISchemaGenerator _defaultSchemaGenerator;
    private readonly IJsonApiOptions _options;
    private readonly ResourceDataSchemaGenerator _resourceDataSchemaGenerator;
    private readonly SchemaRepositoryAccessor _schemaRepositoryAccessor = new();

    public JsonApiSchemaGenerator(SchemaGenerator defaultSchemaGenerator, IResourceGraph resourceGraph, IJsonApiOptions options,
        ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(resourceFieldValidationMetadataProvider);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _options = options;

        _resourceDataSchemaGenerator = new ResourceDataSchemaGenerator(defaultSchemaGenerator, _schemaRepositoryAccessor, resourceGraph, options,
            resourceFieldValidationMetadataProvider);
    }

    public OpenApiSchema GenerateSchema(Type modelType, SchemaRepository schemaRepository, MemberInfo? memberInfo = null, ParameterInfo? parameterInfo = null,
        ApiParameterRouteInfo? routeInfo = null)
    {
        ArgumentGuard.NotNull(modelType);
        ArgumentGuard.NotNull(schemaRepository);

        _schemaRepositoryAccessor.Current = schemaRepository;

        if (parameterInfo is { Name: "id" } && IsJsonApiParameter(parameterInfo))
        {
            return IdTypeSchema;
        }

        if (schemaRepository.TryLookupByType(modelType, out OpenApiSchema jsonApiDocumentSchema))
        {
            // For unknown reasons, Swashbuckle chooses to wrap root request bodies, but not response bodies.
            // See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/861#issuecomment-1373631712
            return memberInfo != null || parameterInfo != null
                ? _defaultSchemaGenerator.GenerateSchema(modelType, schemaRepository, memberInfo, parameterInfo)
                : jsonApiDocumentSchema;
        }

        if (IsJsonApiDocument(modelType))
        {
            OpenApiSchema referenceSchemaForDocument = GenerateJsonApiDocumentSchema(modelType);
            OpenApiSchema fullSchemaForDocument = _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForDocument.Reference.Id];

            if (IsDataPropertyNullableInDocument(modelType))
            {
                SetDataSchemaToNullable(fullSchemaForDocument);
            }

            fullSchemaForDocument.SetValuesInMetaToNullable();

            SetJsonApiVersion(fullSchemaForDocument);

            // Schema might depend on other schemas not handled by us, so should not return here.
        }

        return _defaultSchemaGenerator.GenerateSchema(modelType, schemaRepository, memberInfo, parameterInfo, routeInfo);
    }

    private static bool IsJsonApiParameter(ParameterInfo parameter)
    {
        return parameter.Member.DeclaringType != null && parameter.Member.DeclaringType.IsAssignableTo(typeof(CoreJsonApiController));
    }

    private static bool IsJsonApiDocument(Type type)
    {
        return type.IsConstructedGenericType && JsonApiDocumentOpenTypes.Contains(type.GetGenericTypeDefinition());
    }

    private OpenApiSchema GenerateJsonApiDocumentSchema(Type documentType)
    {
        Type resourceDataType = documentType.BaseType!.GenericTypeArguments[0];

        if (!_schemaRepositoryAccessor.Current.TryLookupByType(resourceDataType, out OpenApiSchema referenceSchemaForResourceData))
        {
            referenceSchemaForResourceData = _resourceDataSchemaGenerator.GenerateSchema(resourceDataType);
        }

        OpenApiSchema referenceSchemaForDocument = _defaultSchemaGenerator.GenerateSchema(documentType, _schemaRepositoryAccessor.Current);
        OpenApiSchema fullSchemaForDocument = _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForDocument.Reference.Id];

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

    private void SetDataSchemaToNullable(OpenApiSchema fullSchemaForDocument)
    {
        OpenApiSchema referenceSchemaForData = fullSchemaForDocument.Properties[JsonApiPropertyName.Data];
        referenceSchemaForData.Nullable = true;
        fullSchemaForDocument.Properties[JsonApiPropertyName.Data] = referenceSchemaForData;
    }

    private void SetJsonApiVersion(OpenApiSchema fullSchemaForDocument)
    {
        if (fullSchemaForDocument.Properties.TryGetValue(JsonApiPropertyName.Jsonapi, out OpenApiSchema? referenceSchemaForJsonapi))
        {
            string jsonapiSchemaId = referenceSchemaForJsonapi.AllOf[0].Reference.Id;

            if (!_options.IncludeJsonApiVersion)
            {
                fullSchemaForDocument.Properties.Remove(JsonApiPropertyName.Jsonapi);
                _schemaRepositoryAccessor.Current.Schemas.Remove(jsonapiSchemaId);
            }
            else
            {
                OpenApiSchema fullSchemaForJsonapi = _schemaRepositoryAccessor.Current.Schemas[jsonapiSchemaId];
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
