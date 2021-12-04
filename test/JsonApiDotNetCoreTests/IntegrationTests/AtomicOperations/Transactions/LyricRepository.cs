using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Transactions;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class LyricRepository : EntityFrameworkCoreRepository<Lyric, long>
{
    private readonly ExtraDbContext _extraDbContext;

    public override string? TransactionId => _extraDbContext.Database.CurrentTransaction?.TransactionId.ToString();

    public LyricRepository(ExtraDbContext extraDbContext, ITargetedFields targetedFields, IDbContextResolver dbContextResolver, IResourceGraph resourceGraph,
        IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory,
        IResourceDefinitionAccessor resourceDefinitionAccessor)
        : base(targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory, resourceDefinitionAccessor)
    {
        _extraDbContext = extraDbContext;

        extraDbContext.Database.EnsureCreated();
        extraDbContext.Database.BeginTransaction();
    }
}
