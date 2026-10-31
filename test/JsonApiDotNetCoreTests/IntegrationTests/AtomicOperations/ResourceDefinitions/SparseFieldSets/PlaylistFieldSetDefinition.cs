using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.ResourceDefinitions.SparseFieldSets;

/// <summary>
/// Returns a different nested sparse fieldset for the query of the first and the second atomic operation. The query side is recognized by the absence of
/// an existing fieldset. When the evaluator is not reset between operations, the hook is not invoked again for the second operation.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class PlaylistFieldSetDefinition(IResourceGraph resourceGraph, PlaylistFieldSetProvider provider)
    : JsonApiResourceDefinition<Playlist, long>(resourceGraph)
{
    private readonly PlaylistFieldSetProvider _provider = provider;

    public override SparseFieldSetExpression? OnApplySparseFieldSet(SparseFieldSetExpression? existingSparseFieldSet)
    {
        if (!_provider.IsEnabled)
        {
            return existingSparseFieldSet;
        }

        if (existingSparseFieldSet != null)
        {
            // Serializer side.
            return existingSparseFieldSet;
        }

        _provider.QueryCallCount++;

        return _provider.QueryCallCount == 1
            ? existingSparseFieldSet.Excluding<Playlist>(playlist => playlist.Credits, ResourceGraph)
            : existingSparseFieldSet.Excluding<Playlist>(playlist => playlist.Recording, ResourceGraph);
    }
}
