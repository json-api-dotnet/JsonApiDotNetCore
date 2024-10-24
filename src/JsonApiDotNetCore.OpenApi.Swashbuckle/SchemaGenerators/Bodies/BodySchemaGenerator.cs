using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Bodies;

/// <summary>
/// Generates the OpenAPI component schema for a request and/or response body.
/// </summary>
internal abstract class BodySchemaGenerator
{
    private readonly MetaSchemaGenerator _metaSchemaGenerator;
    private readonly LinksVisibilitySchemaGenerator _linksVisibilitySchemaGenerator;
    private readonly IJsonApiOptions _options;

    protected BodySchemaGenerator(MetaSchemaGenerator metaSchemaGenerator, LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator,
        IJsonApiOptions options)
    {
        ArgumentGuard.NotNull(metaSchemaGenerator);
        ArgumentGuard.NotNull(linksVisibilitySchemaGenerator);
        ArgumentGuard.NotNull(options);

        _metaSchemaGenerator = metaSchemaGenerator;
        _linksVisibilitySchemaGenerator = linksVisibilitySchemaGenerator;
        _options = options;
    }

    public abstract bool CanGenerate(Type modelType);

    public OpenApiSchema GenerateSchema(Type bodyType, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(bodyType);
        ArgumentGuard.NotNull(schemaRepository);

        if (schemaRepository.TryLookupByType(bodyType, out OpenApiSchema? referenceSchema))
        {
            return referenceSchema;
        }

        _metaSchemaGenerator.GenerateSchema(schemaRepository);

        referenceSchema = GenerateBodySchema(bodyType, schemaRepository);
        OpenApiSchema fullSchema = schemaRepository.Schemas[referenceSchema.Reference.Id];

        _linksVisibilitySchemaGenerator.UpdateSchemaForTopLevel(bodyType, fullSchema, schemaRepository);

        SetJsonApiVersion(fullSchema);

        return referenceSchema;
    }

    protected abstract OpenApiSchema GenerateBodySchema(Type bodyType, SchemaRepository schemaRepository);

    private void SetJsonApiVersion(OpenApiSchema fullSchema)
    {
        if (fullSchema.Properties.ContainsKey(JsonApiPropertyName.Jsonapi) && !_options.IncludeJsonApiVersion)
        {
            fullSchema.Properties.Remove(JsonApiPropertyName.Jsonapi);
        }
    }

    protected static void PostProcessForResourceInheritance(IResourceGraph resourceGraph, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(schemaRepository);

        // TODO: Make this unconditional (changes all existing swagger.json files)
        if (resourceGraph.HasResourceInheritance)
        {
            foreach (ResourceType resourceType in resourceGraph.GetResourceTypes().Where(resourceType => resourceType.BaseType != null))
            {
                Type dataInResponseSchemaType = typeof(ResourceDataInResponse<>).MakeGenericType(resourceType.ClrType);
                ClearDataPropertiesInDerivedType(dataInResponseSchemaType, schemaRepository);

                Type dataInCreateRequestSchemaType = typeof(DataInCreateResourceRequest<>).MakeGenericType(resourceType.ClrType);
                ClearDataPropertiesInDerivedType(dataInCreateRequestSchemaType, schemaRepository);

                Type dataInUpdateRequestSchemaType = typeof(DataInUpdateResourceRequest<>).MakeGenericType(resourceType.ClrType);
                ClearDataPropertiesInDerivedType(dataInUpdateRequestSchemaType, schemaRepository);
            }
        }
    }

    private static void ClearDataPropertiesInDerivedType(Type dataSchemaType, SchemaRepository schemaRepository)
    {
        if (schemaRepository.TryLookupByType(dataSchemaType, out OpenApiSchema? referenceSchemaForData))
        {
            OpenApiSchema fullSchemaForData = schemaRepository.Schemas[referenceSchemaForData.Reference.Id];
            OpenApiSchema fullSchemaForDerivedType = fullSchemaForData.UnwrapLastExtendedSchema();

            // TODO: This shouldn't be needed. Instead, a derived object should be generated that only references base, without adding its own overriding properties.
            // This still needs to be an allOf[2] though, otherwise NSwag eliminates the derived type entirely.
            fullSchemaForDerivedType.Properties.Remove(JsonApiPropertyName.Type);
            fullSchemaForDerivedType.Properties.Remove(JsonApiPropertyName.Id);
            fullSchemaForDerivedType.Properties.Remove(JsonApiPropertyName.Links);
            fullSchemaForDerivedType.Properties.Remove(JsonApiPropertyName.Meta);
            fullSchemaForDerivedType.Properties.Remove(JsonApiPropertyName.Attributes);
            fullSchemaForDerivedType.Properties.Remove(JsonApiPropertyName.Relationships);
            fullSchemaForDerivedType.Required.Clear();
        }
    }

    private static void ReplaceDeclaredType(OpenApiSchema fullSchemaForDataInResponse, Type fieldSchemaOpenType, string propertyName,
        ResourceType ultimateBaseType, SchemaRepository schemaRepository)
    {
        Type ultimateBaseSchemaType = fieldSchemaOpenType.MakeGenericType(ultimateBaseType.ClrType);

        if (schemaRepository.TryLookupByType(ultimateBaseSchemaType, out OpenApiSchema? referenceSchemaForUltimateBaseAttributes))
        {
            IDictionary<string, OpenApiSchema> propertiesSchema = fullSchemaForDataInResponse.UnwrapLastExtendedSchema().Properties;

            if (propertiesSchema.ContainsKey(propertyName))
            {
                propertiesSchema[propertyName] = referenceSchemaForUltimateBaseAttributes.WrapInExtendedSchema();
            }
        }
    }
}
