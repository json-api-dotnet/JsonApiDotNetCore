using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.Transactions
{
    public sealed class MusicTrackRepository : EntityFrameworkCoreRepository<MusicTrack, Guid>
    {
        public override Guid? TransactionId => null;

        public MusicTrackRepository(ITargetedFields targetedFields, IDbContextResolver contextResolver,
            IResourceGraph resourceGraph, IResourceFactory resourceFactory,
            IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory)
            : base(targetedFields, contextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory)
        {
        }
    }
}
