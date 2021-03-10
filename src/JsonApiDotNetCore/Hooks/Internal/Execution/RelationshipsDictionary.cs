using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Hooks.Internal.Execution
{
    /// <summary>
    /// Implementation of IAffectedRelationships{TRightResource} It is practically a ReadOnlyDictionary{RelationshipAttribute, HashSet{TRightResource}}
    /// dictionary with the two helper methods defined on IAffectedRelationships{TRightResource}.
    /// </summary>
    [PublicAPI]
    public class RelationshipsDictionary<TResource> : Dictionary<RelationshipAttribute, HashSet<TResource>>, IRelationshipsDictionary<TResource>
        where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:JsonApiDotNetCore.Hooks.Internal.Execution.RelationshipsDictionary`1" /> class.
        /// </summary>
        /// <param name="relationships">
        /// Relationships.
        /// </param>
        public RelationshipsDictionary(IDictionary<RelationshipAttribute, HashSet<TResource>> relationships)
            : base(relationships)
        {
        }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make life a bit easier with generics
        /// </summary>
        internal RelationshipsDictionary(IDictionary<RelationshipAttribute, IEnumerable> relationships)
            : this(relationships.ToDictionary(pair => pair.Key, pair => (HashSet<TResource>)pair.Value))
        {
        }

        /// <inheritdoc />
        public IDictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship<TRelatedResource>()
            where TRelatedResource : class, IIdentifiable
        {
            return GetByRelationship(typeof(TRelatedResource));
        }

        /// <inheritdoc />
        public IDictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship(Type resourceType)
        {
            return this.Where(pair => pair.Key.RightType == resourceType).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        /// <inheritdoc />
        public HashSet<TResource> GetAffected(Expression<Func<TResource, object>> navigationAction)
        {
            ArgumentGuard.NotNull(navigationAction, nameof(navigationAction));

            PropertyInfo property = HooksNavigationParser.ParseNavigationExpression(navigationAction);
            return this.Where(pair => pair.Key.Property.Name == property.Name).Select(pair => pair.Value).SingleOrDefault();
        }
    }
}
