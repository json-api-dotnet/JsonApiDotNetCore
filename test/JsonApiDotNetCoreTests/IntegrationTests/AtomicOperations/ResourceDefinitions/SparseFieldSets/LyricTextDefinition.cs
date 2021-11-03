using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.ResourceDefinitions.SparseFieldSets
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class LyricTextDefinition : JsonApiResourceDefinition<Lyric, long>
    {
        private readonly LyricPermissionProvider _lyricPermissionProvider;
        private readonly ResourceDefinitionHitCounter _hitCounter;

        public LyricTextDefinition(IResourceGraph resourceGraph, LyricPermissionProvider lyricPermissionProvider, ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph)
        {
            _lyricPermissionProvider = lyricPermissionProvider;
            _hitCounter = hitCounter;
        }

        public override SparseFieldSetExpression? OnApplySparseFieldSet(SparseFieldSetExpression? existingSparseFieldSet)
        {
            _hitCounter.TrackInvocation<Lyric>(ResourceDefinitionHitCounter.ExtensibilityPoint.OnApplySparseFieldSet);

            return _lyricPermissionProvider.CanViewText
                ? base.OnApplySparseFieldSet(existingSparseFieldSet)
                : existingSparseFieldSet.Excluding<Lyric>(lyric => lyric.Text, ResourceGraph);
        }
    }
}
