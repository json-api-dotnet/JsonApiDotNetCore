using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using MultiDbContextExample.Data;

namespace MultiDbContextExample.Repositories
{
    public class DbContextBRepository<TResource> : EntityFrameworkCoreRepository<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public DbContextBRepository(ITargetedFields targetedFields, DbContextResolver<DbContextB> contextResolver,
            IResourceGraph resourceGraph, IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, 
            IGetResourcesByIds getResourcesByIds, ILoggerFactory loggerFactory)
            : base(targetedFields, contextResolver, resourceGraph, resourceFactory,
                constraintProviders, getResourcesByIds, loggerFactory)
        {
        }
    }
}
