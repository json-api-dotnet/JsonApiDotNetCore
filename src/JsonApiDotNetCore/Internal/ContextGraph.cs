using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonApiDotNetCore.Internal
{
    public interface IContextGraph
    {
        object GetRelationship<TParent>(TParent entity, string relationshipName);
        string GetRelationshipName<TParent>(string relationshipName);
        ContextEntity GetContextEntity(string dbSetName);
        ContextEntity GetContextEntity(Type entityType);
        bool UsesDbContext { get; }
    }

    public class ContextGraph : IContextGraph
    {
        internal List<ContextEntity> Entities { get; }
        internal List<ValidationResult> ValidationResults { get; }

        public ContextGraph() { }

        public ContextGraph(List<ContextEntity> entities, bool usesDbContext)
        {
            Entities = entities;
            UsesDbContext = usesDbContext;
            ValidationResults = new List<ValidationResult>();
        }

        // eventually, this is the planned public constructor
        // to avoid breaking changes, we will be leaving the original constructor in place
        // until the context graph validation process is completed
        // you can track progress on this issue here: https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/170
        internal ContextGraph(List<ContextEntity> entities, bool usesDbContext, List<ValidationResult> validationResults)
        {
            Entities = entities;
            UsesDbContext = usesDbContext;
            ValidationResults = validationResults;
        }

        public bool UsesDbContext { get; }

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
                ?.Relationships
                .SingleOrDefault(r => r.Is(relationshipName))
                ?.InternalRelationshipName;
        }
    }
}
