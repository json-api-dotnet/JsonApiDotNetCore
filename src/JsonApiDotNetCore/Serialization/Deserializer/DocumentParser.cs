using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonApiDotNetCore.Serialization.Deserializer
{
    /// <summary>
    /// Base class for deserialization. 
    /// </summary>
    public abstract class DocumentParser
    {
        protected Document _document;
        protected readonly IContextEntityProvider _provider;

        protected DocumentParser(IContextEntityProvider provider)
        {
            _provider = provider;
        }

        protected abstract void AfterProcessField(IIdentifiable entity, IResourceField field, RelationshipData data = null);

        protected object Deserialize(string body)
        {
            var bodyJToken = LoadJToken(body);
            _document = bodyJToken.ToObject<Document>();
            if (_document.IsManyData)
            {
                if (_document.ManyData.Count == 0) return new List<IIdentifiable>();
                return _document.ManyData.Select(DocumentToObject).ToList();
            }
            else
            {
                if (_document.SingleData == null) return null;
                return DocumentToObject(_document.SingleData);
            }
        }

        protected IIdentifiable SetAttributes(IIdentifiable entity, Dictionary<string, object> attributeValues, List<AttrAttribute> attributes)
        {
            if (attributeValues == null || attributeValues.Count == 0)
                return entity;

            foreach (var attr in attributes)
            {
                if (attributeValues.TryGetValue(attr.PublicAttributeName, out object newValue))
                {
                    var convertedValue = ConvertAttrValue(newValue, attr.PropertyInfo.PropertyType);
                    attr.SetValue(entity, convertedValue);
                    AfterProcessField(entity, attr);
                }
            }

            return entity;
        }

        protected IIdentifiable SetRelationships(IIdentifiable entity, Dictionary<string, RelationshipData> relationships, List<RelationshipAttribute> relationshipAttributes)
        {
            if (relationships == null || relationships.Count == 0)
                return entity;

            var entityProperties = entity.GetType().GetProperties();
            foreach (var attr in relationshipAttributes)
            {
                if (attr is HasOneAttribute hasOne)
                    SetHasOneRelationship(entity, entityProperties, (HasOneAttribute)attr, relationships);
                else
                    SetHasManyRelationship(entity, (HasManyAttribute)attr, relationships);

            }
            return entity;
        }

        private JToken LoadJToken(string body)
        {
            JToken jToken;
            using (JsonReader jsonReader = new JsonTextReader(new StringReader(body)))
            {
                jToken = JToken.Load(jsonReader);
            }
            return jToken;
        }

        private IIdentifiable DocumentToObject(ResourceObject data)
        {
            var contextEntity = _provider.GetContextEntity(data.Type);
            if (contextEntity == null)
            {
                throw new JsonApiException(400,
                     message: $"This API does not contain a json:api resource named '{data.Type}'.",
                     detail: "This resource is not registered on the ResourceGraph. "
                             + "If you are using Entity Framework, make sure the DbSet matches the expected resource name. "
                             + "If you have manually registered the resource, check that the call to AddResource correctly sets the public name.");
            }

            var entity = (IIdentifiable)Activator.CreateInstance(contextEntity.EntityType);

            entity = SetAttributes(entity, data.Attributes, contextEntity.Attributes);
            entity = SetRelationships(entity, data.Relationships, contextEntity.Relationships);

            if (data.Id != null)
                entity.StringId = data.Id?.ToString();

            return entity;
        }


        private object ConvertAttrValue(object newValue, Type targetType)
        {
            if (newValue is JContainer jObject)
                return DeserializeComplexType(jObject, targetType);

            var convertedValue = TypeHelper.ConvertType(newValue, targetType);
            return convertedValue;
        }

        private object DeserializeComplexType(JContainer obj, Type targetType)
        {
            return obj.ToObject(targetType);
            //return obj.ToObject(targetType, _jsonSerializer);
        }

        private object SetHasOneRelationship(IIdentifiable entity,
            PropertyInfo[] entityProperties,
            HasOneAttribute attr,
            Dictionary<string, RelationshipData> relationships)
        {
            if (relationships.TryGetValue(attr.PublicRelationshipName, out RelationshipData relationshipData) == false)
                return entity;

            var rio = (ResourceIdentifierObject)relationshipData.Data;
            var relatedId = rio?.Id ?? null;

            // this does not make sense in the following case: if we're setting the dependent of a one-to-one relationship, IdentifiablePropertyName should be null.
            var foreignKeyProperty = entityProperties.FirstOrDefault(p => p.Name == attr.IdentifiablePropertyName);

            if (foreignKeyProperty == null)
            {
                /// there is no FK from the current entity pointing to the related object,
                /// i.e. means we're populating the relationship from the principal side.
                SetPrincipalSide(entity, attr, relatedId);
            }
            else
            {
                /// there is a FK from the current entity pointing to the related object,
                /// i.e. we're populating the relationship from the dependent side.
                SetDependentSide(entity, foreignKeyProperty, attr, relatedId);
            }

            AfterProcessField(entity, attr, relationshipData);

            return entity;
        }

        private void SetDependentSide(IIdentifiable entity, PropertyInfo foreignKey, HasOneAttribute attr, string id)
        {
            bool foreignKeyPropertyIsNullableType = Nullable.GetUnderlyingType(foreignKey.PropertyType) != null
                || foreignKey.PropertyType == typeof(string);
            if (id == null && !foreignKeyPropertyIsNullableType)
            {
                // this happens when a non-optional relationship is deliberatedly set to null.
                // For a server deserializer, it should be mapped to a BadRequest HTTP error code.
                throw new FormatException($"Cannot set required relationship identifier '{attr.IdentifiablePropertyName}' to null because it is a non-nullable type.");
            }
            var convertedId = TypeHelper.ConvertType(id, foreignKey.PropertyType);
            foreignKey.SetValue(entity, convertedId);
        }

        private void SetPrincipalSide(IIdentifiable entity, HasOneAttribute attr, string relatedId)
        {
            if (relatedId == null)
            {
                attr.SetValue(entity, null);
            }
            else
            {
                var relatedInstance = attr.DependentType.New<IIdentifiable>();
                relatedInstance.StringId = relatedId;
                attr.SetValue(entity, relatedInstance);
            }
        }


        private object SetHasManyRelationship(IIdentifiable entity,
            HasManyAttribute attr,
            Dictionary<string, RelationshipData> relationships)
        {
            if (relationships.TryGetValue(attr.PublicRelationshipName, out RelationshipData relationshipData))
            {
                if (!relationshipData.IsManyData)
                    return entity;

                var relatedResources = relationshipData.ManyData.Select(rio =>
                {
                    var relatedInstance = attr.DependentType.New<IIdentifiable>();
                    relatedInstance.StringId = rio.Id;
                    return relatedInstance;
                });

                var convertedCollection = TypeHelper.ConvertCollection(relatedResources, attr.DependentType);
                attr.SetValue(entity, convertedCollection);
                AfterProcessField(entity, attr, relationshipData);
            }

            return entity;
        }
    }
}
