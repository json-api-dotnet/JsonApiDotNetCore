using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class BuildingRepository(
    ITargetedFields targetedFields, IDbContextResolver dbContextResolver, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
    IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory, IResourceDefinitionAccessor resourceDefinitionAccessor)
    : EntityFrameworkCoreRepository<Building, int>(targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory,
        resourceDefinitionAccessor)
{
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
