using System;
using System.Collections.Generic;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Configuration;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents
{
    internal sealed class ResourceObjectSchemaGenerator
    {
        private readonly SchemaGenerator _defaultSchemaGenerator;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly ISchemaRepositoryAccessor _schemaRepositoryAccessor;
        private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
        private readonly bool _allowClientGeneratedIds;
        private readonly Func<ResourceTypeInfo, ResourceFieldObjectSchemaBuilder> _createFieldObjectBuilderFactory;

        public ResourceObjectSchemaGenerator(SchemaGenerator defaultSchemaGenerator, IResourceContextProvider resourceContextProvider,
            IJsonApiOptions jsonApiOptions, ISchemaRepositoryAccessor schemaRepositoryAccessor)
        {
            ArgumentGuard.NotNull(defaultSchemaGenerator, nameof(defaultSchemaGenerator));
            ArgumentGuard.NotNull(resourceContextProvider, nameof(resourceContextProvider));
            ArgumentGuard.NotNull(jsonApiOptions, nameof(jsonApiOptions));
            ArgumentGuard.NotNull(schemaRepositoryAccessor, nameof(schemaRepositoryAccessor));

            _defaultSchemaGenerator = defaultSchemaGenerator;
            _resourceContextProvider = resourceContextProvider;
            _schemaRepositoryAccessor = schemaRepositoryAccessor;

            _resourceTypeSchemaGenerator = new ResourceTypeSchemaGenerator(schemaRepositoryAccessor, resourceContextProvider);
            _allowClientGeneratedIds = jsonApiOptions.AllowClientGeneratedIds;

            _createFieldObjectBuilderFactory = CreateFieldObjectBuilderFactory(defaultSchemaGenerator, resourceContextProvider, jsonApiOptions,
                schemaRepositoryAccessor, _resourceTypeSchemaGenerator);
        }

        private static Func<ResourceTypeInfo, ResourceFieldObjectSchemaBuilder> CreateFieldObjectBuilderFactory(SchemaGenerator defaultSchemaGenerator,
            IResourceContextProvider resourceContextProvider, IJsonApiOptions jsonApiOptions, ISchemaRepositoryAccessor schemaRepositoryAccessor,
            ResourceTypeSchemaGenerator resourceTypeSchemaGenerator)
        {
            NamingStrategy namingStrategy = ((DefaultContractResolver)jsonApiOptions.SerializerSettings.ContractResolver)!.NamingStrategy;
            ResourceNameFormatterProxy resourceNameFormatterProxy = new(namingStrategy);
            var jsonApiSchemaIdSelector = new JsonApiSchemaIdSelector(resourceNameFormatterProxy, resourceContextProvider);

            return resourceTypeInfo => new ResourceFieldObjectSchemaBuilder(resourceTypeInfo, schemaRepositoryAccessor, defaultSchemaGenerator,
                jsonApiSchemaIdSelector, resourceTypeSchemaGenerator);
        }

        public OpenApiSchema GenerateSchema(Type resourceObjectType)
        {
            ArgumentGuard.NotNull(resourceObjectType, nameof(resourceObjectType));

            (OpenApiSchema fullSchemaForResourceObject, OpenApiSchema referenceSchemaForResourceObject) = EnsureSchemasExist(resourceObjectType);

            var resourceTypeInfo = ResourceTypeInfo.Create(resourceObjectType, _resourceContextProvider);
            ResourceFieldObjectSchemaBuilder fieldObjectBuilder = _createFieldObjectBuilderFactory(resourceTypeInfo);

            RemoveResourceIdIfPostResourceObject(resourceTypeInfo.ResourceObjectOpenType, fullSchemaForResourceObject);

            SetResourceType(fullSchemaForResourceObject, resourceTypeInfo.ResourceType);

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
            if (resourceObjectOpenType == typeof(ResourcePostRequestObject<>) && !_allowClientGeneratedIds)
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
            OpenApiSchema fullSchemaForAttributesObject = builder.BuildAttributesObject(fullSchemaForResourceObject);

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
            OpenApiSchema fullSchemaForRelationshipsObject = builder.BuildRelationshipsObject(fullSchemaForResourceObject);

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
