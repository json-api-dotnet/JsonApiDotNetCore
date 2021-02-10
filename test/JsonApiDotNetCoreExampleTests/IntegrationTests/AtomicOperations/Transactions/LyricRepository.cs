using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.Transactions
{
    public sealed class LyricRepository : EntityFrameworkCoreRepository<Lyric, long>
    {
        private readonly ExtraDbContext _extraDbContext;

        public override Guid? TransactionId => _extraDbContext.Database.CurrentTransaction.TransactionId;

        public LyricRepository(ExtraDbContext extraDbContext, ITargetedFields targetedFields,
            IDbContextResolver contextResolver, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
            IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory)
            : base(targetedFields, contextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory)
        {
            _extraDbContext = extraDbContext;

            extraDbContext.Database.EnsureCreated();
            extraDbContext.Database.BeginTransaction();
        }
    }
}
