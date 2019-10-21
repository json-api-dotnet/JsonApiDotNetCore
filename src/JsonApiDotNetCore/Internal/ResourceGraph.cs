using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal
{
    /// <summary>
    ///  keeps track of all the models/resources defined in JADNC
    /// </summary>
    public class ResourceGraph : IContextEntityProvider
    {
        internal List<ValidationResult> ValidationResults { get; }
        private List<ContextEntity> _entities { get; }

        public ResourceGraph(List<ContextEntity> entities)
        {
            _entities = entities;
            ValidationResults = new List<ValidationResult>();
        }

        public ResourceGraph(List<ContextEntity> entities, List<ValidationResult> validationResults)
        {
            _entities = entities;
            ValidationResults = validationResults;
        }

        /// <inheritdoc />
        public ContextEntity[] GetContextEntities() => _entities.ToArray();

        /// <inheritdoc />
        public ContextEntity GetContextEntity(string entityName)
            => _entities.SingleOrDefault(e => string.Equals(e.EntityName, entityName, StringComparison.OrdinalIgnoreCase));

        /// <inheritdoc />
        public ContextEntity GetContextEntity(Type entityType)
            => _entities.SingleOrDefault(e => e.EntityType == entityType);
        /// <inheritdoc />
        public ContextEntity GetContextEntity<TResource>() where TResource : class, IIdentifiable
            => GetContextEntity(typeof(TResource));
    }
}
