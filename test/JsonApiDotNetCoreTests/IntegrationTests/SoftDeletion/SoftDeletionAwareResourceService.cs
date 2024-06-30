using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class SoftDeletionAwareResourceService<TResource, TId>(
    ISystemClock systemClock, ITargetedFields targetedFields, IResourceRepositoryAccessor repositoryAccessor, IQueryLayerComposer queryLayerComposer,
    IPaginationContext paginationContext, IJsonApiOptions options, ILoggerFactory loggerFactory, IJsonApiRequest request,
    IResourceChangeTracker<TResource> resourceChangeTracker, IResourceDefinitionAccessor resourceDefinitionAccessor)
    : JsonApiResourceService<TResource, TId>(repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory, request, resourceChangeTracker,
        resourceDefinitionAccessor)
    where TResource : class, IIdentifiable<TId>
{
    private readonly ISystemClock _systemClock = systemClock;
    private readonly ITargetedFields _targetedFields = targetedFields;
    private readonly IResourceRepositoryAccessor _repositoryAccessor = repositoryAccessor;
    private readonly IJsonApiRequest _request = request;

    // To optimize performance, the default resource service does not always fetch all resources on write operations.
    // We do that here, to assure a 404 error is thrown for soft-deleted resources.

    public override async Task<TResource?> CreateAsync(TResource resource, CancellationToken cancellationToken)
    {
        if (_targetedFields.Relationships.Any(relationship => IsSoftDeletable(relationship.RightType.ClrType)))
        {
            await AssertResourcesToAssignInRelationshipsExistAsync(resource, cancellationToken);
        }

        return await base.CreateAsync(resource, cancellationToken);
    }

    public override async Task<TResource?> UpdateAsync([DisallowNull] TId id, TResource resource, CancellationToken cancellationToken)
    {
        if (_targetedFields.Relationships.Any(relationship => IsSoftDeletable(relationship.RightType.ClrType)))
        {
            await AssertResourcesToAssignInRelationshipsExistAsync(resource, cancellationToken);
        }

        return await base.UpdateAsync(id, resource, cancellationToken);
    }

    public override async Task SetRelationshipAsync([DisallowNull] TId leftId, string relationshipName, object? rightValue, CancellationToken cancellationToken)
    {
        if (IsSoftDeletable(_request.Relationship!.RightType.ClrType))
        {
            await AssertRightResourcesExistAsync(rightValue, cancellationToken);
        }

        await base.SetRelationshipAsync(leftId, relationshipName, rightValue, cancellationToken);
    }

    public override async Task AddToToManyRelationshipAsync([DisallowNull] TId leftId, string relationshipName, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        if (IsSoftDeletable(typeof(TResource)))
        {
            _ = await GetPrimaryResourceByIdAsync(leftId, TopFieldSelection.OnlyIdAttribute, cancellationToken);
        }

        if (IsSoftDeletable(_request.Relationship!.RightType.ClrType))
        {
            await AssertRightResourcesExistAsync(rightResourceIds, cancellationToken);
        }

        await base.AddToToManyRelationshipAsync(leftId, relationshipName, rightResourceIds, cancellationToken);
    }

    public override async Task DeleteAsync([DisallowNull] TId id, CancellationToken cancellationToken)
    {
        if (IsSoftDeletable(typeof(TResource)))
        {
            await SoftDeleteAsync(id, cancellationToken);
        }
        else
        {
            await base.DeleteAsync(id, cancellationToken);
        }
    }

    private async Task SoftDeleteAsync([DisallowNull] TId id, CancellationToken cancellationToken)
    {
        TResource resourceFromDatabase = await GetPrimaryResourceForUpdateAsync(id, cancellationToken);

        ((ISoftDeletable)resourceFromDatabase).SoftDeletedAt = _systemClock.UtcNow;

        // A delete operation does not target any fields, so we can just pass resourceFromDatabase twice.
        await _repositoryAccessor.UpdateAsync(resourceFromDatabase, resourceFromDatabase, cancellationToken);
    }

    private static bool IsSoftDeletable(Type resourceClrType)
    {
        return typeof(ISoftDeletable).IsAssignableFrom(resourceClrType);
    }
}
