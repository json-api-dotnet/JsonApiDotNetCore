using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.SparseFieldSets;

/// <summary>
/// Enables sparse fieldset tests to verify which fields were (not) retrieved from the database.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class ResultCapturingRepository<TResource, TId>(
    ITargetedFields targetedFields, IDbContextResolver dbContextResolver, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
    IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory, IResourceDefinitionAccessor resourceDefinitionAccessor,
    ResourceCaptureStore captureStore)
    : EntityFrameworkCoreRepository<TResource, TId>(targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory,
        resourceDefinitionAccessor)
    where TResource : class, IIdentifiable<TId>
{
    private readonly ResourceCaptureStore _captureStore = captureStore;

    public override async Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TResource> resources = await base.GetAsync(queryLayer, cancellationToken);

        _captureStore.Add(resources);

        return resources;
    }
}
