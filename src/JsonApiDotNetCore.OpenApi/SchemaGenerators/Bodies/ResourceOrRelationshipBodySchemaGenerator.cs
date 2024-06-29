using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.SchemaGenerators.Components;
using JsonApiDotNetCore.OpenApi.SwaggerComponents;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SchemaGenerators.Bodies;

/// <summary>
/// Generates the OpenAPI component schema for a resource/relationship request/response body.
/// </summary>
internal sealed class ResourceOrRelationshipBodySchemaGenerator : BodySchemaGenerator
{
    private static readonly Type[] RequestSchemaTypes =
    [
        typeof(CreateResourceRequestDocument<>),
        typeof(UpdateResourceRequestDocument<>),
        typeof(ToOneRelationshipInRequest<>),
        typeof(NullableToOneRelationshipInRequest<>),
        typeof(ToManyRelationshipInRequest<>)
    ];

    private static readonly Type[] ResponseSchemaTypes =
    [
        typeof(ResourceCollectionResponseDocument<>),
        typeof(PrimaryResourceResponseDocument<>),
        typeof(SecondaryResourceResponseDocument<>),
        typeof(NullableSecondaryResourceResponseDocument<>),
        typeof(ResourceIdentifierResponseDocument<>),
        typeof(NullableResourceIdentifierResponseDocument<>),
        typeof(ResourceIdentifierCollectionResponseDocument<>)
    ];

    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly AbstractResourceDataSchemaGenerator _abstractResourceDataSchemaGenerator;
    private readonly DataSchemaGenerator _dataSchemaGenerator;
    private readonly IncludeDependencyScanner _includeDependencyScanner;
    private readonly IResourceGraph _resourceGraph;

    public ResourceOrRelationshipBodySchemaGenerator(SchemaGenerator defaultSchemaGenerator,
        AbstractResourceDataSchemaGenerator abstractResourceDataSchemaGenerator, DataSchemaGenerator dataSchemaGenerator,
        MetaSchemaGenerator metaSchemaGenerator, LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator,
        IncludeDependencyScanner includeDependencyScanner, IResourceGraph resourceGraph, IJsonApiOptions options)
        : base(metaSchemaGenerator, linksVisibilitySchemaGenerator, options)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(abstractResourceDataSchemaGenerator);
        ArgumentGuard.NotNull(dataSchemaGenerator);
        ArgumentGuard.NotNull(includeDependencyScanner);
        ArgumentGuard.NotNull(resourceGraph);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _abstractResourceDataSchemaGenerator = abstractResourceDataSchemaGenerator;
        _dataSchemaGenerator = dataSchemaGenerator;
        _includeDependencyScanner = includeDependencyScanner;
        _resourceGraph = resourceGraph;
    }

    public override bool CanGenerate(Type modelType)
    {
        Type modelOpenType = modelType.ConstructedToOpenType();
        return RequestSchemaTypes.Contains(modelOpenType) || ResponseSchemaTypes.Contains(modelOpenType);
    }

    protected override OpenApiSchema GenerateBodySchema(Type bodyType, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(bodyType);
        ArgumentGuard.NotNull(schemaRepository);

        if (schemaRepository.TryLookupByType(bodyType, out OpenApiSchema? referenceSchemaForBody))
        {
            return referenceSchemaForBody;
        }

        bool isRequestSchema = RequestSchemaTypes.Contains(bodyType.ConstructedToOpenType());

        if (!isRequestSchema)
        {
            // There's no way to intercept in the Swashbuckle recursive component schema generation when using inheritance, which we need
            // to perform generic type expansions. As a workaround, we generate an empty base schema upfront. Each time the schema
            // for a derived type is generated, we'll add it to the discriminator mapping.
            _ = _abstractResourceDataSchemaGenerator.GenerateSchema(schemaRepository);
        }

        Type dataConstructedType = GetInnerTypeOfDataProperty(bodyType);

        if (!isRequestSchema)
        {
            // Ensure all reachable related resource types are available in the discriminator mapping so includes work.
            // Doing this matters when not all endpoints are exposed.
            EnsureResourceTypesAreMappedInDiscriminator(dataConstructedType, schemaRepository);
        }

        OpenApiSchema referenceSchemaForData = _dataSchemaGenerator.GenerateSchema(dataConstructedType, schemaRepository);

        if (!isRequestSchema)
        {
            _abstractResourceDataSchemaGenerator.MapDiscriminator(dataConstructedType, referenceSchemaForData, schemaRepository);
        }

        referenceSchemaForBody = _defaultSchemaGenerator.GenerateSchema(bodyType, schemaRepository);
        OpenApiSchema fullSchemaForBody = schemaRepository.Schemas[referenceSchemaForBody.Reference.Id].UnwrapLastExtendedSchema();

        if (JsonApiSchemaFacts.HasNullableDataProperty(bodyType))
        {
            fullSchemaForBody.Properties[JsonApiPropertyName.Data].Nullable = true;
        }

        return referenceSchemaForBody;
    }

    private static Type GetInnerTypeOfDataProperty(Type bodyType)
    {
        PropertyInfo? dataProperty = bodyType.GetProperty("Data");

        if (dataProperty == null)
        {
            throw new UnreachableCodeException();
        }

        return dataProperty.PropertyType.ConstructedToOpenType().IsAssignableTo(typeof(ICollection<>))
            ? dataProperty.PropertyType.GenericTypeArguments[0]
            : dataProperty.PropertyType;
    }

    private void EnsureResourceTypesAreMappedInDiscriminator(Type dataConstructedType, SchemaRepository schemaRepository)
    {
        Type dataOpenType = dataConstructedType.GetGenericTypeDefinition();

        if (dataOpenType == typeof(ResourceDataInResponse<>))
        {
            var resourceTypeInfo = ResourceTypeInfo.Create(dataConstructedType, _resourceGraph);

            foreach (ResourceType resourceType in _includeDependencyScanner.GetReachableRelatedTypes(resourceTypeInfo.ResourceType))
            {
                EnsureResourceTypeIsMappedInDiscriminator(schemaRepository, resourceType);
            }
        }
    }

    private void EnsureResourceTypeIsMappedInDiscriminator(SchemaRepository schemaRepository, ResourceType resourceType)
    {
        Type resourceDataConstructedType = typeof(ResourceDataInResponse<>).MakeGenericType(resourceType.ClrType);
        OpenApiSchema referenceSchemaForResourceData = _dataSchemaGenerator.GenerateSchema(resourceDataConstructedType, schemaRepository);

        _abstractResourceDataSchemaGenerator.MapDiscriminator(resourceDataConstructedType, referenceSchemaForResourceData, schemaRepository);
    }
}
