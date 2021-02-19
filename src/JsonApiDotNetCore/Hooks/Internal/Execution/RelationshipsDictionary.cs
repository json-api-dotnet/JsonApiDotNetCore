using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Hooks.Internal.Execution
{
    /// <summary>
    /// Implementation of IAffectedRelationships{TRightResource}
    /// 
    /// It is practically a ReadOnlyDictionary{RelationshipAttribute, HashSet{TRightResource}} dictionary
    /// with the two helper methods defined on IAffectedRelationships{TRightResource}.
    /// </summary>
    public class RelationshipsDictionary<TResource> :
        Dictionary<RelationshipAttribute, HashSet<TResource>>,
        IRelationshipsDictionary<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:JsonApiDotNetCore.Hooks.Internal.Execution.RelationshipsDictionary`1"/> class.
        /// </summary>
        /// <param name="relationships">Relationships.</param>
        public RelationshipsDictionary(Dictionary<RelationshipAttribute, HashSet<TResource>> relationships) : base(relationships) { }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make life a bit easier with generics
        /// </summary>
        internal RelationshipsDictionary(Dictionary<RelationshipAttribute, IEnumerable> relationships)
            : this(TypeHelper.ConvertRelationshipDictionary<TResource>(relationships)) { }

        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship<TRelatedResource>() where TRelatedResource : class, IIdentifiable
        {
            return GetByRelationship(typeof(TRelatedResource));
        }

        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship(Type resourceType)
        {
            return this.Where(p => p.Key.RightType == resourceType).ToDictionary(p => p.Key, p => p.Value);
        }

        /// <inheritdoc />
        public HashSet<TResource> GetAffected(Expression<Func<TResource, object>> navigationAction)
        {
            ArgumentGuard.NotNull(navigationAction, nameof(navigationAction));

            var property = TypeHelper.ParseNavigationExpression(navigationAction);
            return this.Where(p => p.Key.Property.Name == property.Name).Select(p => p.Value).SingleOrDefault();
        }
    }
}
