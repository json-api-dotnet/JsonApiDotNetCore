using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class ResourceObjectSchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly IResourceGraph _resourceGraph;
    private readonly IJsonApiOptions _options;
    private readonly ISchemaRepositoryAccessor _schemaRepositoryAccessor;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly Func<ResourceTypeInfo, ResourceFieldObjectSchemaBuilder> _resourceFieldObjectSchemaBuilderFactory;

    public ResourceObjectSchemaGenerator(SchemaGenerator defaultSchemaGenerator, IResourceGraph resourceGraph, IJsonApiOptions options,
        ISchemaRepositoryAccessor schemaRepositoryAccessor)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(schemaRepositoryAccessor);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _resourceGraph = resourceGraph;
        _options = options;
        _schemaRepositoryAccessor = schemaRepositoryAccessor;

        _resourceTypeSchemaGenerator = new ResourceTypeSchemaGenerator(schemaRepositoryAccessor, resourceGraph, options.SerializerOptions.PropertyNamingPolicy);

        _resourceFieldObjectSchemaBuilderFactory = resourceTypeInfo => new ResourceFieldObjectSchemaBuilder(resourceTypeInfo, schemaRepositoryAccessor,
            defaultSchemaGenerator, _resourceTypeSchemaGenerator, options.SerializerOptions.PropertyNamingPolicy);
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

        ReorderMembers(fullSchemaForResourceObject, new[]
        {
            JsonApiObjectPropertyName.Type,
            JsonApiObjectPropertyName.Id,
            JsonApiObjectPropertyName.AttributesObject,
            JsonApiObjectPropertyName.RelationshipsObject,
            JsonApiObjectPropertyName.LinksObject,
            JsonApiObjectPropertyName.MetaObject
        });

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
                fullSchemaForResourceObject.Required.Remove(JsonApiObjectPropertyName.Id);
                fullSchemaForResourceObject.Properties.Remove(JsonApiObjectPropertyName.Id);
            }
            else if (clientIdGeneration == ClientIdGenerationMode.Allowed)
            {
                fullSchemaForResourceObject.Required.Remove(JsonApiObjectPropertyName.Id);
            }
        }
    }

    private void SetResourceType(OpenApiSchema fullSchemaForResourceObject, Type resourceType)
    {
        fullSchemaForResourceObject.Properties[JsonApiObjectPropertyName.Type] = _resourceTypeSchemaGenerator.Get(resourceType);
    }

    private void SetResourceAttributes(OpenApiSchema fullSchemaForResourceObject, ResourceFieldObjectSchemaBuilder builder)
    {
        OpenApiSchema referenceSchemaForAttributesObject = fullSchemaForResourceObject.Properties[JsonApiObjectPropertyName.AttributesObject];
        OpenApiSchema fullSchemaForAttributesObject = _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForAttributesObject.Reference.Id];

        builder.SetMembersOfAttributesObject(fullSchemaForAttributesObject);

        if (!fullSchemaForAttributesObject.Properties.Any())
        {
            fullSchemaForResourceObject.Properties.Remove(JsonApiObjectPropertyName.AttributesObject);
            _schemaRepositoryAccessor.Current.Schemas.Remove(referenceSchemaForAttributesObject.Reference.Id);
        }
        else
        {
            fullSchemaForAttributesObject.AdditionalPropertiesAllowed = false;
        }
    }

    private void SetResourceRelationships(OpenApiSchema fullSchemaForResourceObject, ResourceFieldObjectSchemaBuilder builder)
    {
        OpenApiSchema referenceSchemaForRelationshipsObject = fullSchemaForResourceObject.Properties[JsonApiObjectPropertyName.RelationshipsObject];
        OpenApiSchema fullSchemaForRelationshipsObject = _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForRelationshipsObject.Reference.Id];

        builder.SetMembersOfRelationshipsObject(fullSchemaForRelationshipsObject);

        if (!fullSchemaForRelationshipsObject.Properties.Any())
        {
            fullSchemaForResourceObject.Properties.Remove(JsonApiObjectPropertyName.RelationshipsObject);
            _schemaRepositoryAccessor.Current.Schemas.Remove(referenceSchemaForRelationshipsObject.Reference.Id);
        }
        else
        {
            fullSchemaForRelationshipsObject.AdditionalPropertiesAllowed = false;
        }
    }

    private static void ReorderMembers(OpenApiSchema fullSchemaForResourceObject, IEnumerable<string> orderedMembers)
    {
        var reorderedMembers = new Dictionary<string, OpenApiSchema>();

        foreach (string member in orderedMembers)
        {
            if (fullSchemaForResourceObject.Properties.TryGetValue(member, out OpenApiSchema? schema))
            {
                reorderedMembers[member] = schema;
            }
        }

        fullSchemaForResourceObject.Properties = reorderedMembers;
    }
}
