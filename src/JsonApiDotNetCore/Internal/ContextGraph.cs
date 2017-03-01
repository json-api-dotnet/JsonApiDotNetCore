using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;

namespace JsonApiDotNetCore.Internal
{
    public class ContextGraph<T> : IContextGraph where T : DbContext
    {
        public List<ContextEntity> Entities { get; set; }

        public ContextEntity GetContextEntity(string dbSetName)
        {
            return Entities
                .FirstOrDefault(e => 
                    e.EntityName.ToLower() == dbSetName.ToLower());
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
                throw new JsonApiException("400", $"{parentEntityType} does not contain a relationship named {relationshipName}");

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
                    r.RelationshipName.ToLower() == relationshipName.ToLower())
                ?.RelationshipName;
        }
    }
}
