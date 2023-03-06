using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Transactions;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class MusicTrackRepository : EntityFrameworkCoreRepository<MusicTrack, Guid>
{
    public override string? TransactionId => null;

    public MusicTrackRepository(IJsonApiRequest request, ITargetedFields targetedFields, IDbContextResolver dbContextResolver, IResourceGraph resourceGraph,
        IResourceFactory resourceFactory, IResourceDefinitionAccessor resourceDefinitionAccessor, IEnumerable<IQueryConstraintProvider> constraintProviders,
        ILoggerFactory loggerFactory)
        : base(request, targetedFields, dbContextResolver, resourceGraph, resourceFactory, resourceDefinitionAccessor, constraintProviders, loggerFactory)
    {
    }
}
