using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonApiDotNetCore.Serialization
{
    public static class JsonApiDeSerializer
    {
        public static object Deserialize(string requestBody, IJsonApiContext context)
        {
            var document = JsonConvert.DeserializeObject<Document>(requestBody);
            var entity = DataToObject(document.Data, context);
            return entity;
        }

        public static object DeserializeRelationship(string requestBody, IJsonApiContext context)
        {
            var data = JToken.Parse(requestBody)["data"];

            if(data is JArray)
                return data.ToObject<List<DocumentData>>();

            return new List<DocumentData> { data.ToObject<DocumentData>() };
        }


        public static List<TEntity> DeserializeList<TEntity>(string requestBody, IJsonApiContext context)
        {
            var documents = JsonConvert.DeserializeObject<Documents>(requestBody);

            var deserializedList = new List<TEntity>();
            foreach (var data in documents.Data)
            {
                var entity = DataToObject(data, context);
                deserializedList.Add((TEntity)entity);
            }

            return deserializedList;
        }

        private static object DataToObject(DocumentData data, IJsonApiContext context)
        {
            var entityTypeName = data.Type.ToProperCase();

            var contextEntity = context.ContextGraph.GetContextEntity(entityTypeName);
            context.RequestEntity = contextEntity;

            var entity = Activator.CreateInstance(contextEntity.EntityType);

            entity = _setEntityAttributes(entity, contextEntity, data.Attributes);
            entity = _setRelationships(entity, contextEntity, data.Relationships);

            var identifiableEntity = (IIdentifiable)entity;

            if (data.Id != null)
                identifiableEntity.Id = ChangeType(data.Id, identifiableEntity.Id.GetType());

            return identifiableEntity;
        }

        private static object _setEntityAttributes(
            object entity, ContextEntity contextEntity, Dictionary<string, object> attributeValues)
        {
            var entityProperties = entity.GetType().GetProperties();

            foreach (var attr in contextEntity.Attributes)
            {
                var entityProperty = entityProperties.FirstOrDefault(p => p.Name == attr.InternalAttributeName);

                if (entityProperty == null)
                    throw new ArgumentException($"{contextEntity.EntityType.Name} does not contain an attribute named {attr.InternalAttributeName}", nameof(entity));

                object newValue;
                if (attributeValues.TryGetValue(attr.PublicAttributeName.Dasherize(), out newValue))
                {
                    var convertedValue = ChangeType(newValue, entityProperty.PropertyType);
                    entityProperty.SetValue(entity, convertedValue);
                }
            }

            return entity;
        }

        private static object _setRelationships(
            object entity, ContextEntity contextEntity, Dictionary<string, RelationshipData> relationships)
        {
            if (relationships == null || relationships.Count == 0)
                return entity;

            var entityProperties = entity.GetType().GetProperties();

            foreach (var attr in contextEntity.Relationships)
            {
                var entityProperty = entityProperties.FirstOrDefault(p => p.Name == $"{attr.InternalRelationshipName}Id");

                if (entityProperty == null)
                    throw new JsonApiException("400", $"{contextEntity.EntityType.Name} does not contain an relationsip named {attr.InternalRelationshipName}");

                var relationshipName = attr.InternalRelationshipName.Dasherize();
                RelationshipData relationshipData;
                if (relationships.TryGetValue(relationshipName, out relationshipData))
                {
                    var data = (Dictionary<string, string>)relationshipData.ExposedData;

                    if (data == null) continue;

                    var newValue = data["id"];
                    var convertedValue = ChangeType(newValue, entityProperty.PropertyType);
                    entityProperty.SetValue(entity, convertedValue);
                }
            }

            return entity;
        }

        private static object ChangeType(object value, Type conversion)
        {
            var t = conversion;

            if (t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                    return null;

                t = Nullable.GetUnderlyingType(t);
            }

            return Convert.ChangeType(value, t);
        }
    }
}
