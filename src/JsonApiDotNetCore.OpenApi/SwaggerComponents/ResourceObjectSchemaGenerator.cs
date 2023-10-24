using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class ResourceObjectSchemaGenerator
{
    private static readonly string[] ResourceObjectPropertyNamesInOrder =
    {
        JsonApiPropertyName.Type,
        JsonApiPropertyName.Id,
        JsonApiPropertyName.Attributes,
        JsonApiPropertyName.Relationships,
        JsonApiPropertyName.Links,
        JsonApiPropertyName.Meta
    };

    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly IResourceGraph _resourceGraph;
    private readonly IJsonApiOptions _options;
    private readonly ISchemaRepositoryAccessor _schemaRepositoryAccessor;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly Func<ResourceTypeInfo, ResourceFieldObjectSchemaBuilder> _resourceFieldObjectSchemaBuilderFactory;

    public ResourceObjectSchemaGenerator(SchemaGenerator defaultSchemaGenerator, IResourceGraph resourceGraph, IJsonApiOptions options,
        ISchemaRepositoryAccessor schemaRepositoryAccessor, ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(schemaRepositoryAccessor);
        ArgumentGuard.NotNull(resourceFieldValidationMetadataProvider);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _resourceGraph = resourceGraph;
        _options = options;
        _schemaRepositoryAccessor = schemaRepositoryAccessor;

        _resourceTypeSchemaGenerator = new ResourceTypeSchemaGenerator(schemaRepositoryAccessor, resourceGraph, options.SerializerOptions.PropertyNamingPolicy);

        _resourceFieldObjectSchemaBuilderFactory = resourceTypeInfo => new ResourceFieldObjectSchemaBuilder(resourceTypeInfo, schemaRepositoryAccessor,
            defaultSchemaGenerator, _resourceTypeSchemaGenerator, options.SerializerOptions.PropertyNamingPolicy, resourceFieldValidationMetadataProvider);
    }

    public OpenApiSchema GenerateSchema(Type resourceObjectType)
    {
        ArgumentGuard.NotNull(resourceObjectType);

        (OpenApiSchema fullSchemaForResourceObject, OpenApiSchema referenceSchemaForResourceObject) = EnsureSchemasExist(resourceObjectType);

        var resourceTypeInfo = ResourceTypeInfo.Create(resourceObjectType, _resourceGraph);
        ResourceFieldObjectSchemaBuilder fieldObjectBuilder = _resourceFieldObjectSchemaBuilderFactory(resourceTypeInfo);

        RemoveResourceIdIfPostResourceObject(resourceTypeInfo, fullSchemaForResourceObject);

        SetResourceType(fullSchemaForResourceObject, resourceTypeInfo.ResourceType.ClrType);

        SetResourceAttributes(fullSchemaForResourceObject, fieldObjectBuilder);

        SetResourceRelationships(fullSchemaForResourceObject, fieldObjectBuilder);

        fullSchemaForResourceObject.ReorderProperties(ResourceObjectPropertyNamesInOrder);

        return referenceSchemaForResourceObject;
    }

    private (OpenApiSchema fullSchema, OpenApiSchema referenceSchema) EnsureSchemasExist(Type resourceObjectType)
    {
        if (!_schemaRepositoryAccessor.Current.TryLookupByType(resourceObjectType, out OpenApiSchema referenceSchema))
        {
            referenceSchema = _defaultSchemaGenerator.GenerateSchema(resourceObjectType, _schemaRepositoryAccessor.Current);
        }

        OpenApiSchema fullSchema = _schemaRepositoryAccessor.Current.Schemas[referenceSchema.Reference.Id];

        return (fullSchema, referenceSchema);
    }

    private void RemoveResourceIdIfPostResourceObject(ResourceTypeInfo resourceTypeInfo, OpenApiSchema fullSchemaForResourceObject)
    {
        if (resourceTypeInfo.ResourceObjectOpenType == typeof(ResourceObjectInPostRequest<>))
        {
            ClientIdGenerationMode clientIdGeneration = resourceTypeInfo.ResourceType.ClientIdGeneration ?? _options.ClientIdGeneration;

            if (clientIdGeneration == ClientIdGenerationMode.Forbidden)
            {
                fullSchemaForResourceObject.Required.Remove(JsonApiPropertyName.Id);
                fullSchemaForResourceObject.Properties.Remove(JsonApiPropertyName.Id);
            }
            else if (clientIdGeneration == ClientIdGenerationMode.Allowed)
            {
                fullSchemaForResourceObject.Required.Remove(JsonApiPropertyName.Id);
            }
        }
    }

    private void SetResourceType(OpenApiSchema fullSchemaForResourceObject, Type resourceType)
    {
        fullSchemaForResourceObject.Properties[JsonApiPropertyName.Type] = _resourceTypeSchemaGenerator.Get(resourceType);
    }

    private void SetResourceAttributes(OpenApiSchema fullSchemaForResourceObject, ResourceFieldObjectSchemaBuilder builder)
    {
        OpenApiSchema referenceSchemaForAttributesObject = fullSchemaForResourceObject.Properties[JsonApiPropertyName.Attributes];
        OpenApiSchema fullSchemaForAttributesObject = _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForAttributesObject.Reference.Id];

        builder.SetMembersOfAttributesObject(fullSchemaForAttributesObject);

        if (!fullSchemaForAttributesObject.Properties.Any())
        {
            fullSchemaForResourceObject.Properties.Remove(JsonApiPropertyName.Attributes);
            _schemaRepositoryAccessor.Current.Schemas.Remove(referenceSchemaForAttributesObject.Reference.Id);
        }
        else
        {
            fullSchemaForAttributesObject.AdditionalPropertiesAllowed = false;
        }
    }

    private void SetResourceRelationships(OpenApiSchema fullSchemaForResourceObject, ResourceFieldObjectSchemaBuilder builder)
    {
        OpenApiSchema referenceSchemaForRelationshipsObject = fullSchemaForResourceObject.Properties[JsonApiPropertyName.Relationships];
        OpenApiSchema fullSchemaForRelationshipsObject = _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForRelationshipsObject.Reference.Id];

        builder.SetMembersOfRelationshipsObject(fullSchemaForRelationshipsObject);

        if (!fullSchemaForRelationshipsObject.Properties.Any())
        {
            fullSchemaForResourceObject.Properties.Remove(JsonApiPropertyName.Relationships);
            _schemaRepositoryAccessor.Current.Schemas.Remove(referenceSchemaForRelationshipsObject.Reference.Id);
        }
        else
        {
            fullSchemaForRelationshipsObject.AdditionalPropertiesAllowed = false;
        }
    }
}
