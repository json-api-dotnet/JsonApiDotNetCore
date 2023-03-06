using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
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

    public LyricRepository(ExtraDbContext extraDbContext, IJsonApiRequest request, ITargetedFields targetedFields, IDbContextResolver dbContextResolver,
        IResourceGraph resourceGraph, IResourceFactory resourceFactory, IResourceDefinitionAccessor resourceDefinitionAccessor,
        IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory)
        : base(request, targetedFields, dbContextResolver, resourceGraph, resourceFactory, resourceDefinitionAccessor, constraintProviders, loggerFactory)
    {
        _extraDbContext = extraDbContext;

        extraDbContext.Database.EnsureCreated();
        extraDbContext.Database.BeginTransaction();
    }
}
