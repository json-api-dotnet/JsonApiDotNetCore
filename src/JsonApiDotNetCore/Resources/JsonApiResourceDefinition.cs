using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources;

/// <inheritdoc />
[PublicAPI]
public class JsonApiResourceDefinition<TResource, TId> : IResourceDefinition<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    protected IResourceGraph ResourceGraph { get; }

    /// <summary>
    /// Provides metadata for the resource type <typeparamref name="TResource" />.
    /// </summary>
    protected ResourceType ResourceType { get; }

    public JsonApiResourceDefinition(IResourceGraph resourceGraph)
    {
        ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

        ResourceGraph = resourceGraph;
        ResourceType = resourceGraph.GetResourceType<TResource>();
    }

    /// <inheritdoc />
    public virtual IImmutableSet<IncludeElementExpression> OnApplyIncludes(IImmutableSet<IncludeElementExpression> existingIncludes)
    {
        return existingIncludes;
    }

    /// <inheritdoc />
    public virtual FilterExpression? OnApplyFilter(FilterExpression? existingFilter)
    {
        return existingFilter;
    }

    /// <inheritdoc />
    public virtual SortExpression? OnApplySort(SortExpression? existingSort)
    {
        return existingSort;
    }

    /// <summary>
    /// Creates a <see cref="SortExpression" /> from a lambda expression.
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
        ArgumentGuard.NotNullNorEmpty(keySelectors, nameof(keySelectors));

        ImmutableArray<SortElementExpression>.Builder elementsBuilder = ImmutableArray.CreateBuilder<SortElementExpression>(keySelectors.Count);

        foreach ((Expression<Func<TResource, dynamic?>> keySelector, ListSortDirection sortDirection) in keySelectors)
        {
            bool isAscending = sortDirection == ListSortDirection.Ascending;
            AttrAttribute attribute = ResourceGraph.GetAttributes(keySelector).Single();

            var sortElement = new SortElementExpression(new ResourceFieldChainExpression(attribute), isAscending);
            elementsBuilder.Add(sortElement);
        }

        return new SortExpression(elementsBuilder.ToImmutable());
    }

    /// <inheritdoc />
    public virtual PaginationExpression? OnApplyPagination(PaginationExpression? existingPagination)
    {
        return existingPagination;
    }

    /// <inheritdoc />
    public virtual SparseFieldSetExpression? OnApplySparseFieldSet(SparseFieldSetExpression? existingSparseFieldSet)
    {
        return existingSparseFieldSet;
    }

    /// <inheritdoc />
    public virtual QueryStringParameterHandlers<TResource>? OnRegisterQueryableHandlersForQueryStringParameters()
    {
        return null;
    }

    /// <inheritdoc />
    public virtual IDictionary<string, object?>? GetMeta(TResource resource)
    {
        return null;
    }

    /// <inheritdoc />
    public virtual Task OnPrepareWriteAsync(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task<IIdentifiable?> OnSetToOneRelationshipAsync(TResource leftResource, HasOneAttribute hasOneRelationship, IIdentifiable? rightResourceId,
        WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        return Task.FromResult(rightResourceId);
    }

    /// <inheritdoc />
    public virtual Task OnSetToManyRelationshipAsync(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
        WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task OnAddToRelationshipAsync(TId leftResourceId, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task OnRemoveFromRelationshipAsync(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task OnWritingAsync(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task OnWriteSucceededAsync(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual void OnDeserialize(TResource resource)
    {
    }

    /// <inheritdoc />
    public virtual void OnSerialize(TResource resource)
    {
    }

    /// <summary>
    /// This is an alias type intended to simplify the implementation's method signature. See <see cref="CreateSortExpressionFromLambda" /> for usage
    /// details.
    /// </summary>
    public sealed class PropertySortOrder : List<(Expression<Func<TResource, dynamic?>> KeySelector, ListSortDirection SortDirection)>
    {
    }
}
