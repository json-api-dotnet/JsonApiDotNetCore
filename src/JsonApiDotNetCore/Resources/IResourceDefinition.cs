using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// Provides an extensibility point to add business logic that is resource-oriented instead of endpoint-oriented.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource type.
    /// </typeparam>
    [PublicAPI]
    public interface IResourceDefinition<TResource> : IResourceDefinition<TResource, int>
        where TResource : class, IIdentifiable<int>
    {
    }

    /// <summary>
    /// Provides an extensibility point to add business logic that is resource-oriented instead of endpoint-oriented.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource type.
    /// </typeparam>
    /// <typeparam name="TId">
    /// The resource identifier type.
    /// </typeparam>
    [PublicAPI]
    public interface IResourceDefinition<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Enables to extend, replace or remove includes that are being applied on this resource type.
        /// </summary>
        /// <param name="existingIncludes">
        /// An optional existing set of includes, coming from query string. Never <c>null</c>, but may be empty.
        /// </param>
        /// <returns>
        /// The new set of includes. Return an empty collection to remove all inclusions (never return <c>null</c>).
        /// </returns>
        IImmutableSet<IncludeElementExpression> OnApplyIncludes(IImmutableSet<IncludeElementExpression> existingIncludes);

        /// <summary>
        /// Enables to extend, replace or remove a filter that is being applied on a set of this resource type.
        /// </summary>
        /// <param name="existingFilter">
        /// An optional existing filter, coming from query string. Can be <c>null</c>.
        /// </param>
        /// <returns>
        /// The new filter, or <c>null</c> to disable the existing filter.
        /// </returns>
        FilterExpression OnApplyFilter(FilterExpression existingFilter);

        /// <summary>
        /// Enables to extend, replace or remove a sort order that is being applied on a set of this resource type. Tip: Use
        /// <see cref="JsonApiResourceDefinition{TResource, TId}.CreateSortExpressionFromLambda" /> to build from a lambda expression.
        /// </summary>
        /// <param name="existingSort">
        /// An optional existing sort order, coming from query string. Can be <c>null</c>.
        /// </param>
        /// <returns>
        /// The new sort order, or <c>null</c> to disable the existing sort order and sort by ID.
        /// </returns>
        SortExpression OnApplySort(SortExpression existingSort);

        /// <summary>
        /// Enables to extend, replace or remove pagination that is being applied on a set of this resource type.
        /// </summary>
        /// <param name="existingPagination">
        /// An optional existing pagination, coming from query string. Can be <c>null</c>.
        /// </param>
        /// <returns>
        /// The changed pagination, or <c>null</c> to use the first page with default size from options. To disable paging, set
        /// <see cref="PaginationExpression.PageSize" /> to <c>null</c>.
        /// </returns>
        PaginationExpression OnApplyPagination(PaginationExpression existingPagination);

        /// <summary>
        /// Enables to extend, replace or remove a sparse fieldset that is being applied on a set of this resource type. Tip: Use
        /// <see cref="SparseFieldSetExpressionExtensions.Including{TResource}" /> and <see cref="SparseFieldSetExpressionExtensions.Excluding{TResource}" /> to
        /// safely change the fieldset without worrying about nulls.
        /// </summary>
        /// <remarks>
        /// This method executes twice for a single request: first to select which fields to retrieve from the data store and then to select which fields to
        /// serialize. Including extra fields from this method will retrieve them, but not include them in the json output. This enables you to expose calculated
        /// properties whose value depends on a field that is not in the sparse fieldset.
        /// </remarks>
        /// <param name="existingSparseFieldSet">
        /// The incoming sparse fieldset from query string. At query execution time, this is <c>null</c> if the query string contains no sparse fieldset. At
        /// serialization time, this contains all viewable fields if the query string contains no sparse fieldset.
        /// </param>
        /// <returns>
        /// The new sparse fieldset, or <c>null</c> to discard the existing sparse fieldset and select all viewable fields.
        /// </returns>
        SparseFieldSetExpression OnApplySparseFieldSet(SparseFieldSetExpression existingSparseFieldSet);

        /// <summary>
        /// Enables to adapt the Entity Framework Core <see cref="IQueryable{T}" /> query, based on custom query string parameters. Note this only works on
        /// primary resource requests, such as /articles, but not on /blogs/1/articles or /blogs?include=articles.
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
#pragma warning disable AV1130 // Return type in method signature should be a collection interface instead of a concrete type
        QueryStringParameterHandlers<TResource> OnRegisterQueryableHandlersForQueryStringParameters();
#pragma warning restore AV1130 // Return type in method signature should be a collection interface instead of a concrete type

        /// <summary>
        /// Enables to add JSON:API meta information, specific to this resource.
        /// </summary>
        IDictionary<string, object> GetMeta(TResource resource);

        /// <summary>
        /// Executes after the original version of the resource has been retrieved from the underlying data store, as part of a write request.
        /// <para>
        /// Implementing this method enables to perform validations and make changes to <paramref name="resource" />, before the fields from the request are
        /// copied into it.
        /// </para>
        /// <para>
        /// For POST resource requests, this method is typically used to assign property default values or to set required relationships by side-loading the
        /// related resources and linking them.
        /// </para>
        /// </summary>
        /// <param name="resource">
        /// The original resource retrieved from the underlying data store, or a freshly instantiated resource in case of a POST resource request.
        /// </param>
        /// <param name="writeOperation">
        /// Identifies the logical write operation for which this method was called. Possible values: <see cref="WriteOperationKind.CreateResource" />,
        /// <see cref="WriteOperationKind.UpdateResource" /> and <see cref="WriteOperationKind.SetRelationship" />. Note this intentionally excludes
        /// <see cref="WriteOperationKind.DeleteResource" />, <see cref="WriteOperationKind.AddToRelationship" /> and
        /// <see cref="WriteOperationKind.RemoveFromRelationship" />, because for those endpoints no resource is retrieved upfront.
        /// </param>
        /// <param name="cancellationToken">
        /// Propagates notification that request handling should be canceled.
        /// </param>
        Task OnPrepareWriteAsync(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken);

        /// <summary>
        /// Executes before setting (or clearing) the resource at the right side of a to-one relationship.
        /// <para>
        /// Implementing this method enables to perform validations and change <paramref name="rightResourceId" />, before the relationship is updated.
        /// </para>
        /// </summary>
        /// <param name="leftResource">
        /// The original resource as retrieved from the underlying data store. The indication "left" specifies that <paramref name="hasOneRelationship" /> is
        /// declared on <typeparamref name="TResource" />.
        /// </param>
        /// <param name="hasOneRelationship">
        /// The to-one relationship being set.
        /// </param>
        /// <param name="rightResourceId">
        /// The new resource identifier (or <c>null</c> to clear the relationship), coming from the request.
        /// </param>
        /// <param name="writeOperation">
        /// Identifies the logical write operation for which this method was called. Possible values: <see cref="WriteOperationKind.CreateResource" />,
        /// <see cref="WriteOperationKind.UpdateResource" /> and <see cref="WriteOperationKind.SetRelationship" />.
        /// </param>
        /// <param name="cancellationToken">
        /// Propagates notification that request handling should be canceled.
        /// </param>
        /// <returns>
        /// The replacement resource identifier, or <c>null</c> to clear the relationship. Returns <paramref name="rightResourceId" /> by default.
        /// </returns>
        Task<IIdentifiable> OnSetToOneRelationshipAsync(TResource leftResource, HasOneAttribute hasOneRelationship, IIdentifiable rightResourceId,
            WriteOperationKind writeOperation, CancellationToken cancellationToken);

        /// <summary>
        /// Executes before setting the resources at the right side of a to-many relationship. This replaces on existing set.
        /// <para>
        /// Implementing this method enables to perform validations and make changes to <paramref name="rightResourceIds" />, before the relationship is updated.
        /// </para>
        /// </summary>
        /// <param name="leftResource">
        /// The original resource as retrieved from the underlying data store. The indication "left" specifies that <paramref name="hasManyRelationship" /> is
        /// declared on <typeparamref name="TResource" />.
        /// </param>
        /// <param name="hasManyRelationship">
        /// The to-many relationship being set.
        /// </param>
        /// <param name="rightResourceIds">
        /// The set of resource identifiers to replace any existing set with, coming from the request.
        /// </param>
        /// <param name="writeOperation">
        /// Identifies the logical write operation for which this method was called. Possible values: <see cref="WriteOperationKind.CreateResource" />,
        /// <see cref="WriteOperationKind.UpdateResource" /> and <see cref="WriteOperationKind.SetRelationship" />.
        /// </param>
        /// <param name="cancellationToken">
        /// Propagates notification that request handling should be canceled.
        /// </param>
        Task OnSetToManyRelationshipAsync(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            WriteOperationKind writeOperation, CancellationToken cancellationToken);

        /// <summary>
        /// Executes before adding resources to the right side of a to-many relationship, as part of a POST relationship request.
        /// <para>
        /// Implementing this method enables to perform validations and make changes to <paramref name="rightResourceIds" />, before the relationship is updated.
        /// </para>
        /// </summary>
        /// <param name="leftResourceId">
        /// Identifier of the left resource. The indication "left" specifies that <paramref name="hasManyRelationship" /> is declared on
        /// <typeparamref name="TResource" />.
        /// </param>
        /// <param name="hasManyRelationship">
        /// The to-many relationship being added to.
        /// </param>
        /// <param name="rightResourceIds">
        /// The set of resource identifiers to add to the to-many relationship, coming from the request.
        /// </param>
        /// <param name="cancellationToken">
        /// Propagates notification that request handling should be canceled.
        /// </param>
        Task OnAddToRelationshipAsync(TId leftResourceId, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes before removing resources from the right side of a to-many relationship, as part of a DELETE relationship request.
        /// <para>
        /// Implementing this method enables to perform validations and make changes to <paramref name="rightResourceIds" />, before the relationship is updated.
        /// </para>
        /// </summary>
        /// <param name="leftResource">
        /// The original resource as retrieved from the underlying data store. The indication "left" specifies that <paramref name="hasManyRelationship" /> is
        /// declared on <typeparamref name="TResource" />. Be aware that for performance reasons, not the full relationship is populated, but only the subset of
        /// resources to be removed.
        /// </param>
        /// <param name="hasManyRelationship">
        /// The to-many relationship being removed from.
        /// </param>
        /// <param name="rightResourceIds">
        /// The set of resource identifiers to remove from the to-many relationship, coming from the request.
        /// </param>
        /// <param name="cancellationToken">
        /// Propagates notification that request handling should be canceled.
        /// </param>
        Task OnRemoveFromRelationshipAsync(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes before writing the changed resource to the underlying data store, as part of a write request.
        /// <para>
        /// Implementing this method enables to perform validations and make changes to <paramref name="resource" />, after the fields from the request have been
        /// copied into it.
        /// </para>
        /// <para>
        /// An example usage is to set the last-modification timestamp, overwriting the value from the incoming request.
        /// </para>
        /// <para>
        /// Another use case is to add a notification message to an outbox table, which gets committed along with the resource write in a single transaction (see
        /// https://microservices.io/patterns/data/transactional-outbox.html).
        /// </para>
        /// </summary>
        /// <param name="resource">
        /// The original resource retrieved from the underlying data store (or a freshly instantiated resource in case of a POST resource request), updated with
        /// the changes from the incoming request. Exception: In case <paramref name="writeOperation" /> is <see cref="WriteOperationKind.DeleteResource" />,
        /// <see cref="WriteOperationKind.AddToRelationship" /> or <see cref="WriteOperationKind.RemoveFromRelationship" />, this is an empty object with only
        /// the <see cref="Identifiable{T}.Id" /> property set, because for those endpoints no resource is retrieved upfront.
        /// </param>
        /// <param name="writeOperation">
        /// Identifies the logical write operation for which this method was called. Possible values: <see cref="WriteOperationKind.CreateResource" />,
        /// <see cref="WriteOperationKind.UpdateResource" />, <see cref="WriteOperationKind.DeleteResource" />, <see cref="WriteOperationKind.SetRelationship" />
        /// , <see cref="WriteOperationKind.AddToRelationship" /> and <see cref="WriteOperationKind.RemoveFromRelationship" />.
        /// </param>
        /// <param name="cancellationToken">
        /// Propagates notification that request handling should be canceled.
        /// </param>
        Task OnWritingAsync(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken);

        /// <summary>
        /// Executes after successfully writing the changed resource to the underlying data store, as part of a write request.
        /// <para>
        /// Implementing this method enables to run additional logic, for example enqueue a notification message on a service bus.
        /// </para>
        /// </summary>
        /// <param name="resource">
        /// The resource as written to the underlying data store.
        /// </param>
        /// <param name="writeOperation">
        /// Identifies the logical write operation for which this method was called. Possible values: <see cref="WriteOperationKind.CreateResource" />,
        /// <see cref="WriteOperationKind.UpdateResource" />, <see cref="WriteOperationKind.DeleteResource" />, <see cref="WriteOperationKind.SetRelationship" />
        /// , <see cref="WriteOperationKind.AddToRelationship" /> and <see cref="WriteOperationKind.RemoveFromRelationship" />.
        /// </param>
        /// <param name="cancellationToken">
        /// Propagates notification that request handling should be canceled.
        /// </param>
        Task OnWriteSucceededAsync(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken);

        /// <summary>
        /// Executes after a resource has been deserialized from an incoming request body.
        /// </summary>
        /// <para>
        /// Implementing this method enables to change the incoming resource before it enters an ASP.NET Controller Action method.
        /// </para>
        /// <para>
        /// Changing attributes on <paramref name="resource" /> from this method may break detection of side effects on resource POST/PATCH requests, because
        /// side effect detection considers any changes done from this method to be part of the incoming request body. So setting additional attributes from this
        /// method (that were not sent by the client) are not considered side effects, resulting in incorrectly reporting that there were no side effects.
        /// </para>
        /// <param name="resource">
        /// The deserialized resource.
        /// </param>
        void OnDeserialize(TResource resource);

        /// <summary>
        /// Executes before a (primary or included) resource is serialized into an outgoing response body.
        /// </summary>
        /// <para>
        /// Implementing this method enables to change the returned resource, for example scrub sensitive data or transform returned attribute values.
        /// </para>
        /// <para>
        /// Changing attributes on <paramref name="resource" /> from this method may break detection of side effects on resource POST/PATCH requests. What this
        /// means is that if side effects were detected before, this is not re-evaluated after running this method, so it may incorrectly report side effects if
        /// they were undone by this method.
        /// </para>
        /// <param name="resource">
        /// The serialized resource.
        /// </param>
        void OnSerialize(TResource resource);
    }
}
