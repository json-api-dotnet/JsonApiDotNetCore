using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class DocumentSchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly AbstractResourceDataSchemaGenerator _abstractResourceDataSchemaGenerator;
    private readonly ResourceDataSchemaGenerator _resourceDataSchemaGenerator;
    private readonly MetaSchemaGenerator _metaSchemaGenerator;
    private readonly LinksVisibilitySchemaGenerator _linksVisibilitySchemaGenerator;
    private readonly IncludeDependencyScanner _includeDependencyScanner;
    private readonly IResourceGraph _resourceGraph;
    private readonly IJsonApiOptions _options;

    public DocumentSchemaGenerator(SchemaGenerator defaultSchemaGenerator, AbstractResourceDataSchemaGenerator abstractResourceDataSchemaGenerator,
        ResourceDataSchemaGenerator resourceDataSchemaGenerator, MetaSchemaGenerator metaSchemaGenerator,
        LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator, IncludeDependencyScanner includeDependencyScanner, IResourceGraph resourceGraph,
        IJsonApiOptions options)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(abstractResourceDataSchemaGenerator);
        ArgumentGuard.NotNull(resourceDataSchemaGenerator);
        ArgumentGuard.NotNull(metaSchemaGenerator);
        ArgumentGuard.NotNull(linksVisibilitySchemaGenerator);
        ArgumentGuard.NotNull(includeDependencyScanner);
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(options);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _abstractResourceDataSchemaGenerator = abstractResourceDataSchemaGenerator;
        _resourceDataSchemaGenerator = resourceDataSchemaGenerator;
        _metaSchemaGenerator = metaSchemaGenerator;
        _linksVisibilitySchemaGenerator = linksVisibilitySchemaGenerator;
        _includeDependencyScanner = includeDependencyScanner;
        _resourceGraph = resourceGraph;
        _options = options;
    }

    public OpenApiSchema GenerateSchema(Type modelType, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(modelType);
        ArgumentGuard.NotNull(schemaRepository);

        _ = _metaSchemaGenerator.GenerateSchema(schemaRepository);

        if (modelType == typeof(ErrorResponseDocument))
        {
            GenerateErrorObjectSchema(schemaRepository);
        }

        OpenApiSchema referenceSchemaForDocument = modelType.IsConstructedGenericType
            ? GenerateJsonApiDocumentSchema(modelType, schemaRepository)
            : _defaultSchemaGenerator.GenerateSchema(modelType, schemaRepository);

        OpenApiSchema fullSchemaForDocument = schemaRepository.Schemas[referenceSchemaForDocument.Reference.Id];

        SetJsonApiVersion(fullSchemaForDocument);

        _linksVisibilitySchemaGenerator.UpdateSchemaForTopLevel(modelType, fullSchemaForDocument, schemaRepository);

        return referenceSchemaForDocument;
    }

    private void GenerateErrorObjectSchema(SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchemaForErrorObject = _defaultSchemaGenerator.GenerateSchema(typeof(ErrorObject), schemaRepository);
        OpenApiSchema fullSchemaForErrorObject = schemaRepository.Schemas[referenceSchemaForErrorObject.Reference.Id];

        OpenApiSchema referenceSchemaForMeta = _metaSchemaGenerator.GenerateSchema(schemaRepository);
        fullSchemaForErrorObject.Properties[JsonApiPropertyName.Meta] = referenceSchemaForMeta.WrapInExtendedSchema();
    }

    private OpenApiSchema GenerateJsonApiDocumentSchema(Type documentType, SchemaRepository schemaRepository)
    {
        // There's no way to intercept in the Swashbuckle recursive component schema generation when using inheritance, which we need
        // to perform generic type expansions. As a workaround, we generate an empty base schema upfront. Each time the schema
        // for a derived type is generated, we'll add it to the discriminator mapping.
        _ = _abstractResourceDataSchemaGenerator.Get(schemaRepository);

        Type resourceDataConstructedType = GetInnerTypeOfDataProperty(documentType);

        // Ensure all reachable related resource types are available in the discriminator mapping so includes work.
        // Doing this matters when not all endpoints are exposed.
        EnsureResourceTypesAreMappedInDiscriminator(resourceDataConstructedType, schemaRepository);

        OpenApiSchema referenceSchemaForResourceData = _resourceDataSchemaGenerator.GenerateSchema(resourceDataConstructedType, schemaRepository);
        _abstractResourceDataSchemaGenerator.MapDiscriminator(resourceDataConstructedType, referenceSchemaForResourceData, schemaRepository);

        OpenApiSchema referenceSchemaForDocument = _defaultSchemaGenerator.GenerateSchema(documentType, schemaRepository);
        OpenApiSchema fullSchemaForDocument = schemaRepository.Schemas[referenceSchemaForDocument.Reference.Id];

        if (JsonApiSchemaFacts.HasNullableDataProperty(documentType))
        {
            SetDataSchemaToNullable(fullSchemaForDocument);
        }

        return referenceSchemaForDocument;
    }

    private static Type GetInnerTypeOfDataProperty(Type documentType)
    {
        PropertyInfo? dataProperty = documentType.GetProperty("Data");

        if (dataProperty == null)
        {
            throw new UnreachableCodeException();
        }

        return dataProperty.PropertyType.ConstructedToOpenType().IsAssignableTo(typeof(ICollection<>))
            ? dataProperty.PropertyType.GenericTypeArguments[0]
            : dataProperty.PropertyType;
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

    private static void SetDataSchemaToNullable(OpenApiSchema fullSchemaForDocument)
    {
        OpenApiSchema referenceSchemaForData = fullSchemaForDocument.Properties[JsonApiPropertyName.Data];
        referenceSchemaForData.Nullable = true;
        fullSchemaForDocument.Properties[JsonApiPropertyName.Data] = referenceSchemaForData;
    }

    private void SetJsonApiVersion(OpenApiSchema fullSchemaForDocument)
    {
        if (fullSchemaForDocument.Properties.ContainsKey(JsonApiPropertyName.Jsonapi) && !_options.IncludeJsonApiVersion)
        {
            fullSchemaForDocument.Properties.Remove(JsonApiPropertyName.Jsonapi);
        }
    }
}
