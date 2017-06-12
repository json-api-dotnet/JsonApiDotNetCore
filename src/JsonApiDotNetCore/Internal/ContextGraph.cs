using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;

namespace JsonApiDotNetCore.Internal
{
    public class ContextGraph : IContextGraph
    {
        public List<ContextEntity> Entities { get; set; }
        public bool UsesDbContext  { get; set; }

        public ContextEntity GetContextEntity(string entityName)
        {
            return Entities
                .FirstOrDefault(e => 
                    e.EntityName.ToLower() == entityName.ToLower());
        }

        public ContextEntity GetContextEntity(Type entityType)
        {
            return Entities
                .FirstOrDefault(e => 
                    e.EntityType == entityType);
        }

        public object GetRelationship<TParent>(TParent entity, string relationshipName)
        {
            var parentEntityType = entity.GetType();

            var navigationProperty = parentEntityType
                .GetProperties()
                .FirstOrDefault(p => p.Name.ToLower() == relationshipName.ToLower());

            if(navigationProperty == null)
                throw new JsonApiException(400, $"{parentEntityType} does not contain a relationship named {relationshipName}");

            return navigationProperty.GetValue(entity);
        }

        public string GetRelationshipName<TParent>(string relationshipName)
        {
            var entityType = typeof(TParent);
            return Entities
                .FirstOrDefault(e => 
                    e.EntityType == entityType)
                .Relationships
                .FirstOrDefault(r => 
                    r.PublicRelationshipName.ToLower() == relationshipName.ToLower())
                ?.InternalRelationshipName;
        }
    }
}
