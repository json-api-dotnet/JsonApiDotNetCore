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
public sealed class LyricRepository : EntityFrameworkCoreRepository<Lyric, long>
{
    private readonly ExtraDbContext _extraDbContext;

    public override string? TransactionId => _extraDbContext.Database.CurrentTransaction?.TransactionId.ToString();

    public LyricRepository(IJsonApiRequest request, ITargetedFields targetedFields, IDbContextResolver dbContextResolver, IResourceGraph resourceGraph,
        IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory,
        IResourceDefinitionAccessor resourceDefinitionAccessor, IQueryableBuilder queryableBuilder, ExtraDbContext extraDbContext)
        : base(request, targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory, resourceDefinitionAccessor,
            queryableBuilder)
    {
        _extraDbContext = extraDbContext;

        extraDbContext.Database.EnsureCreated();
        extraDbContext.Database.BeginTransaction();
    }
}
