using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class BuildingRepository : EntityFrameworkCoreRepository<Building, int>
{
    public BuildingRepository(IJsonApiRequest request, ITargetedFields targetedFields, IDbContextResolver dbContextResolver, IResourceGraph resourceGraph,
        IResourceFactory resourceFactory, IResourceDefinitionAccessor resourceDefinitionAccessor, IEnumerable<IQueryConstraintProvider> constraintProviders,
        ILoggerFactory loggerFactory)
        : base(request, targetedFields, dbContextResolver, resourceGraph, resourceFactory, resourceDefinitionAccessor, constraintProviders, loggerFactory)
    {
    }

    public override async Task<Building> GetForCreateAsync(Type resourceClrType, int id, CancellationToken cancellationToken)
    {
        Building building = await base.GetForCreateAsync(resourceClrType, id, cancellationToken);

        // Must ensure that an instance exists for this required relationship, so that POST Resource succeeds.
        building.PrimaryDoor = new Door
        {
            Color = "(unspecified)"
        };

        return building;
    }
}
