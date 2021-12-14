using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Meta;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class MusicTrackMetaDefinition : HitCountingResourceDefinition<MusicTrack, Guid>
{
    protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.GetMeta;

    public MusicTrackMetaDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter)
        : base(resourceGraph, hitCounter)
    {
    }

    public override IDictionary<string, object?> GetMeta(MusicTrack resource)
    {
        base.GetMeta(resource);

        return new Dictionary<string, object?>
        {
            ["Copyright"] = $"(C) {resource.ReleasedAt.Year}. All rights reserved."
        };
    }
}
