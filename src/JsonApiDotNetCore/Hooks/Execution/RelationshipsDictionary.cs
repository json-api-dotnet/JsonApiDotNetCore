using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// A dummy interface used internally by the hook executor.
    /// </summary>
    public interface IRelationshipsDictionary { }

    /// <summary>
    /// An interface that is implemented to expose a relationship dictionary on another class.
    /// </summary>
    public interface IByAffectedRelationships<TDependentResource> :
        IRelationshipGetters<TDependentResource> where TDependentResource : class, IIdentifiable
    {
        /// <summary>
        /// Gets a dictionary of affected resources grouped by affected relationships.
        /// </summary>
        Dictionary<RelationshipAttribute, HashSet<TDependentResource>> AffectedRelationships { get; }
    }

    /// <summary>
    /// A helper class that provides insights in which relationships have been updated for which entities.
    /// </summary>
    public interface IRelationshipsDictionary<TDependentResource> :
        IRelationshipGetters<TDependentResource>,
        IReadOnlyDictionary<RelationshipAttribute, HashSet<TDependentResource>>,
        IRelationshipsDictionary where TDependentResource : class, IIdentifiable
    { }

    /// <summary>
    /// A helper class that provides insights in which relationships have been updated for which entities.
    /// </summary>
    public interface IRelationshipGetters<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Gets a dictionary of all entities that have an affected relationship to type <typeparamref name="TPrincipalResource"/>
        /// </summary>
        Dictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship<TRelatedResource>() where TRelatedResource : class, IIdentifiable;
        /// <summary>
        /// Gets a dictionary of all entities that have an affected relationship to type <paramref name="principalType"/>
        /// </summary>
        Dictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship(Type relatedResourceType);
        /// <summary>
        /// Gets a collection of all the entities for the property within <paramref name="NavigationAction"/>
        /// has been affected by the request
        /// </summary>
        /// <param name="NavigationAction"></param>
        HashSet<TResource> GetAffected(Expression<Func<TResource, object>> NavigationAction);
    }


    /// <summary>
    /// Implementation of IAffectedRelationships{TDependentResource}
    /// 
    /// It is practically a ReadOnlyDictionary{RelationshipAttribute, HashSet{TDependentResource}} dictionary
    /// with the two helper methods defined on IAffectedRelationships{TDependentResource}.
    /// </summary>
    public class RelationshipsDictionary<TResource> :
        Dictionary<RelationshipAttribute, HashSet<TResource>>,
        IRelationshipsDictionary<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:JsonApiDotNetCore.Hooks.RelationshipsDictionary`1"/> class.
        /// </summary>
        /// <param name="relationships">Relationships.</param>
        public RelationshipsDictionary(Dictionary<RelationshipAttribute, HashSet<TResource>> relationships) : base(relationships) { }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make live a bit easier with generics
        /// </summary>
        internal RelationshipsDictionary(Dictionary<RelationshipAttribute, IEnumerable> relationships)
            : this(TypeHelper.ConvertRelationshipDictionary<TResource>(relationships)) { }

        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship<TRelatedResource>() where TRelatedResource : class, IIdentifiable
        {
            return GetByRelationship(typeof(TRelatedResource));
        }

        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship(Type relatedType)
        {
            return this.Where(p => p.Key.DependentType == relatedType).ToDictionary(p => p.Key, p => p.Value);
        }

        /// <inheritdoc />
        public HashSet<TResource> GetAffected(Expression<Func<TResource, object>> NavigationAction)
        {
            var property = TypeHelper.ParseNavigationExpression(NavigationAction);
            return this.Where(p => p.Key.InternalRelationshipName == property.Name).Select(p => p.Value).SingleOrDefault();
        }
    }
}
