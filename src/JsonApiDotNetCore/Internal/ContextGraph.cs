using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonApiDotNetCore.Internal
{
    public class ContextGraph : IContextGraph
    {
        private List<ContextEntity> _entities;

        public ContextGraph() { }
        
        public ContextGraph(List<ContextEntity> entities, bool usesDbContext) 
        {
            _entities = entities;
            UsesDbContext = usesDbContext;
        }

        public bool UsesDbContext { get; }

        public ContextEntity GetContextEntity(string entityName)
            => _entities.SingleOrDefault(e => string.Equals(e.EntityName, entityName, StringComparison.OrdinalIgnoreCase));

        public ContextEntity GetContextEntity(Type entityType)
            => _entities.SingleOrDefault(e => e.EntityType == entityType);

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
            return _entities
                .SingleOrDefault(e => e.EntityType == entityType) 
                ?.Relationships
                .SingleOrDefault(r => string.Equals(r.PublicRelationshipName, relationshipName, StringComparison.OrdinalIgnoreCase)) 
                ?.InternalRelationshipName;
        }
    }
}
