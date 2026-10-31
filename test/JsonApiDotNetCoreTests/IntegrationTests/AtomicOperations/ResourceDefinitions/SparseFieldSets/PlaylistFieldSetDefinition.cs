using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.ResourceDefinitions.SparseFieldSets;

/// <summary>
/// Returns a different nested sparse fieldset for the first and the second atomic operation. The hook is invoked once per operation (when creating a
/// resource, only the fields to serialize are determined), which proves the evaluator is reset between operations.
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

        _provider.CallCount++;

        return _provider.CallCount == 1
            ? existingSparseFieldSet.Excluding<Playlist>(playlist => playlist.Credits, ResourceGraph)
            : existingSparseFieldSet.Excluding<Playlist>(playlist => playlist.Recording, ResourceGraph);
    }
}
