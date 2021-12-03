using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using MultiDbContextExample.Data;

namespace MultiDbContextExample.Repositories
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class DbContextBRepository<TResource> : EntityFrameworkCoreRepository<TResource, int>
        where TResource : class, IIdentifiable<int>
    {
        public DbContextBRepository(ITargetedFields targetedFields, DbContextResolver<DbContextB> dbContextResolver, IResourceGraph resourceGraph,
            IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory,
            IResourceDefinitionAccessor resourceDefinitionAccessor)
            : base(targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory, resourceDefinitionAccessor)
        {
        }
    }
}
