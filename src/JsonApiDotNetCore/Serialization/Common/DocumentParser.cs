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

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Abstract base class for deserialization. Deserializes JSON content into <see cref="Document"/>s
    /// And constructs instances of the resource(s) in the document body.
    /// </summary>
    public abstract class DocumentParser
    {
        protected readonly IContextEntityProvider _provider;
        protected Document _document;

        protected DocumentParser(IContextEntityProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// This method is called each time an <paramref name="entity"/> is constructed
        /// from the serialized content, which is used to do additional processing
        /// depending on the type of deserializers.
        /// </summary>
        /// <remarks>
        /// See the impementation of this method in <see cref="ResponseDeserializer"/>
        /// and <see cref="RequestDeserializer"/> for examples.
        /// </remarks>
        /// <param name="entity">The entity that was constructed from the document's body</param>
        /// <param name="field">The metadata for the exposed field</param>
        /// <param name="data">Relationship data for <paramref name="entity"/>. Is null when <paramref name="field"/> is not a <see cref="RelationshipAttribute"/></param>
        protected abstract void AfterProcessField(IIdentifiable entity, IResourceField field, RelationshipData data = null);

        /// <inheritdoc/>
        protected object Deserialize(string body)
        {
            var bodyJToken = LoadJToken(body);
            _document = bodyJToken.ToObject<Document>();
            if (_document.IsManyData)
            {
                if (_document.ManyData.Count == 0)
                    return new List<IIdentifiable>();

                return _document.ManyData.Select(ParseResourceObject).ToList();
            }

            if (_document.SingleData == null) return null;
                return ParseResourceObject(_document.SingleData);
        }

        /// <summary>
        /// Sets the attributes on a parsed entity.
        /// </summary>
        /// <param name="entity">The parsed entity</param>
        /// <param name="attributeValues">Attributes and their values, as in the serialized content</param>
        /// <param name="attributes">Exposed attributes for <paramref name="entity"/></param>
        /// <returns></returns>
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
        /// <summary>
        /// Sets the relationships on a parsed entity
        /// </summary>
        /// <param name="entity">The parsed entity</param>
        /// <param name="relationshipsValues">Relationships and their values, as in the serialized content</param>
        /// <param name="relationshipAttributes">Exposed relatinships for <paramref name="entity"/></param>
        /// <returns></returns>
        protected IIdentifiable SetRelationships(IIdentifiable entity, Dictionary<string, RelationshipData> relationshipsValues, List<RelationshipAttribute> relationshipAttributes)
        {
            if (relationshipsValues == null || relationshipsValues.Count == 0)
                return entity;

            var entityProperties = entity.GetType().GetProperties();
            foreach (var attr in relationshipAttributes)
            {
                if (attr is HasOneAttribute hasOne)
                    SetHasOneRelationship(entity, entityProperties, (HasOneAttribute)attr, relationshipsValues);
                else
                    SetHasManyRelationship(entity, (HasManyAttribute)attr, relationshipsValues);

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


        /// <summary>
        /// Creates an instance of the referenced type in <paramref name="data"/>
        /// and sets its attributes and relationships
        /// </summary>
        /// <param name="data"></param>
        /// <returns>The parsed entity</returns>
        private IIdentifiable ParseResourceObject(ResourceObject data)
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

        /// <summary>
        /// Sets a HasOne relationship on a parsed entity. If present, also
        /// populates the foreign key.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityProperties"></param>
        /// <param name="attr"></param>
        /// <param name="relationships"></param>
        /// <returns></returns>
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

            //if (foreignKeyProperty == null)
            //{   /// there is no FK from the current entity pointing to the related object,
            //    /// i.e. means we're populating the relationship from the principal side.
            //    SetNavigation(entity, attr, relatedId);
            //}
            //else
            //{
            //    /// there is a FK from the current entity pointing to the related object,
            //    /// i.e. we're populating the relationship from the dependent side.
            //    SetDependentSide(entity, foreignKeyProperty, attr, relatedId);
            //}

            if (foreignKeyProperty != null)
                /// there is a FK from the current entity pointing to the related object,
                /// i.e. we're populating the relationship from the dependent side.
                SetForeignKey(entity, foreignKeyProperty, attr, relatedId);


            SetNavigation(entity, attr, relatedId);
            


            // allow for additional processing of relationships as required for the
            // serializer class that implements this abstract class.
            AfterProcessField(entity, attr, relationshipData);

            return entity;
        }

        /// <summary>
        /// Sets the dependent side of a HasOne relationship, which means that a
        /// foreign key also will to be populated.
        /// </summary>
        private void SetForeignKey(IIdentifiable entity, PropertyInfo foreignKey, HasOneAttribute attr, string id)
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

        /// <summary>
        /// Sets the principal side of a HasOne relationship, which means no
        /// foreign key is involved
        /// </summary>
        private void SetNavigation(IIdentifiable entity, HasOneAttribute attr, string relatedId)
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

        /// <summary>
        /// Sets a HasMany relationship.
        /// </summary>
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

        private object ConvertAttrValue(object newValue, Type targetType)
        {
            if (newValue is JContainer jObject)
                // the attribute value is a complex type that needs additional deserialization
                return DeserializeComplexType(jObject, targetType);

            // the attribute value is a native C# type.
            var convertedValue = TypeHelper.ConvertType(newValue, targetType);
            return convertedValue;
        }

        private object DeserializeComplexType(JContainer obj, Type targetType)
        {
            return obj.ToObject(targetType);
        }
    }
}
