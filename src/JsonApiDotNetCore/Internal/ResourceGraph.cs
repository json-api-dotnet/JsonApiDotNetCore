using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal
{
    public class ControllerResourceMap
    {
        public string ControllerName { get; set; }
        public Type Resource { get; set; }
    }

    /// <summary>
    ///  keeps track of all the models/resources defined in JADNC
    /// </summary>
    public class ResourceGraph : IResourceGraph
    {
        internal List<ContextEntity> Entities { get; }
        internal List<ValidationResult> ValidationResults { get; }

        public List<ControllerResourceMap> ControllerResourceMap { get; internal set; }

        [Obsolete("please instantiate properly, dont use the static constructor")]
        internal static IResourceGraph Instance { get; set; }

        public ResourceGraph() { }
        [Obsolete("Use new one")]
        public ResourceGraph(List<ContextEntity> entities, bool usesDbContext)
        {
            Entities = entities;
            UsesDbContext = usesDbContext;
            ValidationResults = new List<ValidationResult>();
            Instance = this;
        }

        internal ResourceGraph(List<ContextEntity> entities, bool usesDbContext, List<ValidationResult> validationResults, List<ControllerResourceMap> controllerContexts)
        {
            ControllerResourceMap = controllerContexts;
            Entities = entities;
            UsesDbContext = usesDbContext;
            ValidationResults = validationResults;
            Instance = this;
        }

        /// <inheritdoc />
        public bool UsesDbContext { get; }

        public RelationshipAttribute GetInverseRelationship(RelationshipAttribute relationship)
        {
            if (relationship.InverseNavigation == null) return null;
            return GetContextEntity(relationship.DependentType).Relationships.SingleOrDefault(r => r.InternalRelationshipName == relationship.InverseNavigation);
        }

        /// <inheritdoc />
        public ContextEntity GetContextEntity(string entityName)
            => Entities.SingleOrDefault(e => string.Equals(e.EntityName, entityName, StringComparison.OrdinalIgnoreCase));

        /// <inheritdoc />
        public ContextEntity GetContextEntity(Type entityType)
            => Entities.SingleOrDefault(e => e.EntityType == entityType);
        /// <inheritdoc />
        public ContextEntity GetContextEntity<TResource>() where TResource : class, IIdentifiable
            => GetContextEntity(typeof(TResource));
    }
}
