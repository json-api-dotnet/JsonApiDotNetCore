using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using MultiDbContextExample.Data;

namespace MultiDbContextExample.Repositories;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class DbContextBRepository<TResource>(
    ITargetedFields targetedFields, DbContextResolver<DbContextB> dbContextResolver, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
    IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory, IResourceDefinitionAccessor resourceDefinitionAccessor)
    : EntityFrameworkCoreRepository<TResource, int>(targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory,
        resourceDefinitionAccessor)
    where TResource : class, IIdentifiable<int>;
