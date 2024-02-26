using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.ResourceDefinitions.SparseFieldSets;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class LyricTextDefinition(IResourceGraph resourceGraph, LyricPermissionProvider lyricPermissionProvider, ResourceDefinitionHitCounter hitCounter)
    : HitCountingResourceDefinition<Lyric, long>(resourceGraph, hitCounter)
{
    private readonly LyricPermissionProvider _lyricPermissionProvider = lyricPermissionProvider;

    protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet;

    public override SparseFieldSetExpression? OnApplySparseFieldSet(SparseFieldSetExpression? existingSparseFieldSet)
    {
        base.OnApplySparseFieldSet(existingSparseFieldSet);

        return _lyricPermissionProvider.CanViewText ? existingSparseFieldSet : existingSparseFieldSet.Excluding<Lyric>(lyric => lyric.Text, ResourceGraph);
    }
}
