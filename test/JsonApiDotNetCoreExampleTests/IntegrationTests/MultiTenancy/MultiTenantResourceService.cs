using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.MultiTenancy
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public class MultiTenantResourceService<TResource, TId> : JsonApiResourceService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly ITenantProvider _tenantProvider;

        private static bool ResourceHasTenant => typeof(IHasTenant).IsAssignableFrom(typeof(TResource));

        public MultiTenantResourceService(ITenantProvider tenantProvider, IResourceRepositoryAccessor repositoryAccessor,
            IQueryLayerComposer queryLayerComposer, IPaginationContext paginationContext, IJsonApiOptions options, ILoggerFactory loggerFactory,
            IJsonApiRequest request, IResourceChangeTracker<TResource> resourceChangeTracker)
            : base(repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory, request, resourceChangeTracker)
        {
            _tenantProvider = tenantProvider;
        }

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

        public override async Task<TResource> CreateAsync(TResource resource, CancellationToken cancellationToken)
        {
            await AssertResourcesToAssignInRelationshipsExistAsync(resource, cancellationToken);

            return await base.CreateAsync(resource, cancellationToken);
        }

        public override async Task<TResource> UpdateAsync(TId id, TResource resource, CancellationToken cancellationToken)
        {
            await AssertResourcesToAssignInRelationshipsExistAsync(resource, cancellationToken);

            return await base.UpdateAsync(id, resource, cancellationToken);
        }

        public override async Task SetRelationshipAsync(TId primaryId, string relationshipName, object secondaryResourceIds,
            CancellationToken cancellationToken)
        {
            await AssertRightResourcesExistAsync(secondaryResourceIds, cancellationToken);

            await base.SetRelationshipAsync(primaryId, relationshipName, secondaryResourceIds, cancellationToken);
        }

        public override async Task AddToToManyRelationshipAsync(TId primaryId, string relationshipName, ISet<IIdentifiable> secondaryResourceIds,
            CancellationToken cancellationToken)
        {
            _ = await GetPrimaryResourceByIdAsync(primaryId, TopFieldSelection.OnlyIdAttribute, cancellationToken);
            await AssertRightResourcesExistAsync(secondaryResourceIds, cancellationToken);

            await base.AddToToManyRelationshipAsync(primaryId, relationshipName, secondaryResourceIds, cancellationToken);
        }

        public override async Task DeleteAsync(TId id, CancellationToken cancellationToken)
        {
            _ = await GetPrimaryResourceByIdAsync(id, TopFieldSelection.OnlyIdAttribute, cancellationToken);

            await base.DeleteAsync(id, cancellationToken);
        }
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public class MultiTenantResourceService<TResource> : MultiTenantResourceService<TResource, int>, IResourceService<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public MultiTenantResourceService(ITenantProvider tenantProvider, IResourceRepositoryAccessor repositoryAccessor,
            IQueryLayerComposer queryLayerComposer, IPaginationContext paginationContext, IJsonApiOptions options, ILoggerFactory loggerFactory,
            IJsonApiRequest request, IResourceChangeTracker<TResource> resourceChangeTracker)
            : base(tenantProvider, repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory, request, resourceChangeTracker)
        {
        }
    }
}
