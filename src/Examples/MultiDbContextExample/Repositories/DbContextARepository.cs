using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using MultiDbContextExample.Data;

namespace MultiDbContextExample.Repositories;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class DbContextARepository<TResource> : EntityFrameworkCoreRepository<TResource, int>
    where TResource : class, IIdentifiable<int>
{
    public DbContextARepository(IJsonApiRequest request, ITargetedFields targetedFields, DbContextResolver<DbContextA> dbContextResolver,
        IResourceGraph resourceGraph, IResourceFactory resourceFactory, IResourceDefinitionAccessor resourceDefinitionAccessor,
        IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory)
        : base(request, targetedFields, dbContextResolver, resourceGraph, resourceFactory, resourceDefinitionAccessor, constraintProviders, loggerFactory)
    {
    }
}
