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

            var entityTypeName = document.Data.Type.ToProperCase();

            var contextEntity = context.ContextGraph.GetContextEntity(entityTypeName);
            context.RequestEntity = contextEntity;

            var entity = Activator.CreateInstance(contextEntity.EntityType);

            entity = _setEntityAttributes(entity, contextEntity, document.Data.Attributes);
            entity = _setRelationships(entity, contextEntity, document.Data.Relationships);
            
            return entity;
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
                    var convertedValue = Convert.ChangeType(newValue, entityProperty.PropertyType);
                    entityProperty.SetValue(entity, convertedValue);
                }
            }

            return entity;
        }

        private static object _setRelationships(
            object entity, ContextEntity contextEntity, Dictionary<string, Dictionary<string, object>> relationships)
        {
            if(relationships == null)
                return entity;

            var entityProperties = entity.GetType().GetProperties();
            
            foreach (var attr in contextEntity.Relationships)
            {
                var entityProperty = entityProperties.FirstOrDefault(p => p.Name == $"{attr.RelationshipName}Id");

                if (entityProperty == null)
                    throw new ArgumentException($"{contextEntity.EntityType.Name} does not contain an relationsip named {attr.RelationshipName}", nameof(entity));

                Dictionary<string, object> relationshipData;
                if (relationships.TryGetValue(attr.RelationshipName.Dasherize(), out relationshipData))
                {
                    var data = ((JObject)relationshipData["data"]).ToObject<Dictionary<string,string>>();
                    var newValue = data["id"];
                    var convertedValue = Convert.ChangeType(newValue, entityProperty.PropertyType);
                    entityProperty.SetValue(entity, convertedValue);
                }
            }

            return entity;
        }
    }
}
