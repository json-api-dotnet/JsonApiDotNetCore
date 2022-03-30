using System.Collections;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

namespace JsonApiDotNetCore.Services;

/// <inheritdoc />
[PublicAPI]
public class JsonApiResourceService<TResource, TId> : IResourceService<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private readonly CollectionConverter _collectionConverter = new();
    private readonly IResourceRepositoryAccessor _repositoryAccessor;
    private readonly IQueryLayerComposer _queryLayerComposer;
    private readonly IPaginationContext _paginationContext;
    private readonly IJsonApiOptions _options;
    private readonly TraceLogWriter<JsonApiResourceService<TResource, TId>> _traceWriter;
    private readonly IJsonApiRequest _request;
    private readonly IResourceChangeTracker<TResource> _resourceChangeTracker;
    private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;

    public JsonApiResourceService(IResourceRepositoryAccessor repositoryAccessor, IQueryLayerComposer queryLayerComposer, IPaginationContext paginationContext,
        IJsonApiOptions options, ILoggerFactory loggerFactory, IJsonApiRequest request, IResourceChangeTracker<TResource> resourceChangeTracker,
        IResourceDefinitionAccessor resourceDefinitionAccessor)
    {
        ArgumentGuard.NotNull(repositoryAccessor, nameof(repositoryAccessor));
        ArgumentGuard.NotNull(queryLayerComposer, nameof(queryLayerComposer));
        ArgumentGuard.NotNull(paginationContext, nameof(paginationContext));
        ArgumentGuard.NotNull(options, nameof(options));
        ArgumentGuard.NotNull(loggerFactory, nameof(loggerFactory));
        ArgumentGuard.NotNull(request, nameof(request));
        ArgumentGuard.NotNull(resourceChangeTracker, nameof(resourceChangeTracker));
        ArgumentGuard.NotNull(resourceDefinitionAccessor, nameof(resourceDefinitionAccessor));

        _repositoryAccessor = repositoryAccessor;
        _queryLayerComposer = queryLayerComposer;
        _paginationContext = paginationContext;
        _options = options;
        _request = request;
        _resourceChangeTracker = resourceChangeTracker;
        _resourceDefinitionAccessor = resourceDefinitionAccessor;
        _traceWriter = new TraceLogWriter<JsonApiResourceService<TResource, TId>>(loggerFactory);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyCollection<TResource>> GetAsync(CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart();

        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Get resources");

        AssertPrimaryResourceTypeInJsonApiRequestIsNotNull(_request.PrimaryResourceType);

        if (_options.IncludeTotalResourceCount)
        {
            FilterExpression? topFilter = _queryLayerComposer.GetPrimaryFilterFromConstraints(_request.PrimaryResourceType);
            _paginationContext.TotalResourceCount = await _repositoryAccessor.CountAsync(_request.PrimaryResourceType, topFilter, cancellationToken);

            if (_paginationContext.TotalResourceCount == 0)
            {
                return Array.Empty<TResource>();
            }
        }

        QueryLayer queryLayer = _queryLayerComposer.ComposeFromConstraints(_request.PrimaryResourceType);
        IReadOnlyCollection<TResource> resources = await _repositoryAccessor.GetAsync<TResource>(queryLayer, cancellationToken);

        if (queryLayer.Pagination?.PageSize?.Value == resources.Count)
        {
            _paginationContext.IsPageFull = true;
        }

        return resources;
    }

    /// <inheritdoc />
    public virtual async Task<TResource> GetAsync(TId id, CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            id
        });

        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Get single resource");

        return await GetPrimaryResourceByIdAsync(id, TopFieldSelection.PreserveExisting, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<object?> GetSecondaryAsync(TId id, string relationshipName, CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            id,
            relationshipName
        });

        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Get secondary resource(s)");

        AssertPrimaryResourceTypeInJsonApiRequestIsNotNull(_request.PrimaryResourceType);
        AssertHasRelationship(_request.Relationship, relationshipName);

        if (_options.IncludeTotalResourceCount && _request.IsCollection)
        {
            await RetrieveResourceCountForNonPrimaryEndpointAsync(id, (HasManyAttribute)_request.Relationship, cancellationToken);

            // We cannot return early when _paginationContext.TotalResourceCount == 0, because we don't know whether
            // the parent resource exists. In case the parent does not exist, an error is produced below.
        }

        QueryLayer secondaryLayer = _queryLayerComposer.ComposeFromConstraints(_request.SecondaryResourceType!);
        QueryLayer primaryLayer = _queryLayerComposer.WrapLayerForSecondaryEndpoint(secondaryLayer, _request.PrimaryResourceType, id, _request.Relationship);
        IReadOnlyCollection<TResource> primaryResources = await _repositoryAccessor.GetAsync<TResource>(primaryLayer, cancellationToken);

        TResource? primaryResource = primaryResources.SingleOrDefault();
        AssertPrimaryResourceExists(primaryResource);

        object? rightValue = _request.Relationship.GetValue(primaryResource);

        if (rightValue is ICollection rightResources && secondaryLayer.Pagination?.PageSize?.Value == rightResources.Count)
        {
            _paginationContext.IsPageFull = true;
        }

        return rightValue;
    }

    /// <inheritdoc />
    public virtual async Task<object?> GetRelationshipAsync(TId id, string relationshipName, CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            id,
            relationshipName
        });

        ArgumentGuard.NotNullNorEmpty(relationshipName, nameof(relationshipName));

        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Get relationship");

        AssertPrimaryResourceTypeInJsonApiRequestIsNotNull(_request.PrimaryResourceType);
        AssertHasRelationship(_request.Relationship, relationshipName);

        if (_options.IncludeTotalResourceCount && _request.IsCollection)
        {
            await RetrieveResourceCountForNonPrimaryEndpointAsync(id, (HasManyAttribute)_request.Relationship, cancellationToken);

            // We cannot return early when _paginationContext.TotalResourceCount == 0, because we don't know whether
            // the parent resource exists. In case the parent does not exist, an error is produced below.
        }

        QueryLayer secondaryLayer = _queryLayerComposer.ComposeSecondaryLayerForRelationship(_request.SecondaryResourceType!);
        QueryLayer primaryLayer = _queryLayerComposer.WrapLayerForSecondaryEndpoint(secondaryLayer, _request.PrimaryResourceType, id, _request.Relationship);
        IReadOnlyCollection<TResource> primaryResources = await _repositoryAccessor.GetAsync<TResource>(primaryLayer, cancellationToken);

        TResource? primaryResource = primaryResources.SingleOrDefault();
        AssertPrimaryResourceExists(primaryResource);

        object? rightValue = _request.Relationship.GetValue(primaryResource);

        if (rightValue is ICollection rightResources && secondaryLayer.Pagination?.PageSize?.Value == rightResources.Count)
        {
            _paginationContext.IsPageFull = true;
        }

        return rightValue;
    }

    private async Task RetrieveResourceCountForNonPrimaryEndpointAsync(TId id, HasManyAttribute relationship, CancellationToken cancellationToken)
    {
        FilterExpression? secondaryFilter = _queryLayerComposer.GetSecondaryFilterFromConstraints(id, relationship);

        if (secondaryFilter != null)
        {
            _paginationContext.TotalResourceCount = await _repositoryAccessor.CountAsync(relationship.RightType, secondaryFilter, cancellationToken);
        }
    }

    /// <inheritdoc />
    public virtual async Task<TResource?> CreateAsync(TResource resource, CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            resource
        });

        ArgumentGuard.NotNull(resource, nameof(resource));

        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Create resource");

        TResource resourceFromRequest = resource;
        _resourceChangeTracker.SetRequestAttributeValues(resourceFromRequest);

        await RefreshResourceTypesInHierarchyToAssignInRelationshipsAsync(resourceFromRequest, cancellationToken);

        Type resourceClrType = resourceFromRequest.GetClrType();
        TResource resourceForDatabase = await _repositoryAccessor.GetForCreateAsync<TResource, TId>(resourceClrType, resourceFromRequest.Id, cancellationToken);
        PromoteJsonApiRequest(resourceForDatabase);

        _resourceChangeTracker.SetInitiallyStoredAttributeValues(resourceForDatabase);

        await InitializeResourceAsync(resourceForDatabase, cancellationToken);

        try
        {
            await _repositoryAccessor.CreateAsync(resourceFromRequest, resourceForDatabase, cancellationToken);
        }
        catch (DataStoreUpdateException)
        {
            await AssertPrimaryResourceDoesNotExistAsync(resourceFromRequest, cancellationToken);
            await AssertResourcesToAssignInRelationshipsExistAsync(resourceFromRequest, cancellationToken);
            throw;
        }

        TResource resourceFromDatabase = await GetPrimaryResourceByIdAsync(resourceForDatabase.Id, TopFieldSelection.WithAllAttributes, cancellationToken);

        _resourceChangeTracker.SetFinallyStoredAttributeValues(resourceFromDatabase);

        bool hasImplicitChanges = _resourceChangeTracker.HasImplicitChanges();
        return hasImplicitChanges ? resourceFromDatabase : null;
    }

    protected async Task AssertPrimaryResourceDoesNotExistAsync(TResource resource, CancellationToken cancellationToken)
    {
        if (!Equals(resource.Id, default(TId)))
        {
            TResource? existingResource = await GetPrimaryResourceByIdOrDefaultAsync(resource.Id, TopFieldSelection.OnlyIdAttribute, cancellationToken);

            if (existingResource != null)
            {
                throw new ResourceAlreadyExistsException(resource.StringId!, _request.PrimaryResourceType!.PublicName);
            }
        }
    }

    protected virtual async Task InitializeResourceAsync(TResource resourceForDatabase, CancellationToken cancellationToken)
    {
        await _resourceDefinitionAccessor.OnPrepareWriteAsync(resourceForDatabase, WriteOperationKind.CreateResource, cancellationToken);
    }

    private async Task RefreshResourceTypesInHierarchyToAssignInRelationshipsAsync(TResource primaryResource, CancellationToken cancellationToken)
    {
        await ValidateResourcesToAssignInRelationshipsExistWithRefreshAsync(primaryResource, true, cancellationToken);
    }

    protected async Task AssertResourcesToAssignInRelationshipsExistAsync(TResource primaryResource, CancellationToken cancellationToken)
    {
        await ValidateResourcesToAssignInRelationshipsExistWithRefreshAsync(primaryResource, false, cancellationToken);
    }

    private async Task ValidateResourcesToAssignInRelationshipsExistWithRefreshAsync(TResource primaryResource, bool onlyIfTypeHierarchy,
        CancellationToken cancellationToken)
    {
        var missingResources = new List<MissingResourceInRelationship>();

        foreach ((QueryLayer queryLayer, RelationshipAttribute relationship) in _queryLayerComposer.ComposeForGetTargetedSecondaryResourceIds(primaryResource))
        {
            if (!onlyIfTypeHierarchy || relationship.RightType.IsPartOfTypeHierarchy())
            {
                object? rightValue = relationship.GetValue(primaryResource);
                HashSet<IIdentifiable> rightResourceIds = _collectionConverter.ExtractResources(rightValue).ToHashSet(IdentifiableComparer.Instance);

                if (rightResourceIds.Any())
                {
                    IAsyncEnumerable<MissingResourceInRelationship> missingResourcesInRelationship =
                        GetMissingRightResourcesAsync(queryLayer, relationship, rightResourceIds, cancellationToken);

                    await missingResources.AddRangeAsync(missingResourcesInRelationship, cancellationToken);

                    // Some of the right-side resources from request may be typed as base types, but stored as derived types.
                    // Now that we've fetched them, update the request types so that resource definitions observe the actually stored types.
                    object? newRightValue = relationship is HasOneAttribute
                        ? rightResourceIds.FirstOrDefault()
                        : _collectionConverter.CopyToTypedCollection(rightResourceIds, relationship.Property.PropertyType);

                    relationship.SetValue(primaryResource, newRightValue);
                }
            }
        }

        if (missingResources.Any())
        {
            throw new ResourcesInRelationshipsNotFoundException(missingResources);
        }
    }

    private async IAsyncEnumerable<MissingResourceInRelationship> GetMissingRightResourcesAsync(QueryLayer existingRightResourceIdsQueryLayer,
        RelationshipAttribute relationship, ISet<IIdentifiable> rightResourceIds, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IReadOnlyCollection<IIdentifiable> existingResources = await _repositoryAccessor.GetAsync(existingRightResourceIdsQueryLayer.ResourceType,
            existingRightResourceIdsQueryLayer, cancellationToken);

        foreach (IIdentifiable rightResourceId in rightResourceIds.ToArray())
        {
            Type rightResourceClrType = rightResourceId.GetClrType();
            IIdentifiable? existingResourceId = existingResources.FirstOrDefault(resource => resource.StringId == rightResourceId.StringId);

            if (existingResourceId != null)
            {
                Type existingResourceClrType = existingResourceId.GetClrType();

                if (rightResourceClrType.IsAssignableFrom(existingResourceClrType))
                {
                    if (rightResourceClrType != existingResourceClrType)
                    {
                        // PERF: As a side effect, we replace the resource base type from request with the derived type that is stored.
                        rightResourceIds.Remove(rightResourceId);
                        rightResourceIds.Add(existingResourceId);
                    }

                    continue;
                }
            }

            ResourceType requestResourceType = relationship.RightType.GetTypeOrDerived(rightResourceClrType);
            yield return new MissingResourceInRelationship(relationship.PublicName, requestResourceType.PublicName, rightResourceId.StringId!);
        }
    }

    /// <inheritdoc />
    public virtual async Task AddToToManyRelationshipAsync(TId leftId, string relationshipName, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            leftId,
            relationshipName,
            rightResourceIds
        });

        ArgumentGuard.NotNullNorEmpty(relationshipName, nameof(relationshipName));
        ArgumentGuard.NotNull(rightResourceIds, nameof(rightResourceIds));

        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Add to to-many relationship");

        AssertHasRelationship(_request.Relationship, relationshipName);

        TResource? resourceFromDatabase = null;

        if (rightResourceIds.Any() && _request.Relationship is HasManyAttribute { IsManyToMany: true } manyToManyRelationship)
        {
            // In the case of a many-to-many relationship, creating a duplicate entry in the join table results in a
            // unique constraint violation. We avoid that by excluding already-existing entries from the set in advance.
            resourceFromDatabase = await RemoveExistingIdsFromRelationshipRightSideAsync(manyToManyRelationship, leftId, rightResourceIds, cancellationToken);
        }

        if (_request.Relationship.LeftType.IsPartOfTypeHierarchy())
        {
            // The left resource may be stored as a derived type. We fetch it, so we'll know the stored type, which
            // enables to invoke IResourceDefinition<TResource> with TResource being the stored resource type.
            resourceFromDatabase ??= await GetPrimaryResourceForUpdateAsync(leftId, cancellationToken);
            PromoteJsonApiRequest(resourceFromDatabase);
        }

        ISet<IIdentifiable> effectiveRightResourceIds = rightResourceIds;

        if (_request.Relationship.RightType.IsPartOfTypeHierarchy())
        {
            // Some of the incoming right-side resources may be stored as a derived type. We fetch them, so we'll know
            // the stored types, which enables to invoke resource definitions with the stored right-side resources types.
            object? rightValue = await AssertRightResourcesExistAsync(rightResourceIds, cancellationToken);
            effectiveRightResourceIds = ((IEnumerable<IIdentifiable>)rightValue!).ToHashSet(IdentifiableComparer.Instance);
        }

        try
        {
            await _repositoryAccessor.AddToToManyRelationshipAsync(resourceFromDatabase, leftId, effectiveRightResourceIds, cancellationToken);
        }
        catch (DataStoreUpdateException)
        {
            await GetPrimaryResourceByIdAsync(leftId, TopFieldSelection.OnlyIdAttribute, cancellationToken);
            await AssertRightResourcesExistAsync(effectiveRightResourceIds, cancellationToken);
            throw;
        }
    }

    private async Task<TResource> RemoveExistingIdsFromRelationshipRightSideAsync(HasManyAttribute hasManyRelationship, TId leftId,
        ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
    {
        TResource leftResource = await GetForHasManyUpdateAsync(hasManyRelationship, leftId, rightResourceIds, cancellationToken);

        object? rightValue = hasManyRelationship.GetValue(leftResource);
        ICollection<IIdentifiable> existingRightResourceIds = _collectionConverter.ExtractResources(rightValue);

        rightResourceIds.ExceptWith(existingRightResourceIds);

        return leftResource;
    }

    private async Task<TResource> GetForHasManyUpdateAsync(HasManyAttribute hasManyRelationship, TId leftId, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        QueryLayer queryLayer = _queryLayerComposer.ComposeForHasMany(hasManyRelationship, leftId, rightResourceIds);
        var leftResource = await _repositoryAccessor.GetForUpdateAsync<TResource>(queryLayer, cancellationToken);
        AssertPrimaryResourceExists(leftResource);

        return leftResource;
    }

    protected async Task<object?> AssertRightResourcesExistAsync(object? rightValue, CancellationToken cancellationToken)
    {
        AssertRelationshipInJsonApiRequestIsNotNull(_request.Relationship);

        HashSet<IIdentifiable> rightResourceIds = _collectionConverter.ExtractResources(rightValue).ToHashSet(IdentifiableComparer.Instance);
        object? newRightValue = rightValue;

        if (rightResourceIds.Any())
        {
            QueryLayer queryLayer = _queryLayerComposer.ComposeForGetRelationshipRightIds(_request.Relationship, rightResourceIds);

            List<MissingResourceInRelationship> missingResources =
                await GetMissingRightResourcesAsync(queryLayer, _request.Relationship, rightResourceIds, cancellationToken).ToListAsync(cancellationToken);

            // Some of the right-side resources from request may be typed as base types, but stored as derived types.
            // Now that we've fetched them, update the request types so that resource definitions observe the actually stored types.
            newRightValue = _request.Relationship is HasOneAttribute
                ? rightResourceIds.FirstOrDefault()
                : _collectionConverter.CopyToTypedCollection(rightResourceIds, _request.Relationship.Property.PropertyType);

            if (missingResources.Any())
            {
                throw new ResourcesInRelationshipsNotFoundException(missingResources);
            }
        }

        return newRightValue;
    }

    /// <inheritdoc />
    public virtual async Task<TResource?> UpdateAsync(TId id, TResource resource, CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            id,
            resource
        });

        ArgumentGuard.NotNull(resource, nameof(resource));

        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Update resource");

        TResource resourceFromRequest = resource;
        _resourceChangeTracker.SetRequestAttributeValues(resourceFromRequest);

        await RefreshResourceTypesInHierarchyToAssignInRelationshipsAsync(resourceFromRequest, cancellationToken);

        TResource resourceFromDatabase = await GetPrimaryResourceForUpdateAsync(id, cancellationToken);
        PromoteJsonApiRequest(resourceFromDatabase);

        _resourceChangeTracker.SetInitiallyStoredAttributeValues(resourceFromDatabase);

        await _resourceDefinitionAccessor.OnPrepareWriteAsync(resourceFromDatabase, WriteOperationKind.UpdateResource, cancellationToken);

        try
        {
            await _repositoryAccessor.UpdateAsync(resourceFromRequest, resourceFromDatabase, cancellationToken);
        }
        catch (DataStoreUpdateException)
        {
            await AssertResourcesToAssignInRelationshipsExistAsync(resourceFromRequest, cancellationToken);
            throw;
        }

        TResource afterResourceFromDatabase = await GetPrimaryResourceByIdAsync(id, TopFieldSelection.WithAllAttributes, cancellationToken);

        _resourceChangeTracker.SetFinallyStoredAttributeValues(afterResourceFromDatabase);

        bool hasImplicitChanges = _resourceChangeTracker.HasImplicitChanges();
        return hasImplicitChanges ? afterResourceFromDatabase : null;
    }

    /// <inheritdoc />
    public virtual async Task SetRelationshipAsync(TId leftId, string relationshipName, object? rightValue, CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            leftId,
            relationshipName,
            rightValue
        });

        ArgumentGuard.NotNullNorEmpty(relationshipName, nameof(relationshipName));

        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Set relationship");

        AssertHasRelationship(_request.Relationship, relationshipName);

        object? effectiveRightValue = _request.Relationship.RightType.IsPartOfTypeHierarchy()
            // Some of the incoming right-side resources may be stored as a derived type. We fetch them, so we'll know
            // the stored types, which enables to invoke resource definitions with the stored right-side resources types.
            ? await AssertRightResourcesExistAsync(rightValue, cancellationToken)
            : rightValue;

        TResource resourceFromDatabase = await GetPrimaryResourceForUpdateAsync(leftId, cancellationToken);
        PromoteJsonApiRequest(resourceFromDatabase);

        await _resourceDefinitionAccessor.OnPrepareWriteAsync(resourceFromDatabase, WriteOperationKind.SetRelationship, cancellationToken);

        try
        {
            await _repositoryAccessor.SetRelationshipAsync(resourceFromDatabase, effectiveRightValue, cancellationToken);
        }
        catch (DataStoreUpdateException)
        {
            await AssertRightResourcesExistAsync(effectiveRightValue, cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            id
        });

        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Repository - Delete resource");

        AssertPrimaryResourceTypeInJsonApiRequestIsNotNull(_request.PrimaryResourceType);

        TResource? resourceFromDatabase = null;

        if (_request.PrimaryResourceType.IsPartOfTypeHierarchy())
        {
            // The resource to delete may be stored as a derived type. We fetch it, so we'll know the stored type, which
            // enables to invoke IResourceDefinition<TResource> with TResource being the stored resource type.
            resourceFromDatabase = await GetPrimaryResourceForUpdateAsync(id, cancellationToken);
            PromoteJsonApiRequest(resourceFromDatabase);
        }

        try
        {
            await _repositoryAccessor.DeleteAsync(resourceFromDatabase, id, cancellationToken);
        }
        catch (DataStoreUpdateException)
        {
            await GetPrimaryResourceByIdAsync(id, TopFieldSelection.OnlyIdAttribute, cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    public virtual async Task RemoveFromToManyRelationshipAsync(TId leftId, string relationshipName, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            leftId,
            relationshipName,
            rightResourceIds
        });

        ArgumentGuard.NotNullNorEmpty(relationshipName, nameof(relationshipName));
        ArgumentGuard.NotNull(rightResourceIds, nameof(rightResourceIds));

        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Repository - Remove from to-many relationship");

        AssertHasRelationship(_request.Relationship, relationshipName);
        var hasManyRelationship = (HasManyAttribute)_request.Relationship;

        TResource resourceFromDatabase = await GetForHasManyUpdateAsync(hasManyRelationship, leftId, rightResourceIds, cancellationToken);
        PromoteJsonApiRequest(resourceFromDatabase);

        object? rightValue = await AssertRightResourcesExistAsync(rightResourceIds, cancellationToken);
        ISet<IIdentifiable> effectiveRightResourceIds = ((IEnumerable<IIdentifiable>)rightValue!).ToHashSet(IdentifiableComparer.Instance);

        await _resourceDefinitionAccessor.OnPrepareWriteAsync(resourceFromDatabase, WriteOperationKind.SetRelationship, cancellationToken);

        await _repositoryAccessor.RemoveFromToManyRelationshipAsync(resourceFromDatabase, effectiveRightResourceIds, cancellationToken);
    }

    protected async Task<TResource> GetPrimaryResourceByIdAsync(TId id, TopFieldSelection fieldSelection, CancellationToken cancellationToken)
    {
        TResource? primaryResource = await GetPrimaryResourceByIdOrDefaultAsync(id, fieldSelection, cancellationToken);
        AssertPrimaryResourceExists(primaryResource);

        return primaryResource;
    }

    private async Task<TResource?> GetPrimaryResourceByIdOrDefaultAsync(TId id, TopFieldSelection fieldSelection, CancellationToken cancellationToken)
    {
        AssertPrimaryResourceTypeInJsonApiRequestIsNotNull(_request.PrimaryResourceType);

        QueryLayer primaryLayer = _queryLayerComposer.ComposeForGetById(id, _request.PrimaryResourceType, fieldSelection);

        IReadOnlyCollection<TResource> primaryResources = await _repositoryAccessor.GetAsync<TResource>(primaryLayer, cancellationToken);
        return primaryResources.SingleOrDefault();
    }

    protected async Task<TResource> GetPrimaryResourceForUpdateAsync(TId id, CancellationToken cancellationToken)
    {
        AssertPrimaryResourceTypeInJsonApiRequestIsNotNull(_request.PrimaryResourceType);

        QueryLayer queryLayer = _queryLayerComposer.ComposeForUpdate(id, _request.PrimaryResourceType);
        var resource = await _repositoryAccessor.GetForUpdateAsync<TResource>(queryLayer, cancellationToken);
        AssertPrimaryResourceExists(resource);

        return resource;
    }

    private void PromoteJsonApiRequest(TResource resourceFromDatabase)
    {
        // When using resource inheritance, the stored left-side resource may be more derived than what this endpoint assumes.
        // In that case, we promote data in IJsonApiRequest to better represent what is going on.

        Type storedType = resourceFromDatabase.GetClrType();

        if (storedType != typeof(TResource))
        {
            AssertPrimaryResourceTypeInJsonApiRequestIsNotNull(_request.PrimaryResourceType);
            ResourceType? derivedType = _request.PrimaryResourceType.GetAllConcreteDerivedTypes().FirstOrDefault(type => type.ClrType == storedType);

            if (derivedType == null)
            {
                throw new InvalidConfigurationException($"Type '{storedType}' does not exist in the resource graph.");
            }

            var request = (JsonApiRequest)_request;
            request.PrimaryResourceType = derivedType;

            if (request.Relationship != null)
            {
                request.Relationship = derivedType.GetRelationshipByPublicName(request.Relationship.PublicName);
            }
        }
    }

    [AssertionMethod]
    private void AssertPrimaryResourceExists([SysNotNull] TResource? resource)
    {
        AssertPrimaryResourceTypeInJsonApiRequestIsNotNull(_request.PrimaryResourceType);

        if (resource == null)
        {
            throw new ResourceNotFoundException(_request.PrimaryId!, _request.PrimaryResourceType.PublicName);
        }
    }

    [AssertionMethod]
    private void AssertHasRelationship([SysNotNull] RelationshipAttribute? relationship, string name)
    {
        if (relationship == null)
        {
            throw new RelationshipNotFoundException(name, _request.PrimaryResourceType!.PublicName);
        }
    }

    [AssertionMethod]
    private void AssertPrimaryResourceTypeInJsonApiRequestIsNotNull([SysNotNull] ResourceType? resourceType)
    {
        if (resourceType == null)
        {
            throw new InvalidOperationException(
                $"Expected {nameof(IJsonApiRequest)}.{nameof(IJsonApiRequest.PrimaryResourceType)} not to be null at this point.");
        }
    }

    [AssertionMethod]
    private void AssertRelationshipInJsonApiRequestIsNotNull([SysNotNull] RelationshipAttribute? relationship)
    {
        if (relationship == null)
        {
            throw new InvalidOperationException($"Expected {nameof(IJsonApiRequest)}.{nameof(IJsonApiRequest.Relationship)} not to be null at this point.");
        }
    }
}
