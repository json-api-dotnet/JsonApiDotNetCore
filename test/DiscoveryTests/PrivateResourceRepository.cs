using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace DiscoveryTests;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class PrivateResourceRepository : EntityFrameworkCoreRepository<PrivateResource, int>
{
    public PrivateResourceRepository(IJsonApiRequest request, ITargetedFields targetedFields, IDbContextResolver dbContextResolver,
        IResourceGraph resourceGraph, IResourceFactory resourceFactory, IResourceDefinitionAccessor resourceDefinitionAccessor,
        IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory)
        : base(request, targetedFields, dbContextResolver, resourceGraph, resourceFactory, resourceDefinitionAccessor, constraintProviders, loggerFactory)
    {
    }
}
