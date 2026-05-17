using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Transactions;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class MusicTrackRepository(
    ITargetedFields targetedFields, IDbContextResolver dbContextResolver, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
    IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory, IResourceDefinitionAccessor resourceDefinitionAccessor,
    IJsonApiOptions options)
    : EntityFrameworkCoreRepository<MusicTrack, Guid>(targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory,
        resourceDefinitionAccessor, options)
{
    public override string? TransactionId => null;
}
