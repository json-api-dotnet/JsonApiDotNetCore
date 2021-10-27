using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class BuildingRepository : EntityFrameworkCoreRepository<Building>
    {
        public BuildingRepository(ITargetedFields targetedFields, IDbContextResolver dbContextResolver, IResourceGraph resourceGraph,
            IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory,
            IResourceDefinitionAccessor resourceDefinitionAccessor)
            : base(targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory, resourceDefinitionAccessor)
        {
        }

        public override async Task<Building> GetForCreateAsync(int id, CancellationToken cancellationToken)
        {
            Building building = await base.GetForCreateAsync(id, cancellationToken);

            // Must ensure that an instance exists for this required relationship, so that POST succeeds.
            building.PrimaryDoor = new Door();

            return building;
        }
    }
}
