using System;
using System.Collections.Generic;
using System.Text.Json;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents
{
    internal sealed class ResourceObjectSchemaGenerator
    {
        private readonly SchemaGenerator _defaultSchemaGenerator;
        private readonly IResourceGraph _resourceGraph;
        private readonly ISchemaRepositoryAccessor _schemaRepositoryAccessor;
        private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
        private readonly bool _allowClientGeneratedIds;
        private readonly Func<ResourceTypeInfo, ResourceFieldObjectSchemaBuilder> _createFieldObjectBuilderFactory;

        public ResourceObjectSchemaGenerator(SchemaGenerator defaultSchemaGenerator, IResourceGraph resourceGraph, IJsonApiOptions options,
            ISchemaRepositoryAccessor schemaRepositoryAccessor)
        {
            ArgumentGuard.NotNull(defaultSchemaGenerator, nameof(defaultSchemaGenerator));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(options, nameof(options));
            ArgumentGuard.NotNull(schemaRepositoryAccessor, nameof(schemaRepositoryAccessor));

            _defaultSchemaGenerator = defaultSchemaGenerator;
            _resourceGraph = resourceGraph;
            _schemaRepositoryAccessor = schemaRepositoryAccessor;

            _resourceTypeSchemaGenerator = new ResourceTypeSchemaGenerator(schemaRepositoryAccessor, resourceGraph);
            _allowClientGeneratedIds = options.AllowClientGeneratedIds;

            _createFieldObjectBuilderFactory = CreateFieldObjectBuilderFactory(defaultSchemaGenerator, resourceGraph, options, schemaRepositoryAccessor,
                _resourceTypeSchemaGenerator);
        }

        private static Func<ResourceTypeInfo, ResourceFieldObjectSchemaBuilder> CreateFieldObjectBuilderFactory(SchemaGenerator defaultSchemaGenerator,
            IResourceGraph resourceGraph, IJsonApiOptions options, ISchemaRepositoryAccessor schemaRepositoryAccessor,
            ResourceTypeSchemaGenerator resourceTypeSchemaGenerator)
        {
            JsonNamingPolicy? namingPolicy = options.SerializerOptions.PropertyNamingPolicy;
            ResourceNameFormatter resourceNameFormatter = new(namingPolicy);
            var jsonApiSchemaIdSelector = new JsonApiSchemaIdSelector(resourceNameFormatter, resourceGraph);

            return resourceTypeInfo => new ResourceFieldObjectSchemaBuilder(resourceTypeInfo, schemaRepositoryAccessor, defaultSchemaGenerator,
                jsonApiSchemaIdSelector, resourceTypeSchemaGenerator);
        }

        public OpenApiSchema GenerateSchema(Type resourceObjectType)
        {
            ArgumentGuard.NotNull(resourceObjectType, nameof(resourceObjectType));

            (OpenApiSchema fullSchemaForResourceObject, OpenApiSchema referenceSchemaForResourceObject) = EnsureSchemasExist(resourceObjectType);

            var resourceTypeInfo = ResourceTypeInfo.Create(resourceObjectType, _resourceGraph);
            ResourceFieldObjectSchemaBuilder fieldObjectBuilder = _createFieldObjectBuilderFactory(resourceTypeInfo);

            RemoveResourceIdIfPostResourceObject(resourceTypeInfo.ResourceObjectOpenType, fullSchemaForResourceObject);

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

        private void RemoveResourceIdIfPostResourceObject(Type resourceObjectOpenType, OpenApiSchema fullSchemaForResourceObject)
        {
            if (resourceObjectOpenType == typeof(ResourceObjectInPostRequest<>) && !_allowClientGeneratedIds)
            {
                fullSchemaForResourceObject.Required.Remove(JsonApiObjectPropertyName.Id);
                fullSchemaForResourceObject.Properties.Remove(JsonApiObjectPropertyName.Id);
            }
        }

        private void SetResourceType(OpenApiSchema fullSchemaForResourceObject, Type resourceType)
        {
            fullSchemaForResourceObject.Properties[JsonApiObjectPropertyName.Type] = _resourceTypeSchemaGenerator.Get(resourceType);
        }

        private static void SetResourceAttributes(OpenApiSchema fullSchemaForResourceObject, ResourceFieldObjectSchemaBuilder builder)
        {
            OpenApiSchema? fullSchemaForAttributesObject = builder.BuildAttributesObject(fullSchemaForResourceObject);

            if (fullSchemaForAttributesObject != null)
            {
                fullSchemaForResourceObject.Properties[JsonApiObjectPropertyName.AttributesObject] = fullSchemaForAttributesObject;
            }
            else
            {
                fullSchemaForResourceObject.Properties.Remove(JsonApiObjectPropertyName.AttributesObject);
            }
        }

        private static void SetResourceRelationships(OpenApiSchema fullSchemaForResourceObject, ResourceFieldObjectSchemaBuilder builder)
        {
            OpenApiSchema? fullSchemaForRelationshipsObject = builder.BuildRelationshipsObject(fullSchemaForResourceObject);

            if (fullSchemaForRelationshipsObject != null)
            {
                fullSchemaForResourceObject.Properties[JsonApiObjectPropertyName.RelationshipsObject] = fullSchemaForRelationshipsObject;
            }
            else
            {
                fullSchemaForResourceObject.Properties.Remove(JsonApiObjectPropertyName.RelationshipsObject);
            }
        }

        private static void ReorderMembers(OpenApiSchema fullSchemaForResourceObject, IEnumerable<string> orderedMembers)
        {
            var reorderedMembers = new Dictionary<string, OpenApiSchema>();

            foreach (string member in orderedMembers)
            {
                if (fullSchemaForResourceObject.Properties.ContainsKey(member))
                {
                    reorderedMembers[member] = fullSchemaForResourceObject.Properties[member];
                }
            }

            fullSchemaForResourceObject.Properties = reorderedMembers;
        }
    }
}
