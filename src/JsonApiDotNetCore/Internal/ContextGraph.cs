using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;

namespace JsonApiDotNetCore.Internal
{
    public class ContextGraph : IContextGraph
    {
        public List<ContextEntity> Entities { get; set; }
        public bool UsesDbContext { get; set; }

        public ContextEntity GetContextEntity(string entityName)
            => Entities.SingleOrDefault(e => string.Equals(e.EntityName, entityName, StringComparison.OrdinalIgnoreCase));

        public ContextEntity GetContextEntity(Type entityType)
            => Entities.SingleOrDefault(e => e.EntityType == entityType);

        public object GetRelationship<TParent>(TParent entity, string relationshipName)
        {
            var parentEntityType = entity.GetType();

            var navigationProperty = parentEntityType
                .GetProperties()
                .SingleOrDefault(p => string.Equals(p.Name, relationshipName, StringComparison.OrdinalIgnoreCase));

            if (navigationProperty == null)
                throw new JsonApiException(400, $"{parentEntityType} does not contain a relationship named {relationshipName}");

            return navigationProperty.GetValue(entity);
        }

        public string GetRelationshipName<TParent>(string relationshipName)
        {
            var entityType = typeof(TParent);
            return Entities
                .SingleOrDefault(e => e.EntityType == entityType)
                .Relationships
                .SingleOrDefault(r => string.Equals(r.PublicRelationshipName, relationshipName, StringComparison.OrdinalIgnoreCase))
                ?.InternalRelationshipName;
        }
    }
}
