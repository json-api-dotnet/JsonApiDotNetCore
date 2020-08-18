using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Queries.Expressions;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// Exposes developer friendly hooks into how their resources are exposed. 
    /// It is intended to improve the experience and reduce boilerplate for commonly required features.
    /// The goal of this class is to reduce the frequency with which developers have to override the
    /// service and repository layers.
    /// </summary>
    /// <typeparam name="TResource">The resource type</typeparam>
    public class ResourceDefinition<TResource> : IResourceDefinition, IResourceHookContainer<TResource> where TResource : class, IIdentifiable
    {
        protected IResourceGraph ResourceGraph { get; }

        public ResourceDefinition(IResourceGraph resourceGraph)
        {
            ResourceGraph = resourceGraph ?? throw new ArgumentNullException(nameof(resourceGraph));
        }

        /// <inheritdoc/>
        public virtual void AfterCreate(HashSet<TResource> resources, ResourcePipeline pipeline) { }
        /// <inheritdoc/>
        public virtual void AfterRead(HashSet<TResource> resources, ResourcePipeline pipeline, bool isIncluded = false) { }
        /// <inheritdoc/>
        public virtual void AfterUpdate(HashSet<TResource> resources, ResourcePipeline pipeline) { }
        /// <inheritdoc/>
        public virtual void AfterDelete(HashSet<TResource> resources, ResourcePipeline pipeline, bool succeeded) { }
        /// <inheritdoc/>
        public virtual void AfterUpdateRelationship(IRelationshipsDictionary<TResource> resourcesByRelationship, ResourcePipeline pipeline) { }
        /// <inheritdoc/>
        public virtual IEnumerable<TResource> BeforeCreate(IResourceHashSet<TResource> resources, ResourcePipeline pipeline) { return resources; }
        /// <inheritdoc/>
        public virtual void BeforeRead(ResourcePipeline pipeline, bool isIncluded = false, string stringId = null) { }
        /// <inheritdoc/>
        public virtual IEnumerable<TResource> BeforeUpdate(IDiffableResourceHashSet<TResource> resources, ResourcePipeline pipeline) { return resources; }
        /// <inheritdoc/>
        public virtual IEnumerable<TResource> BeforeDelete(IResourceHashSet<TResource> resources, ResourcePipeline pipeline) { return resources; }
        /// <inheritdoc/>
        public virtual IEnumerable<string> BeforeUpdateRelationship(HashSet<string> ids, IRelationshipsDictionary<TResource> resourcesByRelationship, ResourcePipeline pipeline) { return ids; }
        /// <inheritdoc/>
        public virtual void BeforeImplicitUpdateRelationship(IRelationshipsDictionary<TResource> resourcesByRelationship, ResourcePipeline pipeline) { }
        /// <inheritdoc/>
        public virtual IEnumerable<TResource> OnReturn(HashSet<TResource> resources, ResourcePipeline pipeline) { return resources; }

        /// <summary>
        /// Enables to extend, replace or remove includes that are being applied on this resource type.
        /// </summary>
        /// <param name="existingIncludes">
        /// An optional existing set of includes, coming from query string. Never <c>null</c>, but may be empty.
        /// </param>
        /// <returns>
        /// The new set of includes. Return an empty collection to remove all inclusions (never return <c>null</c>).
        /// </returns>
        public virtual IReadOnlyCollection<IncludeElementExpression> OnApplyIncludes(IReadOnlyCollection<IncludeElementExpression> existingIncludes)
        {
            return existingIncludes;
        }

        /// <summary>
        /// Enables to extend, replace or remove a filter that is being applied on a set of this resource type.
        /// </summary>
        /// <param name="existingFilter">
        /// An optional existing filter, coming from query string. Can be <c>null</c>.
        /// </param>
        /// <returns>
        /// The new filter, or <c>null</c> to disable the existing filter.
        /// </returns>
        public virtual FilterExpression OnApplyFilter(FilterExpression existingFilter)
        {
            return existingFilter;
        }

        /// <summary>
        /// Enables to extend, replace or remove a sort order that is being applied on a set of this resource type.
        /// Tip: Use <see cref="CreateSortExpressionFromLambda"/> to build from a lambda expression.
        /// </summary>
        /// <param name="existingSort">
        /// An optional existing sort order, coming from query string. Can be <c>null</c>.
        /// </param>
        /// <returns>
        /// The new sort order, or <c>null</c> to disable the existing sort order and sort by ID.
        /// </returns>
        public virtual SortExpression OnApplySort(SortExpression existingSort)
        {
            return existingSort;
        }

        /// <summary>
        /// Creates a <see cref="SortExpression"/> from a lambda expression.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// var sort = CreateSortExpressionFromLambda(new PropertySortOrder
        /// {
        ///     (model => model.CreatedAt, ListSortDirection.Ascending),
        ///     (model => model.Password, ListSortDirection.Descending)
        /// });
        /// ]]></code>
        /// </example>
        protected SortExpression CreateSortExpressionFromLambda(PropertySortOrder keySelectors)
        {
            if (keySelectors == null)
            {
                throw new ArgumentNullException(nameof(keySelectors));
            }

            List<SortElementExpression> sortElements = new List<SortElementExpression>();

            foreach (var (keySelector, sortDirection) in keySelectors)
            {
                bool isAscending = sortDirection == ListSortDirection.Ascending;
                var attribute = ResourceGraph.GetAttributes(keySelector).Single();

                var sortElement = new SortElementExpression(new ResourceFieldChainExpression(attribute), isAscending);
                sortElements.Add(sortElement);
            }

            return new SortExpression(sortElements);
        }

        /// <summary>
        /// Enables to extend, replace or remove pagination that is being applied on a set of this resource type.
        /// </summary>
        /// <param name="existingPagination">
        /// An optional existing pagination, coming from query string. Can be <c>null</c>.
        /// </param>
        /// <returns>
        /// The changed pagination, or <c>null</c> to use the first page with default size from options.
        /// To disable paging, set <see cref="PaginationExpression.PageSize"/> to <c>null</c>.
        /// </returns>
        public virtual PaginationExpression OnApplyPagination(PaginationExpression existingPagination)
        {
            return existingPagination;
        }

        /// <summary>
        /// Enables to extend, replace or remove a sparse fieldset that is being applied on a set of this resource type.
        /// Tip: Use <see cref="SparseFieldSetExpressionExtensions.Including{TResource}"/> and <see cref="SparseFieldSetExpressionExtensions.Excluding{TResource}"/>
        /// to safely change the fieldset without worrying about nulls.
        /// </summary>
        /// <remarks>
        /// This method executes twice for a single request: first to select which fields to retrieve from the data store and then to
        /// select which fields to serialize. Including extra fields from this method will retrieve them, but not include them in the json output.
        /// This enables you to expose calculated properties whose value depends on a field that is not in the sparse fieldset.
        /// </remarks>
        /// <param name="existingSparseFieldSet">The incoming sparse fieldset from query string.
        /// At query execution time, this is <c>null</c> if the query string contains no sparse fieldset.
        /// At serialization time, this contains all viewable fields if the query string contains no sparse fieldset.
        /// </param>
        /// <returns>
        /// The new sparse fieldset, or <c>null</c> to discard the existing sparse fieldset and select all viewable fields.
        /// </returns>
        public virtual SparseFieldSetExpression OnApplySparseFieldSet(SparseFieldSetExpression existingSparseFieldSet)
        {
            return existingSparseFieldSet;
        }
        
        /// <summary>
        /// Enables to adapt the Entity Framework Core <see cref="IQueryable{TResource}"/> query, based on custom query string parameters.
        /// Note this only works on primary resource requests, such as /articles, but not on /blogs/1/articles or /blogs?include=articles.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// protected override QueryStringParameterHandlers OnRegisterQueryableHandlersForQueryStringParameters()
        /// {
        ///     return new QueryStringParameterHandlers
        ///     {
        ///         ["isActive"] = (source, parameterValue) => source
        ///             .Include(model => model.Children)
        ///             .Where(model => model.LastUpdateTime > DateTime.Now.AddMonths(-1)),
        ///         ["isHighRisk"] = FilterByHighRisk
        ///     };
        /// }
        ///
        /// private static IQueryable<Model> FilterByHighRisk(IQueryable<Model> source, StringValues parameterValue)
        /// {
        ///     bool isFilterOnHighRisk = bool.Parse(parameterValue);
        ///     return isFilterOnHighRisk ? source.Where(model => model.RiskLevel >= 5) : source.Where(model => model.RiskLevel < 5);
        /// }
        /// ]]></code>
        /// </example>
        /// <returns></returns>
        protected virtual QueryStringParameterHandlers OnRegisterQueryableHandlersForQueryStringParameters()
        {
            return new QueryStringParameterHandlers();
        }

        public object GetQueryableHandlerForQueryStringParameter(string parameterName)
        {
            var handlers = OnRegisterQueryableHandlersForQueryStringParameters();
            return handlers != null && handlers.ContainsKey(parameterName) ? handlers[parameterName] : null;
        }

        /// <summary>
        /// This is an alias type intended to simplify the implementation's method signature.
        /// See <see cref="ResourceDefinition{TResource}.CreateSortExpressionFromLambda"/> for usage details.
        /// </summary>
        public sealed class PropertySortOrder : List<(Expression<Func<TResource, dynamic>> KeySelector, ListSortDirection SortDirection)>
        {
        }

        /// <summary>
        /// This is an alias type intended to simplify the implementation's method signature.
        /// See <see cref="OnRegisterQueryableHandlersForQueryStringParameters"/> for usage details.
        /// </summary>
        public sealed class QueryStringParameterHandlers : Dictionary<string, Func<IQueryable<TResource>, StringValues, IQueryable<TResource>>>
        {
        }
    }
}
