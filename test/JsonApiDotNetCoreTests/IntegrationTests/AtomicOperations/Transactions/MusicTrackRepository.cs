using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Transactions;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class MusicTrackRepository(
    IJsonApiRequest request, ITargetedFields targetedFields, IDbContextResolver dbContextResolver, IResourceGraph resourceGraph,
    IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory,
    IResourceDefinitionAccessor resourceDefinitionAccessor, IQueryableBuilder queryableBuilder)
    : EntityFrameworkCoreRepository<MusicTrack, Guid>(request, targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders,
        loggerFactory, resourceDefinitionAccessor, queryableBuilder)
{
    public override string? TransactionId => null;
}
