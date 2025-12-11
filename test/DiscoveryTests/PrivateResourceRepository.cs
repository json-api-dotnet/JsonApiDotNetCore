using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace DiscoveryTests;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class PrivateResourceRepository(
    ITargetedFields targetedFields, IDbContextResolver dbContextResolver, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
    IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory, IResourceDefinitionAccessor resourceDefinitionAccessor)
    : EntityFrameworkCoreRepository<PrivateResource, long>(targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders,
        loggerFactory, resourceDefinitionAccessor);
