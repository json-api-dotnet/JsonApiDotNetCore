using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.SparseFieldSets;

/// <summary>
/// Enables sparse fieldset tests to verify which fields were (not) retrieved from the database.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class ResultCapturingRepository<TResource, TId> : EntityFrameworkCoreRepository<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private readonly ResourceCaptureStore _captureStore;

    public ResultCapturingRepository(ResourceCaptureStore captureStore, IJsonApiRequest request, ITargetedFields targetedFields,
        IDbContextResolver dbContextResolver, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
        IResourceDefinitionAccessor resourceDefinitionAccessor, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory)
        : base(request, targetedFields, dbContextResolver, resourceGraph, resourceFactory, resourceDefinitionAccessor, constraintProviders, loggerFactory)
    {
        _captureStore = captureStore;
    }

    public override async Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TResource> resources = await base.GetAsync(queryLayer, cancellationToken);

        _captureStore.Add(resources);

        return resources;
    }
}
