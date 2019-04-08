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

        /// <inheritdoc/>
        public virtual T BeforeCreate(T entity, ResourceAction actionSource)
        {
            return entity;
        }

        /// <inheritdoc/>
        public virtual T AfterCreate(T entity, ResourceAction actionSource)
        {
            return entity;
        }

        /// <inheritdoc/>
        public virtual void BeforeRead(ResourceAction actionSource, string stringId = null)
        {
            return;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<T> AfterRead(IEnumerable<T> entities, ResourceAction actionSource)
        {
            return entities;
        }

        /// <inheritdoc/>
        public virtual T BeforeUpdate(T entity, ResourceAction actionSource)
        {
            return entity;
        }

        /// <inheritdoc/>
        public virtual T AfterUpdate(T entity, ResourceAction actionSource)
        {
            return entity;
        }

        /// <inheritdoc/>
        public virtual void BeforeDelete(T entity, ResourceAction actionSource)
        {
            return;
        }

        /// <inheritdoc/>
        public virtual void AfterDelete(T entity, bool succeeded, ResourceAction actionSource)
        {
            return;
        }

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
