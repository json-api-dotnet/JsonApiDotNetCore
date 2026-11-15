using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using MultiDbContextExample.Data;

namespace MultiDbContextExample.Repositories;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class DbContextBRepository<TResource>(
    IJsonApiRequest request, ITargetedFields targetedFields, DbContextResolver<DbContextB> dbContextResolver, IResourceGraph resourceGraph,
    IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory,
    IResourceDefinitionAccessor resourceDefinitionAccessor, IQueryableBuilder queryableBuilder)
    : EntityFrameworkCoreRepository<TResource, long>(request, targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders,
        loggerFactory, resourceDefinitionAccessor, queryableBuilder)
    where TResource : class, IIdentifiable<long>;
