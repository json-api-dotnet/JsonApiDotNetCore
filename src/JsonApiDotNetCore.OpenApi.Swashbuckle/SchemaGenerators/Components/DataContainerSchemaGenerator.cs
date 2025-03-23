using System.Diagnostics;
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
    private readonly DataSchemaGenerator _dataSchemaGenerator;
    private readonly IResourceGraph _resourceGraph;

    public DataContainerSchemaGenerator(DataSchemaGenerator dataSchemaGenerator, IResourceGraph resourceGraph)
    {
        ArgumentNullException.ThrowIfNull(dataSchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceGraph);

        _dataSchemaGenerator = dataSchemaGenerator;
        _resourceGraph = resourceGraph;
    }

    public OpenApiSchema GenerateSchemaForCommonResourceDataInResponse(SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaRepository);

        return _dataSchemaGenerator.GenerateSchemaForCommonData(typeof(ResourceInResponse), schemaRepository);
    }

    public OpenApiSchema GenerateSchema(Type dataContainerSchemaType, ResourceType resourceType, bool forRequestSchema, bool canIncludeRelated,
        SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(dataContainerSchemaType);
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByType(dataContainerSchemaType, out OpenApiSchema referenceSchemaForData))
        {
            return referenceSchemaForData;
        }

        Type dataConstructedType = GetElementTypeOfDataProperty(dataContainerSchemaType, resourceType);

        if (canIncludeRelated)
        {
            var resourceSchemaType = ResourceSchemaType.Create(dataConstructedType, _resourceGraph);

            if (resourceSchemaType.SchemaOpenType == typeof(DataInResponse<>))
            {
                // Ensure all reachable related resource types in response schemas are generated upfront.
                // This is needed to make includes work when not all endpoints are exposed.
                GenerateReachableRelatedTypesInResponse(dataConstructedType, schemaRepository);
            }
        }

        return _dataSchemaGenerator.GenerateSchema(dataConstructedType, forRequestSchema, schemaRepository);
    }

    private static Type GetElementTypeOfDataProperty(Type dataContainerConstructedType, ResourceType resourceType)
    {
        PropertyInfo? dataProperty = dataContainerConstructedType.GetProperty("Data");

        if (dataProperty == null)
        {
            throw new UnreachableException();
        }

        Type innerPropertyType = dataProperty.PropertyType.ConstructedToOpenType().IsAssignableTo(typeof(ICollection<>))
            ? dataProperty.PropertyType.GenericTypeArguments[0]
            : dataProperty.PropertyType;

        if (innerPropertyType == typeof(ResourceInResponse))
        {
            return typeof(DataInResponse<>).MakeGenericType(resourceType.ClrType);
        }

        if (!innerPropertyType.IsGenericType)
        {
            throw new UnreachableException();
        }

        return innerPropertyType;
    }

    private void GenerateReachableRelatedTypesInResponse(Type dataConstructedType, SchemaRepository schemaRepository)
    {
        Type dataOpenType = dataConstructedType.GetGenericTypeDefinition();

        if (dataOpenType == typeof(DataInResponse<>))
        {
            var resourceSchemaType = ResourceSchemaType.Create(dataConstructedType, _resourceGraph);

            foreach (ResourceType relatedType in IncludeDependencyScanner.Instance.GetReachableRelatedTypes(resourceSchemaType.ResourceType))
            {
                Type resourceDataConstructedType = typeof(DataInResponse<>).MakeGenericType(relatedType.ClrType);
                _ = _dataSchemaGenerator.GenerateSchema(resourceDataConstructedType, false, schemaRepository);
            }
        }
    }
}
