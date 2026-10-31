using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.ResourceDefinitions.SparseFieldSets;

/// <summary>
/// Performs a query that honors the sparse fieldset of the resource definition after each update, so the query side of the evaluator is used while atomic
/// operations are being processed.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class PlaylistQueryService(
    IResourceRepositoryAccessor repositoryAccessor, IQueryLayerComposer queryLayerComposer, IPaginationContext paginationContext, IJsonApiOptions options,
    ILoggerFactory loggerFactory, IJsonApiRequest request, IResourceChangeTracker<Playlist> resourceChangeTracker,
    IResourceDefinitionAccessor resourceDefinitionAccessor)
    : JsonApiResourceService<Playlist, long>(repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory, request, resourceChangeTracker,
        resourceDefinitionAccessor)
{
    public override async Task<Playlist?> UpdateAsync([DisallowNull] long id, Playlist resource, CancellationToken cancellationToken)
    {
        Playlist? result = await base.UpdateAsync(id, resource, cancellationToken);

        _ = await GetPrimaryResourceByIdAsync(id, TopFieldSelection.PreserveExisting, cancellationToken);

        return result;
    }
}
