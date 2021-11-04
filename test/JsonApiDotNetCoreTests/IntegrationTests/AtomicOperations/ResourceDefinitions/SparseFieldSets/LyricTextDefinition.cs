using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.ResourceDefinitions.SparseFieldSets
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class LyricTextDefinition : HitCountingResourceDefinition<Lyric, long>
    {
        private readonly LyricPermissionProvider _lyricPermissionProvider;

        protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet;

        public LyricTextDefinition(IResourceGraph resourceGraph, LyricPermissionProvider lyricPermissionProvider, ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph, hitCounter)
        {
            _lyricPermissionProvider = lyricPermissionProvider;
        }

        public override SparseFieldSetExpression? OnApplySparseFieldSet(SparseFieldSetExpression? existingSparseFieldSet)
        {
            base.OnApplySparseFieldSet(existingSparseFieldSet);

            return _lyricPermissionProvider.CanViewText ? existingSparseFieldSet : existingSparseFieldSet.Excluding<Lyric>(lyric => lyric.Text, ResourceGraph);
        }
    }
}
