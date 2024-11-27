using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

/// <summary>
/// Generates the reference schema for the Data property in a request or response schema, taking schema inheritance into account.
/// </summary>
internal sealed class DataContainerSchemaGenerator
{
    private readonly AbstractResourceDataSchemaGenerator _abstractResourceDataSchemaGenerator;
    private readonly DataSchemaGenerator _dataSchemaGenerator;
    private readonly IncludeDependencyScanner _includeDependencyScanner;
    private readonly IResourceGraph _resourceGraph;

    public DataContainerSchemaGenerator(AbstractResourceDataSchemaGenerator abstractResourceDataSchemaGenerator, DataSchemaGenerator dataSchemaGenerator,
        IncludeDependencyScanner includeDependencyScanner, IResourceGraph resourceGraph)
    {
        ArgumentGuard.NotNull(abstractResourceDataSchemaGenerator);
        ArgumentGuard.NotNull(dataSchemaGenerator);
        ArgumentGuard.NotNull(includeDependencyScanner);
        ArgumentGuard.NotNull(resourceGraph);

        _abstractResourceDataSchemaGenerator = abstractResourceDataSchemaGenerator;
        _dataSchemaGenerator = dataSchemaGenerator;
        _includeDependencyScanner = includeDependencyScanner;
        _resourceGraph = resourceGraph;
    }

    public OpenApiSchema GenerateSchema(Type dataContainerConstructedType, ResourceType resourceType, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(dataContainerConstructedType);
        ArgumentGuard.NotNull(resourceType);
        ArgumentGuard.NotNull(schemaRepository);

        if (schemaRepository.TryLookupByType(dataContainerConstructedType, out OpenApiSchema referenceSchemaForData))
        {
            return referenceSchemaForData;
        }

        if (!forRequestSchema)
        {
            // There's no way to intercept in the Swashbuckle recursive component schema generation when using schema inheritance, which we need
            // to perform generic type expansions. As a workaround, we generate an empty base schema upfront. And each time the schema
            // for a derived type is generated, we'll add it to the discriminator mapping.
            _ = _abstractResourceDataSchemaGenerator.GenerateSchema(schemaRepository);
        }

        Type dataConstructedType = GetElementTypeOfDataProperty(dataContainerConstructedType, resourceType);

        if (!forRequestSchema)
        {
            // Ensure all reachable related resource types are available in the discriminator mapping upfront.
            // This is needed to make includes work when not all endpoints are exposed.
            EnsureResourceDataInResponseDerivedTypesAreMappedInDiscriminator(dataConstructedType, schemaRepository);
        }

        referenceSchemaForData = _dataSchemaGenerator.GenerateSchema(dataConstructedType, schemaRepository);

        if (!forRequestSchema)
        {
            _abstractResourceDataSchemaGenerator.MapDiscriminator(dataConstructedType, referenceSchemaForData, schemaRepository);
        }

        return referenceSchemaForData;
    }

    private static Type GetElementTypeOfDataProperty(Type dataContainerConstructedType, ResourceType resourceType)
    {
        PropertyInfo? dataProperty = dataContainerConstructedType.GetProperty("Data");

        if (dataProperty == null)
        {
            throw new UnreachableCodeException();
        }

        Type innerPropertyType = dataProperty.PropertyType.ConstructedToOpenType().IsAssignableTo(typeof(ICollection<>))
            ? dataProperty.PropertyType.GenericTypeArguments[0]
            : dataProperty.PropertyType;

        if (innerPropertyType == typeof(ResourceData))
        {
            return typeof(ResourceDataInResponse<>).MakeGenericType(resourceType.ClrType);
        }

        if (!innerPropertyType.IsGenericType)
        {
            throw new UnreachableCodeException();
        }

        return innerPropertyType;
    }

    private void EnsureResourceDataInResponseDerivedTypesAreMappedInDiscriminator(Type dataConstructedType, SchemaRepository schemaRepository)
    {
        Type dataOpenType = dataConstructedType.GetGenericTypeDefinition();

        if (dataOpenType == typeof(ResourceDataInResponse<>))
        {
            var resourceTypeInfo = ResourceTypeInfo.Create(dataConstructedType, _resourceGraph);

            foreach (ResourceType relatedType in _includeDependencyScanner.GetReachableRelatedTypes(resourceTypeInfo.ResourceType))
            {
                MapResourceDataInResponseDerivedTypeInDiscriminator(relatedType, schemaRepository);
            }
        }
    }

    private void MapResourceDataInResponseDerivedTypeInDiscriminator(ResourceType resourceType, SchemaRepository schemaRepository)
    {
        Type resourceDataConstructedType = typeof(ResourceDataInResponse<>).MakeGenericType(resourceType.ClrType);
        OpenApiSchema referenceSchemaForResourceData = _dataSchemaGenerator.GenerateSchema(resourceDataConstructedType, schemaRepository);

        _abstractResourceDataSchemaGenerator.MapDiscriminator(resourceDataConstructedType, referenceSchemaForResourceData, schemaRepository);
    }
}
