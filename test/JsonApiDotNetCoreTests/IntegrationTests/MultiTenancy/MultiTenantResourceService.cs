using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class MultiTenantResourceService<TResource, TId>(
    ITenantProvider tenantProvider, IResourceRepositoryAccessor repositoryAccessor, IQueryLayerComposer queryLayerComposer,
    IPaginationContext paginationContext, IJsonApiOptions options, ILoggerFactory loggerFactory, IJsonApiRequest request,
    IResourceChangeTracker<TResource> resourceChangeTracker, IResourceDefinitionAccessor resourceDefinitionAccessor)
    : JsonApiResourceService<TResource, TId>(repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory, request, resourceChangeTracker,
        resourceDefinitionAccessor)
    where TResource : class, IIdentifiable<TId>
{
    private readonly ITenantProvider _tenantProvider = tenantProvider;

    private static bool ResourceHasTenant => typeof(IHasTenant).IsAssignableFrom(typeof(TResource));

    protected override async Task InitializeResourceAsync(TResource resourceForDatabase, CancellationToken cancellationToken)
    {
        await base.InitializeResourceAsync(resourceForDatabase, cancellationToken);

        if (ResourceHasTenant)
        {
            Guid tenantId = _tenantProvider.TenantId;

            var resourceWithTenant = (IHasTenant)resourceForDatabase;
            resourceWithTenant.TenantId = tenantId;
        }
    }

    // To optimize performance, the default resource service does not always fetch all resources on write operations.
    // We do that here, to assure everything belongs to the active tenant. On mismatch, a 404 error is thrown.

    public override async Task<TResource?> CreateAsync(TResource resource, CancellationToken cancellationToken)
    {
        await AssertResourcesToAssignInRelationshipsExistAsync(resource, cancellationToken);

        return await base.CreateAsync(resource, cancellationToken);
    }

    public override async Task<TResource?> UpdateAsync([DisallowNull] TId id, TResource resource, CancellationToken cancellationToken)
    {
        await AssertResourcesToAssignInRelationshipsExistAsync(resource, cancellationToken);

        return await base.UpdateAsync(id, resource, cancellationToken);
    }

    public override async Task SetRelationshipAsync([DisallowNull] TId leftId, string relationshipName, object? rightValue, CancellationToken cancellationToken)
    {
        await AssertRightResourcesExistAsync(rightValue, cancellationToken);

        await base.SetRelationshipAsync(leftId, relationshipName, rightValue, cancellationToken);
    }

    public override async Task AddToToManyRelationshipAsync([DisallowNull] TId leftId, string relationshipName, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        _ = await GetPrimaryResourceByIdAsync(leftId, TopFieldSelection.OnlyIdAttribute, cancellationToken);
        await AssertRightResourcesExistAsync(rightResourceIds, cancellationToken);

        await base.AddToToManyRelationshipAsync(leftId, relationshipName, rightResourceIds, cancellationToken);
    }

    public override async Task DeleteAsync([DisallowNull] TId id, CancellationToken cancellationToken)
    {
        _ = await GetPrimaryResourceByIdAsync(id, TopFieldSelection.OnlyIdAttribute, cancellationToken);

        await base.DeleteAsync(id, cancellationToken);
    }
}
