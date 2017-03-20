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
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Serialization
{
    public class JsonApiDeSerializer : IJsonApiDeSerializer
    {
        private readonly DbContext _dbContext;
        private readonly IJsonApiContext _jsonApiContext;
        private readonly IGenericProcessorFactory _genericProcessorFactor;

        public JsonApiDeSerializer(DbContext dbContext, 
            IJsonApiContext jsonApiContext,
            IGenericProcessorFactory genericProcessorFactory)
        {
            _dbContext = dbContext;
            _jsonApiContext = jsonApiContext;
            _genericProcessorFactor = genericProcessorFactory;
        }

        public object Deserialize(string requestBody)
        {
            var document = JsonConvert.DeserializeObject<Document>(requestBody);
            var entity = DataToObject(document.Data);
            return entity;
        }

        public object DeserializeRelationship(string requestBody)
        {
            var data = JToken.Parse(requestBody)["data"];

            if(data is JArray)
                return data.ToObject<List<DocumentData>>();

            return new List<DocumentData> { data.ToObject<DocumentData>() };
        }


        public List<TEntity> DeserializeList<TEntity>(string requestBody)
        {
            var documents = JsonConvert.DeserializeObject<Documents>(requestBody);

            var deserializedList = new List<TEntity>();
            foreach (var data in documents.Data)
            {
                var entity = DataToObject(data);
                deserializedList.Add((TEntity)entity);
            }

            return deserializedList;
        }

        private object DataToObject(DocumentData data)
        {
            var entityTypeName = data.Type.ToProperCase();

            var contextEntity = _jsonApiContext.ContextGraph.GetContextEntity(entityTypeName);
            _jsonApiContext.RequestEntity = contextEntity;

            var entity = Activator.CreateInstance(contextEntity.EntityType);
            
            entity = _setEntityAttributes(entity, contextEntity, data.Attributes);
            entity = _setRelationships(entity, contextEntity, data.Relationships);

            var identifiableEntity = (IIdentifiable)entity;

            if (data.Id != null)
                identifiableEntity.StringId = data.Id;

            return identifiableEntity;
        }

        private object _setEntityAttributes(
            object entity, ContextEntity contextEntity, Dictionary<string, object> attributeValues)
        {
            if (attributeValues == null || attributeValues.Count == 0)
                return entity;

            var entityProperties = entity.GetType().GetProperties();

            foreach (var attr in contextEntity.Attributes)
            {
                var entityProperty = entityProperties.FirstOrDefault(p => p.Name == attr.InternalAttributeName);

                if (entityProperty == null)
                    throw new ArgumentException($"{contextEntity.EntityType.Name} does not contain an attribute named {attr.InternalAttributeName}", nameof(entity));

                object newValue;
                if (attributeValues.TryGetValue(attr.PublicAttributeName.Dasherize(), out newValue))
                {
                    var convertedValue = TypeHelper.ConvertType(newValue, entityProperty.PropertyType);
                    entityProperty.SetValue(entity, convertedValue);
                }
            }

            return entity;
        }

        private object _setRelationships(
            object entity, 
            ContextEntity contextEntity, 
            Dictionary<string, RelationshipData> relationships)
        {
            if (relationships == null || relationships.Count == 0)
                return entity;

            var entityProperties = entity.GetType().GetProperties();

            foreach (var attr in contextEntity.Relationships)
            {
                if (attr.IsHasOne)
                    entity = _setHasOneRelationship(entity, entityProperties, attr, contextEntity, relationships);
                else
                    entity = _setHasManyRelationship(entity, entityProperties, attr, contextEntity, relationships);
            }

            return entity;
        }

        private object _setHasOneRelationship(object entity, 
            PropertyInfo[] entityProperties, 
            RelationshipAttribute attr, 
            ContextEntity contextEntity, 
            Dictionary<string, RelationshipData> relationships)
        {
            var entityProperty = entityProperties.FirstOrDefault(p => p.Name == $"{attr.InternalRelationshipName}Id");

            if (entityProperty == null)
                throw new JsonApiException("400", $"{contextEntity.EntityType.Name} does not contain an relationsip named {attr.InternalRelationshipName}");

            var relationshipName = attr.InternalRelationshipName.Dasherize();

            if (relationships.TryGetValue(relationshipName, out RelationshipData relationshipData))
            {
                var relationshipAttr = _jsonApiContext.RequestEntity.Relationships
                        .SingleOrDefault(r => r.PublicRelationshipName == relationshipName);
                
                var data = (Dictionary<string, string>)relationshipData.ExposedData;

                if (data == null) return entity;

                var newValue = data["id"];
                var convertedValue = TypeHelper.ConvertType(newValue, entityProperty.PropertyType);

                _jsonApiContext.RelationshipsToUpdate[relationshipAttr] = convertedValue;

                entityProperty.SetValue(entity, convertedValue);
            }

            return entity;
        }

        private object _setHasManyRelationship(object entity,
            PropertyInfo[] entityProperties, 
            RelationshipAttribute attr, 
            ContextEntity contextEntity, 
            Dictionary<string, RelationshipData> relationships)
        {
            var entityProperty = entityProperties.FirstOrDefault(p => p.Name == attr.InternalRelationshipName);

            if (entityProperty == null)
                throw new JsonApiException("400", $"{contextEntity.EntityType.Name} does not contain an relationsip named {attr.InternalRelationshipName}");

            var relationshipName = attr.InternalRelationshipName.Dasherize();

            if (relationships.TryGetValue(relationshipName, out RelationshipData relationshipData))
            {
                var data = (List<Dictionary<string, string>>)relationshipData.ExposedData;

                if (data == null) return entity;

                var genericProcessor = _genericProcessorFactor.GetProcessor(attr.Type);
                var ids = relationshipData.ManyData.Select(r => r["id"]);
                genericProcessor.SetRelationships(entity, attr, ids);    
            }

            return entity;
        }
    }
}
