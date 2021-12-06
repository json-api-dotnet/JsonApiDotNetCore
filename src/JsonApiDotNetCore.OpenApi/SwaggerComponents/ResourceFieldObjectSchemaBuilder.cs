using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using JsonApiDotNetCore.OpenApi.JsonApiObjects;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents
{
    internal sealed class ResourceFieldObjectSchemaBuilder
    {
        private static readonly Type[] RelationshipInResponseOpenTypes =
        {
            typeof(ToOneRelationshipInResponse<>),
            typeof(ToManyRelationshipInResponse<>),
            typeof(NullableToOneRelationshipInResponse<>)
        };

        private readonly ResourceTypeInfo _resourceTypeInfo;
        private readonly ISchemaRepositoryAccessor _schemaRepositoryAccessor;
        private readonly SchemaGenerator _defaultSchemaGenerator;
        private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
        private readonly SchemaRepository _resourceSchemaRepository = new();
        private readonly NullableReferenceSchemaGenerator _nullableReferenceSchemaGenerator;
        private readonly IDictionary<string, OpenApiSchema> _schemasForResourceFields;

        public ResourceFieldObjectSchemaBuilder(ResourceTypeInfo resourceTypeInfo, ISchemaRepositoryAccessor schemaRepositoryAccessor,
            SchemaGenerator defaultSchemaGenerator, ResourceTypeSchemaGenerator resourceTypeSchemaGenerator, JsonNamingPolicy? namingPolicy)
        {
            ArgumentGuard.NotNull(resourceTypeInfo, nameof(resourceTypeInfo));
            ArgumentGuard.NotNull(schemaRepositoryAccessor, nameof(schemaRepositoryAccessor));
            ArgumentGuard.NotNull(defaultSchemaGenerator, nameof(defaultSchemaGenerator));
            ArgumentGuard.NotNull(resourceTypeSchemaGenerator, nameof(resourceTypeSchemaGenerator));

            _resourceTypeInfo = resourceTypeInfo;
            _schemaRepositoryAccessor = schemaRepositoryAccessor;
            _defaultSchemaGenerator = defaultSchemaGenerator;
            _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;

            _nullableReferenceSchemaGenerator = new NullableReferenceSchemaGenerator(schemaRepositoryAccessor, namingPolicy);
            _schemasForResourceFields = GetFieldSchemas();
        }

        private IDictionary<string, OpenApiSchema> GetFieldSchemas()
        {
            if (!_resourceSchemaRepository.TryLookupByType(_resourceTypeInfo.ResourceType.ClrType, out OpenApiSchema referenceSchemaForResource))
            {
                referenceSchemaForResource = _defaultSchemaGenerator.GenerateSchema(_resourceTypeInfo.ResourceType.ClrType, _resourceSchemaRepository);
            }

            OpenApiSchema fullSchemaForResource = _resourceSchemaRepository.Schemas[referenceSchemaForResource.Reference.Id];
            return fullSchemaForResource.Properties;
        }

        public void SetMembersOfAttributesObject(OpenApiSchema fullSchemaForAttributesObject)
        {
            AttrCapabilities requiredCapability = GetRequiredCapabilityForAttributes(_resourceTypeInfo.ResourceObjectOpenType);

            foreach ((string fieldName, OpenApiSchema resourceFieldSchema) in _schemasForResourceFields)
            {
                AttrAttribute? matchingAttribute = _resourceTypeInfo.ResourceType.FindAttributeByPublicName(fieldName);

                if (matchingAttribute != null && matchingAttribute.Capabilities.HasFlag(requiredCapability))
                {
                    AddAttributeSchemaToResourceObject(matchingAttribute, fullSchemaForAttributesObject, resourceFieldSchema);

                    resourceFieldSchema.Nullable = matchingAttribute.IsNullable();

                    if (IsFieldRequired(matchingAttribute))
                    {
                        fullSchemaForAttributesObject.Required.Add(matchingAttribute.PublicName);
                    }
                }
            }
        }

        private static AttrCapabilities GetRequiredCapabilityForAttributes(Type resourceObjectOpenType)
        {
            return resourceObjectOpenType == typeof(ResourceObjectInResponse<>) ? AttrCapabilities.AllowView :
                resourceObjectOpenType == typeof(ResourceObjectInPostRequest<>) ? AttrCapabilities.AllowCreate :
                resourceObjectOpenType == typeof(ResourceObjectInPatchRequest<>) ? AttrCapabilities.AllowChange : throw new UnreachableCodeException();
        }

        private void AddAttributeSchemaToResourceObject(AttrAttribute attribute, OpenApiSchema attributesObjectSchema, OpenApiSchema resourceAttributeSchema)
        {
            if (resourceAttributeSchema.Reference != null && !_schemaRepositoryAccessor.Current.TryLookupByType(attribute.Property.PropertyType, out _))
            {
                ExposeSchema(resourceAttributeSchema.Reference, attribute.Property.PropertyType);
            }

            attributesObjectSchema.Properties.Add(attribute.PublicName, resourceAttributeSchema);
        }

        private void ExposeSchema(OpenApiReference openApiReference, Type typeRepresentedBySchema)
        {
            OpenApiSchema fullSchema = _resourceSchemaRepository.Schemas[openApiReference.Id];
            _schemaRepositoryAccessor.Current.AddDefinition(openApiReference.Id, fullSchema);
            _schemaRepositoryAccessor.Current.RegisterType(typeRepresentedBySchema, openApiReference.Id);
        }

        private bool IsFieldRequired(ResourceFieldAttribute field)
        {
            if (field is HasManyAttribute || _resourceTypeInfo.ResourceObjectOpenType != typeof(ResourceObjectInPostRequest<>))
            {
                return false;
            }

            TypeCategory fieldTypeCategory = field.Property.GetTypeCategory();
            bool hasRequiredAttribute = field.Property.HasAttribute<RequiredAttribute>();

            return fieldTypeCategory switch
            {
                TypeCategory.NonNullableReferenceType => true,
                TypeCategory.ValueType => hasRequiredAttribute,
                TypeCategory.NullableReferenceType or TypeCategory.NullableValueType => hasRequiredAttribute,
                _ => throw new UnreachableCodeException()
            };
        }

        public void SetMembersOfRelationshipsObject(OpenApiSchema fullSchemaForRelationshipsObject)
        {
            foreach (string fieldName in _schemasForResourceFields.Keys)
            {
                RelationshipAttribute? matchingRelationship = _resourceTypeInfo.ResourceType.FindRelationshipByPublicName(fieldName);

                if (matchingRelationship != null)
                {
                    EnsureResourceIdentifierObjectSchemaExists(matchingRelationship);
                    AddRelationshipSchemaToResourceObject(matchingRelationship, fullSchemaForRelationshipsObject);
                }
            }
        }

        private void EnsureResourceIdentifierObjectSchemaExists(RelationshipAttribute relationship)
        {
            Type resourceIdentifierObjectType = typeof(ResourceIdentifierObject<>).MakeGenericType(relationship.RightType.ClrType);

            if (!ResourceIdentifierObjectSchemaExists(resourceIdentifierObjectType))
            {
                GenerateResourceIdentifierObjectSchema(resourceIdentifierObjectType);
            }
        }

        private bool ResourceIdentifierObjectSchemaExists(Type resourceIdentifierObjectType)
        {
            return _schemaRepositoryAccessor.Current.TryLookupByType(resourceIdentifierObjectType, out _);
        }

        private void GenerateResourceIdentifierObjectSchema(Type resourceIdentifierObjectType)
        {
            OpenApiSchema referenceSchemaForResourceIdentifierObject =
                _defaultSchemaGenerator.GenerateSchema(resourceIdentifierObjectType, _schemaRepositoryAccessor.Current);

            OpenApiSchema fullSchemaForResourceIdentifierObject =
                _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForResourceIdentifierObject.Reference.Id];

            Type resourceType = resourceIdentifierObjectType.GetGenericArguments()[0];
            fullSchemaForResourceIdentifierObject.Properties[JsonApiObjectPropertyName.Type] = _resourceTypeSchemaGenerator.Get(resourceType);
        }

        private void AddRelationshipSchemaToResourceObject(RelationshipAttribute relationship, OpenApiSchema fullSchemaForRelationshipsObject)
        {
            Type relationshipSchemaType = GetRelationshipSchemaType(relationship, _resourceTypeInfo.ResourceObjectOpenType);

            OpenApiSchema relationshipSchema = GetReferenceSchemaForRelationship(relationshipSchemaType) ?? CreateRelationshipSchema(relationshipSchemaType);

            fullSchemaForRelationshipsObject.Properties.Add(relationship.PublicName, relationshipSchema);

            if (IsFieldRequired(relationship))
            {
                fullSchemaForRelationshipsObject.Required.Add(relationship.PublicName);
            }
        }

        private static Type GetRelationshipSchemaType(RelationshipAttribute relationship, Type resourceObjectType)
        {
            return resourceObjectType.GetGenericTypeDefinition().IsAssignableTo(typeof(ResourceObjectInResponse<>))
                ? RelationshipTypeFactory.Instance.GetForResponse(relationship)
                : RelationshipTypeFactory.Instance.GetForRequest(relationship);
        }

        private OpenApiSchema? GetReferenceSchemaForRelationship(Type relationshipSchemaType)
        {
            _schemaRepositoryAccessor.Current.TryLookupByType(relationshipSchemaType, out OpenApiSchema? referenceSchema);
            return referenceSchema;
        }

        private OpenApiSchema CreateRelationshipSchema(Type relationshipSchemaType)
        {
            OpenApiSchema referenceSchema = _defaultSchemaGenerator.GenerateSchema(relationshipSchemaType, _schemaRepositoryAccessor.Current);

            OpenApiSchema fullSchema = _schemaRepositoryAccessor.Current.Schemas[referenceSchema.Reference.Id];

            if (IsDataPropertyNullable(relationshipSchemaType))
            {
                fullSchema.Properties[JsonApiObjectPropertyName.Data] =
                    _nullableReferenceSchemaGenerator.GenerateSchema(fullSchema.Properties[JsonApiObjectPropertyName.Data]);
            }

            if (IsRelationshipInResponseType(relationshipSchemaType))
            {
                fullSchema.Required.Remove(JsonApiObjectPropertyName.Data);
            }

            return referenceSchema;
        }

        private static bool IsRelationshipInResponseType(Type relationshipSchemaType)
        {
            Type relationshipSchemaOpenType = relationshipSchemaType.GetGenericTypeDefinition();

            return RelationshipInResponseOpenTypes.Contains(relationshipSchemaOpenType);
        }

        private static bool IsDataPropertyNullable(Type type)
        {
            PropertyInfo? dataProperty = type.GetProperty(nameof(JsonApiObjectPropertyName.Data));

            if (dataProperty == null)
            {
                throw new UnreachableCodeException();
            }

            return dataProperty.GetTypeCategory() == TypeCategory.NullableReferenceType;
        }
    }
}
