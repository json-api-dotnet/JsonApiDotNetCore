using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonApiDotNetCore.Internal
{
    public interface IContextGraph
    {
        /// <summary>
        /// Gets the value of the navigation property, defined by the relationshipName,
        /// on the provided instance.
        /// </summary>
        /// <param name="resource">The resource instance</param>
        /// <param name="propertyName">The navigation property name.</param>
        /// <example>
        /// <code>
        /// _graph.GetRelationship(todoItem, nameof(TodoItem.Owner));
        /// </code>
        /// </example>
        object GetRelationship<TParent>(TParent resource, string propertyName);

        /// <summary>
        /// Get the internal navigation property name for the specified public
        /// relationship name.
        /// </summary>
        /// <param name="relationshipName">The public relationship name specified by a <see cref="HasOneAttribute" /> or <see cref="HasManyAttribute" /></param>
        /// <example>
        /// <code>
        /// _graph.GetRelationshipName&lt;TodoItem&gt;("achieved-date");
        /// // returns "AchievedDate"
        /// </code>
        /// </example>
        string GetRelationshipName<TParent>(string relationshipName);

        /// <summary>
        /// Get the resource metadata by the DbSet property name
        /// </summary>
        ContextEntity GetContextEntity(string dbSetName);

        /// <summary>
        /// Get the resource metadata by the resource type
        /// </summary>
        ContextEntity GetContextEntity(Type entityType);

        /// <summary>
        /// Get the public attribute name for a type based on the internal attribute name.
        /// </summary>
        /// <param name="internalAttributeName">The internal attribute name for a <see cref="Attr" />.</param>
        string GetPublicAttributeName<TParent>(string internalAttributeName);

        /// <summary>
        /// Was built against an EntityFrameworkCore DbContext ?
        /// </summary>
        bool UsesDbContext { get; }
    }

    public class ContextGraph : IContextGraph
    {
        internal List<ContextEntity> Entities { get; }
        internal List<ValidationResult> ValidationResults { get; }
        internal static IContextGraph Instance { get; set; }

        public ContextGraph() { }
        public ContextGraph(List<ContextEntity> entities, bool usesDbContext)
        {
            Entities = entities;
            UsesDbContext = usesDbContext;
            ValidationResults = new List<ValidationResult>();
            Instance = this;
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
            Instance = this;
        }

        /// </ inheritdoc>
        public bool UsesDbContext { get; }

        /// </ inheritdoc>
        public ContextEntity GetContextEntity(string entityName)
            => Entities.SingleOrDefault(e => string.Equals(e.EntityName, entityName, StringComparison.OrdinalIgnoreCase));

        /// </ inheritdoc>
        public ContextEntity GetContextEntity(Type entityType)
            => Entities.SingleOrDefault(e => e.EntityType == entityType);

        /// </ inheritdoc>
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

        /// </ inheritdoc>
        public string GetRelationshipName<TParent>(string relationshipName)
        {
            var entityType = typeof(TParent);
            return Entities
                .SingleOrDefault(e => e.EntityType == entityType)
                ?.Relationships
                .SingleOrDefault(r => r.Is(relationshipName))
                ?.InternalRelationshipName;
        }

        public string GetPublicAttributeName<TParent>(string internalAttributeName)
        {
            return GetContextEntity(typeof(TParent))
                .Attributes
                .SingleOrDefault(a => a.InternalAttributeName == internalAttributeName)?
                .PublicAttributeName;
        }
  }
}
