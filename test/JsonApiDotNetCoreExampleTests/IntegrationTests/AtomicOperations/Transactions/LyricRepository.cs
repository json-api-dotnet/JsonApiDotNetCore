using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.Transactions
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class LyricRepository : EntityFrameworkCoreRepository<Lyric, long>
    {
        private readonly ExtraDbContext _extraDbContext;

        public override string TransactionId => _extraDbContext.Database.CurrentTransaction.TransactionId.ToString();

        public LyricRepository(ExtraDbContext extraDbContext, ITargetedFields targetedFields, IDbContextResolver contextResolver, IResourceGraph resourceGraph,
            IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory)
            : base(targetedFields, contextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory)
        {
            _extraDbContext = extraDbContext;

            extraDbContext.Database.EnsureCreated();
            extraDbContext.Database.BeginTransaction();
        }
    }
}
