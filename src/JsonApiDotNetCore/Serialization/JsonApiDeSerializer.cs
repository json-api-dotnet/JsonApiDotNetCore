using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization
{
    public static class JsonApiDeSerializer
    {
        public static object Deserialize(string requestBody, IContextGraph contextGraph)
        {
            var document = JsonConvert.DeserializeObject<Document>(requestBody);

            var entityTypeName = document.Data.Type.ToProperCase();

            var contextEntity = contextGraph.GetContextEntity(entityTypeName);

            var entity = Activator.CreateInstance(contextEntity.EntityType);

            return _setEntityAttributes(entity, contextEntity, document.Data.Attributes);
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
                if (attributeValues.TryGetValue(attr.PublicAttributeName, out newValue))
                {
                    Convert.ChangeType(newValue, entityProperty.PropertyType);
                    entityProperty.SetValue(entity, newValue);
                }
            }

            return entity;
        }
    }
}
