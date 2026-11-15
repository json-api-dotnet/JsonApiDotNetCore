using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class BuildingRepository(
    IJsonApiRequest request, ITargetedFields targetedFields, IDbContextResolver dbContextResolver, IResourceGraph resourceGraph,
    IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory,
    IResourceDefinitionAccessor resourceDefinitionAccessor, IQueryableBuilder queryableBuilder)
    : EntityFrameworkCoreRepository<Building, long>(request, targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders,
        loggerFactory, resourceDefinitionAccessor, queryableBuilder)
{
    public override async Task<Building> GetForCreateAsync(Type resourceClrType, long id, CancellationToken cancellationToken)
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
