using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.ResourceDefinitions
{
    public sealed class LyricTextDefinition : JsonApiResourceDefinition<Lyric, long>
    {
        private readonly LyricPermissionProvider _lyricPermissionProvider;

        public LyricTextDefinition(IResourceGraph resourceGraph, LyricPermissionProvider lyricPermissionProvider)
            : base(resourceGraph)
        {
            _lyricPermissionProvider = lyricPermissionProvider;
        }

        public override SparseFieldSetExpression OnApplySparseFieldSet(SparseFieldSetExpression existingSparseFieldSet)
        {
            _lyricPermissionProvider.HitCount++;

            return _lyricPermissionProvider.CanViewText
                ? base.OnApplySparseFieldSet(existingSparseFieldSet)
                : existingSparseFieldSet.Excluding<Lyric>(lyric => lyric.Text, ResourceGraph);
        }
    }
}
