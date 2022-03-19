using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

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
    ///     (blog => blog.Author.Name.LastName, ListSortDirection.Ascending),
    ///     (blog => blog.Posts.Count, ListSortDirection.Descending),
    ///     (blog => blog.Title, ListSortDirection.Ascending)
    /// });
    /// ]]></code>
    /// </example>
    protected SortExpression CreateSortExpressionFromLambda(PropertySortOrder keySelectors)
    {
        ArgumentGuard.NotNullNorEmpty(keySelectors, nameof(keySelectors));

        ImmutableArray<SortElementExpression>.Builder elementsBuilder = ImmutableArray.CreateBuilder<SortElementExpression>(keySelectors.Count);
        var lambdaConverter = new SortExpressionLambdaConverter(ResourceGraph);

        foreach ((Expression<Func<TResource, object?>> keySelector, ListSortDirection sortDirection) in keySelectors)
        {
            try
            {
                SortElementExpression sortElement = lambdaConverter.FromLambda(keySelector, sortDirection);
                elementsBuilder.Add(sortElement);
            }
            catch (InvalidOperationException exception)
            {
                throw new JsonApiException(new ErrorObject(HttpStatusCode.InternalServerError)
                {
                    Title = "Invalid lambda expression for sorting from resource definition. " +
                        "It should select a property that is exposed as an attribute, or a to-many relationship followed by Count(). " +
                        "The property can be preceded by a path of to-one relationships. " +
                        "Examples: 'blog => blog.Title', 'blog => blog.Posts.Count', 'blog => blog.Author.Name.LastName'.",
                    Detail = $"The lambda expression '{keySelector}' is invalid. {exception.Message}"
                }, exception);
            }
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
    public sealed class PropertySortOrder : List<(Expression<Func<TResource, object?>> KeySelector, ListSortDirection SortDirection)>
    {
    }
}
