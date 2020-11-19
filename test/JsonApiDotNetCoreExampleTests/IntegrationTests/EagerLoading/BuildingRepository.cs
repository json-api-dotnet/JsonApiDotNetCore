using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.EagerLoading
{
    public sealed class BuildingRepository : EntityFrameworkCoreRepository<Building>
    {
        public BuildingRepository(ITargetedFields targetedFields, IDbContextResolver contextResolver,
            IResourceGraph resourceGraph, IResourceFactory resourceFactory,
            IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory)
            : base(targetedFields, contextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory)
        {
        }

        public override async Task<Building> GetForCreateAsync(int id, CancellationToken cancellationToken)
        {
            var building = await base.GetForCreateAsync(id, cancellationToken);

            // Must ensure that an instance exists for this required relationship, so that POST succeeds.
            building.PrimaryDoor = new Door();

            return building;
        }
    }
}
