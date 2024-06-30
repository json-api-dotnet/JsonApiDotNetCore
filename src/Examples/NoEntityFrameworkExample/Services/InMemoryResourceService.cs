using System.Collections;
using System.Diagnostics.CodeAnalysis;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NoEntityFrameworkExample.Services;

/// <summary>
/// Demonstrates how to replace the built-in <see cref="JsonApiResourceService{TResource,TId}" />. This read-only resource service uses the built-in
/// <see cref="IQueryLayerComposer" /> to convert the incoming query string parameters into a <see cref="QueryLayer" />, then uses the built-in
/// <see cref="QueryableBuilder" /> to convert the <see cref="QueryLayer" /> into a LINQ expression, then compiles and executes it against the in-memory
/// database.
/// </summary>
/// <remarks>
/// <para>
/// This resource service is a simplified version of the built-in resource service. Instead of implementing a resource service, consider implementing a
/// resource repository, which only needs to provide data access.
/// </para>
/// The incoming filter from query string is logged, just to show how you can access it directly.
/// </remarks>
/// <typeparam name="TResource">
/// The resource type.
/// </typeparam>
/// <typeparam name="TId">
/// The resource identifier type.
/// </typeparam>
public abstract class InMemoryResourceService<TResource, TId>(
    IJsonApiOptions options, IResourceGraph resourceGraph, IQueryLayerComposer queryLayerComposer, IPaginationContext paginationContext,
    IEnumerable<IQueryConstraintProvider> constraintProviders, IQueryableBuilder queryableBuilder, IReadOnlyModel entityModel,
    ILoggerFactory loggerFactory) : IResourceQueryService<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private readonly IJsonApiOptions _options = options;
    private readonly IQueryLayerComposer _queryLayerComposer = queryLayerComposer;
    private readonly IPaginationContext _paginationContext = paginationContext;
    private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders = constraintProviders;
    private readonly ILogger<InMemoryResourceService<TResource, TId>> _logger = loggerFactory.CreateLogger<InMemoryResourceService<TResource, TId>>();
    private readonly ResourceType _resourceType = resourceGraph.GetResourceType<TResource>();
    private readonly QueryLayerToLinqConverter _queryLayerToLinqConverter = new(entityModel, queryableBuilder);

    /// <inheritdoc />
    public Task<IReadOnlyCollection<TResource>> GetAsync(CancellationToken cancellationToken)
    {
        LogFiltersInTopScope();

        if (SetPrimaryTotalCountIsZero())
        {
            return Task.FromResult<IReadOnlyCollection<TResource>>(Array.Empty<TResource>());
        }

        QueryLayer queryLayer = _queryLayerComposer.ComposeFromConstraints(_resourceType);

        IEnumerable<TResource> dataSource = GetDataSource(_resourceType).Cast<TResource>();
        List<TResource> resources = _queryLayerToLinqConverter.ApplyQueryLayer(queryLayer, dataSource).ToList();

        if (queryLayer.Pagination?.PageSize?.Value == resources.Count)
        {
            _paginationContext.IsPageFull = true;
        }

        return Task.FromResult<IReadOnlyCollection<TResource>>(resources);
    }

    private void LogFiltersInTopScope()
    {
        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        FilterExpression[] filtersInTopScope = _constraintProviders
            .SelectMany(provider => provider.GetConstraints())
            .Where(constraint => constraint.Scope == null)
            .Select(constraint => constraint.Expression)
            .OfType<FilterExpression>()
            .ToArray();

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore

        FilterExpression? filter = LogicalExpression.Compose(LogicalOperator.And, filtersInTopScope);

        if (filter != null)
        {
            _logger.LogInformation($"Incoming top-level filter from query string: {filter}");
        }
    }

    private bool SetPrimaryTotalCountIsZero()
    {
        if (_options.IncludeTotalResourceCount)
        {
            var queryLayer = new QueryLayer(_resourceType)
            {
                Filter = _queryLayerComposer.GetPrimaryFilterFromConstraints(_resourceType)
            };

            IEnumerable<TResource> dataSource = GetDataSource(_resourceType).Cast<TResource>();
            IEnumerable<TResource> resources = _queryLayerToLinqConverter.ApplyQueryLayer(queryLayer, dataSource);

            _paginationContext.TotalResourceCount = resources.Count();

            if (_paginationContext.TotalResourceCount == 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public Task<TResource> GetAsync([DisallowNull] TId id, CancellationToken cancellationToken)
    {
        QueryLayer queryLayer = _queryLayerComposer.ComposeForGetById(id, _resourceType, TopFieldSelection.PreserveExisting);

        IEnumerable<TResource> dataSource = GetDataSource(_resourceType).Cast<TResource>();
        IEnumerable<TResource> resources = _queryLayerToLinqConverter.ApplyQueryLayer(queryLayer, dataSource);
        TResource? resource = resources.SingleOrDefault();

        if (resource == null)
        {
            throw new ResourceNotFoundException(id.ToString()!, _resourceType.PublicName);
        }

        return Task.FromResult(resource);
    }

    /// <inheritdoc />
    public Task<object?> GetSecondaryAsync([DisallowNull] TId id, string relationshipName, CancellationToken cancellationToken)
    {
        RelationshipAttribute? relationship = _resourceType.FindRelationshipByPublicName(relationshipName);

        if (relationship == null)
        {
            throw new RelationshipNotFoundException(relationshipName, _resourceType.PublicName);
        }

        SetNonPrimaryTotalCount(id, relationship);

        QueryLayer secondaryLayer = _queryLayerComposer.ComposeFromConstraints(relationship.RightType);
        QueryLayer primaryLayer = _queryLayerComposer.WrapLayerForSecondaryEndpoint(secondaryLayer, _resourceType, id, relationship);

        IEnumerable<TResource> dataSource = GetDataSource(_resourceType).Cast<TResource>();
        IEnumerable<TResource> primaryResources = _queryLayerToLinqConverter.ApplyQueryLayer(primaryLayer, dataSource);
        TResource? primaryResource = primaryResources.SingleOrDefault();

        if (primaryResource == null)
        {
            throw new ResourceNotFoundException(id.ToString()!, _resourceType.PublicName);
        }

        object? rightValue = relationship.GetValue(primaryResource);

        if (rightValue is ICollection rightResources && secondaryLayer.Pagination?.PageSize?.Value == rightResources.Count)
        {
            _paginationContext.IsPageFull = true;
        }

        return Task.FromResult(rightValue);
    }

    private void SetNonPrimaryTotalCount([DisallowNull] TId id, RelationshipAttribute relationship)
    {
        if (_options.IncludeTotalResourceCount && relationship is HasManyAttribute hasManyRelationship)
        {
            FilterExpression? secondaryFilter = _queryLayerComposer.GetSecondaryFilterFromConstraints(id, hasManyRelationship);

            if (secondaryFilter == null)
            {
                return;
            }

            var queryLayer = new QueryLayer(hasManyRelationship.RightType)
            {
                Filter = secondaryFilter
            };

            IEnumerable<IIdentifiable> dataSource = GetDataSource(hasManyRelationship.RightType);
            IEnumerable<IIdentifiable> resources = _queryLayerToLinqConverter.ApplyQueryLayer(queryLayer, dataSource);

            _paginationContext.TotalResourceCount = resources.Count();
        }
    }

    /// <inheritdoc />
    public Task<object?> GetRelationshipAsync([DisallowNull] TId id, string relationshipName, CancellationToken cancellationToken)
    {
        return GetSecondaryAsync(id, relationshipName, cancellationToken);
    }

    protected abstract IEnumerable<IIdentifiable> GetDataSource(ResourceType resourceType);
}
