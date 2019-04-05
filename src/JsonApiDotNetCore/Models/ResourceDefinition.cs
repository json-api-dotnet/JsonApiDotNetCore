using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JsonApiDotNetCore.Models
{

    public interface IResourceDefinition
    {
        List<AttrAttribute> GetOutputAttrs(object instance);
    }


    /// <summary>
    /// exposes developer friendly hooks into how their resources are exposed. 
    /// It is intended to improve the experience and reduce boilerplate for commonly required features.
    /// The goal of this class is to reduce the frequency with which developers have to override the
    /// service and repository layers.
    /// </summary>
    /// <typeparam name="T">The resource type</typeparam>
    public class ResourceDefinition<T> : IResourceDefinition, IResourceHookContainer<T> where T : class, IIdentifiable
    {
        private readonly IResourceGraph _graph;
        private readonly ContextEntity _contextEntity;
        internal readonly bool _instanceAttrsAreSpecified;

        private bool _requestCachedAttrsHaveBeenLoaded = false;
        private List<AttrAttribute> _requestCachedAttrs;

        public ResourceDefinition()
        {
            _graph = ResourceGraph.Instance;
            _contextEntity = ResourceGraph.Instance.GetContextEntity(typeof(T));
            _instanceAttrsAreSpecified = InstanceOutputAttrsAreSpecified();
        }

        private bool InstanceOutputAttrsAreSpecified()
        {
            var derivedType = GetType();
            var methods = derivedType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            var instanceMethod = methods
                .Where(m =>
                   m.Name == nameof(OutputAttrs)
                   && m.GetParameters()
                        .FirstOrDefault()
                        ?.ParameterType == typeof(T))
                .FirstOrDefault();
            var declaringType = instanceMethod?.DeclaringType;
            return declaringType == derivedType;
        }

        /// <summary>
        /// Remove an attribute
        /// </summary>
        /// <param name="filter">the filter to execute</param>
        /// <param name="from">@TODO</param>
        /// <returns></returns>
        protected List<AttrAttribute> Remove(Expression<Func<T, dynamic>> filter, List<AttrAttribute> from = null)
        {
            //@TODO: need to investigate options for caching these
            from = from ?? _contextEntity.Attributes;

            // model => model.Attribute
            if (filter.Body is MemberExpression memberExpression)
                return _contextEntity.Attributes
                        .Where(a => a.InternalAttributeName != memberExpression.Member.Name)
                        .ToList();

            // model => new { model.Attribute1, model.Attribute2 }
            if (filter.Body is NewExpression newExpression)
            {
                var attributes = new List<AttrAttribute>();
                foreach (var attr in _contextEntity.Attributes)
                    if (newExpression.Members.Any(m => m.Name == attr.InternalAttributeName) == false)
                        attributes.Add(attr);

                return attributes;
            }

            throw new JsonApiException(500,
                message: $"The expression returned by '{filter}' for '{GetType()}' is of type {filter.Body.GetType()}"
                        + " and cannot be used to select resource attributes. ",
                detail: "The type must be a NewExpression. Example: article => new { article.Author }; ");
        }

        /// <summary>
        /// Allows POST / PATCH requests to set the value of an
        /// attribute, but exclude the attribute in the response
        /// this might be used if the incoming value gets hashed or
        /// encrypted prior to being persisted and this value should
        /// never be sent back to the client.
        ///
        /// Called once per filtered resource in request.
        /// </summary>
        protected virtual List<AttrAttribute> OutputAttrs() => _contextEntity.Attributes;

        /// <summary>
        /// Allows POST / PATCH requests to set the value of an
        /// attribute, but exclude the attribute in the response
        /// this might be used if the incoming value gets hashed or
        /// encrypted prior to being persisted and this value should
        /// never be sent back to the client.
        ///
        /// Called for every instance of a resource.
        /// </summary>
        protected virtual List<AttrAttribute> OutputAttrs(T instance) => _contextEntity.Attributes;

        public List<AttrAttribute> GetOutputAttrs(object instance)
            => _instanceAttrsAreSpecified == false
                ? GetOutputAttrs()
                : OutputAttrs(instance as T);

        private List<AttrAttribute> GetOutputAttrs()
        {
            if (_requestCachedAttrsHaveBeenLoaded == false)
            {
                _requestCachedAttrs = OutputAttrs();
                // the reason we don't just check for null is because we
                // guarantee that OutputAttrs will be called once per
                // request and null is a valid return value
                _requestCachedAttrsHaveBeenLoaded = true;
            }

            return _requestCachedAttrs;
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

        ///// <summary>
        ///// Executed when listing all resources
        ///// </summary>
        ///// <param name="entities"></param>
        ///// <returns></returns>
        //public virtual IEnumerable<T> OnList(List<T> entities, int index) => entities;
        //public virtual IEnumerable<T> OnList(HashSet<T> entities) => entities;


        // GET HOOKS
        /// <summary>
        /// @TODO: should query params be passed along to allow fo authorization on requested relations?
        /// A hook executed before getting entities. Can be used eg. for authorization.
        /// </summary>
        public virtual void BeforeGet() { }
        /// <summary>
        /// A hook executed after getting entities. Can be used eg. for publishing events.
        /// 
        /// Can also be used to to filter on the result set of a custom include. For example,
        /// if Articles -> Blogs -> Tags are retrieved, the AfterGet method as defined in
        /// in all related ResourceDefinitions (if present) will be called: 
        ///     * first for all articles;
        ///     * then for all blogs;
        ///     * lastly for all tags. 
        /// This can be used to build an in-memory filtered include, which is not yet suported by EF Core, 
        /// <see href="https://github.com/aspnet/EntityFrameworkCore/issues/1833">see this issue</see>.
        /// </summary>
        /// <returns>The (adjusted) entities that result from the query</returns>
        /// <param name="entities">The entities that result from the query</param>
        public virtual IEnumerable<T> AfterGet(List<T> entities) => entities;
        /// <summary>
        /// @TODO: should query params be passed along to allow fo authorization on requested relations?
        /// A hook executed before getting an individual entity. Can be used eg. for authorization.
        /// 
        /// @TODO Instead of <paramref name="stringId"/> it would be better to have 
        /// a generic {TId}, but this will requre to change ResourceDefinition{T} into
        /// ResourceDefinition{T, TId}. This is probaly better, but not doing this now here
        /// to keep it simple.
        /// </summary>
        /// <param name="stringId">String identifier of the entity to be retrieved</param>
        public virtual void BeforeGetSingle(string stringId) { }
        /// <summary>
        /// A hook executed after getting an individual. Can be used eg. for publishing events.
        /// 
        /// Can also be used to to filter on the result set of a custom include. For example,
        /// if Articls -> Blogs -> Tags are retrieved, the AfterGet() method as defined in
        /// in all related ResourceDefinitions (if present) will be called: 
        ///     * first for the retrieved article;
        ///     * then for all blogs;
        ///     * lastly for all tags. 
        /// This can be used to build an in-memory filtered include, which is not yet suported by EF Core, 
        /// <see href="https://github.com/aspnet/EntityFrameworkCore/issues/1833">see this issue</see>.
        /// </summary>
        /// <returns>The (adjusted) entity that result from the query</returns>
        /// <param name="entity">The entity that result from the query</param>
        public virtual T AfterGetSingle(T entity) => entity;

        /// <summary>
        /// @TODO [THIS IS FOR LATER]
        /// As soon as the <see href="https://github.com/aspnet/EntityFrameworkCore/issues/1833">filtered include issue</see>
        /// has been resolved, we start building the relationship inclusion expression tree ourselves, 
        /// and we can allow for a "during query" hook like this to allow for in-sql filtering.
        /// 
        /// For now, we are bound to use AfterGet() to achieve the same in-memory after the query has been executed.
        /// 
        /// For now we will not expose this method on <see cref="IResourceDefinition"/>.
        /// </summary>
        /// <param name="entities">Entities.</param>
        public virtual IQueryable<T> OnQueryGet(IQueryable<T> entities) => entities;
        

        // CREATE HOOKS
        /// <summary>
        /// A hook executed before creating an entity. Can be used eg. for authorization.
        /// If the entity also contains to be created relationships, the BeforeUpdateRelationships()
        /// methods on the ResourceDefinition (if implemented) of the entities associated to these relationships
        /// will also be called
        /// @TODO dubble check if BeforeUpdateRelationships should really be called and how 
        /// </summary>
        /// <returns>The (adjusted) entity to be created</returns>
        /// <param name="entity">The entity to be created</param>
        public virtual T BeforeCreate(T entity) => entity;
        /// <summary>
        /// A hook executed after creating an entity. Can be used eg. for publishing events.
        /// </summary>
        /// <param name="entity">The entity that was created</param>
        public virtual void AfterCreate(T entity) { }

        // UPDATE HOOKS
        /// <summary>
        /// A hook executed before updating an entity. Can be used eg. for authorization.
        /// If the entity also contains to be updated relationships, the BeforeUpdateRelationships()
        /// methods on the ResourceDefinition (if implemented) of the entities associated to these relationships
        /// will also be called.
        /// @TODO dubble check if BeforeUpdateRelationships should really be called and how 
        /// </summary>
        /// <returns>The (adjusted) entity to be updated</returns>
        /// <param name="entity">The entity to be updated</param>
        public virtual T BeforeUpdate(T entity) => entity;
        /// <summary>
        /// A hook executed after updating an entity. Can be used eg. for publishing events.
        /// </summary>
        /// <param name="entity">The entity that was updated</param>
        public virtual void AfterUpdate(T entity) { }

        // DELETE HOOKS
        /// <summary>
        /// A hook executed before deleting an entity. Can be used eg. for authorization.
        /// </summary>
        /// <param name="entity">The entity to be updated</param>
        public virtual void BeforeDelete(T entity) { }
        /// <summary>
        /// A hook executed after deleting an entity. Can be used eg. for publishing events.
        /// </summary>
        /// <param name="entity">The entity that was deleted</param>
        public virtual void AfterDelete(T entity) { }


        // GET RELATIONSHIPS HOOKS
        /// <summary>
        /// A hook executed before getting a relationship of a particular entity. 
        /// Can be used eg. for authorization.
        /// 
        /// @TODO it would make more sense to include the actual entity here instead of <paramref name="stringId"/>,
        /// but this would require an new/additional query in <see cref="IResourceService{T}" />
        /// </summary>
        /// <param name="stringId">The id of the "parent" entity</param>
        /// <param name="relationshipName">Name of the relationship</param>
        public virtual void BeforeGetRelationship(string stringId, string relationshipName) { }
        /// <summary>
        /// A hook executed after getting a relationship of a particular entity. 
        /// Can be used eg. for publishing events.
        /// 
        /// Can be used to construct filtered include, similar to AfterGetSingle().
        /// 
        /// @TODO: we need to think on how to implement this. Maybe we shoud 
        /// give user access to parsed relationship object instead of the "parent" 
        /// entity (the one T where T.Id == stringId)? 
        /// </summary>
        /// <returns>The (adjusted) parent entity that contains the requested relationship</returns>
        /// <param name="entity">The parent entity that contains the requested relationship</param>
        public virtual T AfterGetRelationship(T entity) => entity;

        // UPDATE RELATIONSHIPS HOOKS
        /// <summary>
        /// A hook executed before updating a relationship of a particular entity. 
        /// @TODO we need to check if it makes sense to expose List{object} relationships
        /// to the hook.
        /// </summary>
        /// <param name="entity">The "parent" entity of which the relationship is to be updated</param>
        /// <param name="relationshipName">The name of the relationship to be updated</param>
        /// <param name="relationships">The objects which represent the updated relationships (does this make sense to include?)</param>
        public virtual void BeforeUpdateRelationships(T entity, string relationshipName, List<object> relationships) { }
        /// <summary>
        /// A hook executed after updating a relationship of a particular entity. 
        /// @TODO we need to check if it makes sense to expose List{object} relationships
        /// to the hook.
        /// </summary>
        /// <param name="entity">The "parent" entity of which the relationship is to be updated</param>
        /// <param name="relationshipName">The name of the relationship to be updated</param>
        /// <param name="relationships">The objects which represent the updated relationships (does this make sense to include?)</param>
        public virtual void AfterUpdateRelationships(T entity, string relationshipName, List<object> relationships) { }



        /// <summary>
        /// This is an alias type intended to simplify the implementation's
        /// method signature.
        /// See <see cref="GetQueryFilters" /> for usage details.
        /// </summary>
        public class QueryFilters : Dictionary<string, Func<IQueryable<T>, string, IQueryable<T>>> { }

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
        protected virtual PropertySortOrder GetDefaultSortOrder() => null;

        internal List<(AttrAttribute, SortDirection)> DefaultSort()
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
        public class PropertySortOrder : List<(Expression<Func<T, dynamic>>, SortDirection)> { }
    }
}
