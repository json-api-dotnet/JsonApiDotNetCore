using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Models
{
    public interface IResourceDefinition
    {
        List<AttrAttribute> GetAllowedAttributes();
        List<RelationshipAttribute> GetAllowedRelationships();
    }

    /// <summary>
    /// exposes developer friendly hooks into how their resources are exposed. 
    /// It is intended to improve the experience and reduce boilerplate for commonly required features.
    /// The goal of this class is to reduce the frequency with which developers have to override the
    /// service and repository layers.
    /// </summary>
    /// <typeparam name="TResource">The resource type</typeparam>
    public class ResourceDefinition<TResource> : IResourceDefinition, IResourceHookContainer<TResource> where TResource : class, IIdentifiable
    {
        private readonly ContextEntity _contextEntity;
        private readonly IFieldsExplorer _fieldExplorer;
        private List<AttrAttribute> _allowedAttributes;
        private List<RelationshipAttribute> _allowedRelationships;
        public ResourceDefinition(IFieldsExplorer fieldExplorer, IResourceGraph graph)
        {
            _contextEntity = graph.GetContextEntity(typeof(TResource));
            _allowedAttributes = _contextEntity.Attributes;
            _allowedRelationships = _contextEntity.Relationships;
            _fieldExplorer = fieldExplorer;
        }

        public ResourceDefinition(IResourceGraph graph)
        {
            _allowedAttributes = _contextEntity.Attributes;
            _allowedRelationships = _contextEntity.Relationships;
            _contextEntity = graph.GetContextEntity(typeof(TResource));
        }

        public List<RelationshipAttribute> GetAllowedRelationships() => _allowedRelationships;
        public List<AttrAttribute> GetAllowedAttributes() => _allowedAttributes;

        /// <summary>
        /// Allows POST / PATCH requests to set the value of an
        /// attribute, but exclude the attribute in the response
        /// this might be used if the incoming value gets hashed or
        /// encrypted prior to being persisted and this value should
        /// never be sent back to the client.
        ///
        /// Called once per filtered resource in request.
        /// </summary>
        public void HideAttributes(Expression<Func<TResource, dynamic>> selector)
        {
            var attributesToHide = _fieldExplorer.GetAttributes(selector);
            _allowedAttributes = _allowedAttributes.Except(attributesToHide).ToList();
        }
        public void HideRelationships(Expression<Func<TResource, dynamic>> selector)
        {
            var relationshipsToHide = _fieldExplorer.GetRelationships(selector);
            _allowedRelationships = _allowedRelationships.Except(relationshipsToHide).ToList();
        }


        /// <summary>
        /// Define a set of custom query expressions that can be applied
        /// instead of the default query behavior. A common use-case for this
        /// is including related resources and filtering on them.
        /// </summary>
        ///
        /// <returns>
        /// A set of custom queries that will be applied instead of the default
        /// queries for the given key. Null will be returned if default behavior
        /// is desired.
        /// </returns>
        ///
        /// <example>
        /// <code>
        /// protected override QueryFilters GetQueryFilters() =>  { 
        ///     { "facility", (t, value) => t.Include(t => t.Tenant)
        ///                                   .Where(t => t.Facility == value) }
        ///  }
        /// </code>
        /// 
        /// If the logic is simply too complex for an in-line expression, you can
        /// delegate to a private method:
        /// <code>
        /// protected override QueryFilters GetQueryFilters()
        ///     => new QueryFilters {
        ///         { "is-active", FilterIsActive }
        ///     };
        /// 
        /// private IQueryable&lt;Model&gt; FilterIsActive(IQueryable&lt;Model&gt; query, string value)
        /// {
        ///     // some complex logic goes here...
        ///     return query.Where(x => x.IsActive == computedValue);
        /// }
        /// </code>
        /// </example>
        public virtual QueryFilters GetQueryFilters() => null;

        /// <inheritdoc/>
        public virtual void AfterCreate(HashSet<TResource> entities, ResourcePipeline pipeline) { }
        /// <inheritdoc/>
        public virtual void AfterRead(HashSet<TResource> entities, ResourcePipeline pipeline, bool isIncluded = false) { }
        /// <inheritdoc/>
        public virtual void AfterUpdate(HashSet<TResource> entities, ResourcePipeline pipeline) { }
        /// <inheritdoc/>
        public virtual void AfterDelete(HashSet<TResource> entities, ResourcePipeline pipeline, bool succeeded) { }
        /// <inheritdoc/>
        public virtual void AfterUpdateRelationship(IRelationshipsDictionary<TResource> entitiesByRelationship, ResourcePipeline pipeline) { }
        /// <inheritdoc/>
        public virtual IEnumerable<TResource> BeforeCreate(IEntityHashSet<TResource> entities, ResourcePipeline pipeline) { return entities; }
        /// <inheritdoc/>
        public virtual void BeforeRead(ResourcePipeline pipeline, bool isIncluded = false, string stringId = null) { }
        /// <inheritdoc/>
        public virtual IEnumerable<TResource> BeforeUpdate(IDiffableEntityHashSet<TResource> entities, ResourcePipeline pipeline) { return entities; }
        /// <inheritdoc/>
        public virtual IEnumerable<TResource> BeforeDelete(IEntityHashSet<TResource> entities, ResourcePipeline pipeline) { return entities; }
        /// <inheritdoc/>
        public virtual IEnumerable<string> BeforeUpdateRelationship(HashSet<string> ids, IRelationshipsDictionary<TResource> entitiesByRelationship, ResourcePipeline pipeline) { return ids; }
        /// <inheritdoc/>
        public virtual void BeforeImplicitUpdateRelationship(IRelationshipsDictionary<TResource> entitiesByRelationship, ResourcePipeline pipeline) { }
        /// <inheritdoc/>
        public virtual IEnumerable<TResource> OnReturn(HashSet<TResource> entities, ResourcePipeline pipeline) { return entities; }


        /// <summary>
        /// This is an alias type intended to simplify the implementation's
        /// method signature.
        /// See <see cref="GetQueryFilters" /> for usage details.
        /// </summary>
        public class QueryFilters : Dictionary<string, Func<IQueryable<TResource>, FilterQuery, IQueryable<TResource>>> { }

        /// <summary>
        /// Define a the default sort order if no sort key is provided.
        /// </summary>
        /// <returns>
        /// A list of properties and the direction they should be sorted.
        /// </returns>
        /// <example>
        /// <code>
        /// protected override PropertySortOrder GetDefaultSortOrder()
        ///     => new PropertySortOrder {
        ///         (t => t.Prop1, SortDirection.Ascending),
        ///         (t => t.Prop2, SortDirection.Descending),
        ///     };
        /// </code>
        /// </example>
        public virtual PropertySortOrder GetDefaultSortOrder() => null;

        public List<(AttrAttribute, SortDirection)> DefaultSort()
        {
            var defaultSortOrder = GetDefaultSortOrder();
            if (defaultSortOrder != null && defaultSortOrder.Count > 0)
            {
                var order = new List<(AttrAttribute, SortDirection)>();
                foreach (var sortProp in defaultSortOrder)
                {
                    // TODO: error handling, log or throw?
                    if (sortProp.Item1.Body is MemberExpression memberExpression)
                        order.Add(
                            (_contextEntity.Attributes.SingleOrDefault(a => a.InternalAttributeName != memberExpression.Member.Name),
                            sortProp.Item2)
                        );
                }

                return order;
            }

            return null;
        }


        /// <summary>
        /// This is an alias type intended to simplify the implementation's
        /// method signature.
        /// See <see cref="GetQueryFilters" /> for usage details.
        /// </summary>
        public class PropertySortOrder : List<(Expression<Func<TResource, dynamic>>, SortDirection)> { }
    }
}
