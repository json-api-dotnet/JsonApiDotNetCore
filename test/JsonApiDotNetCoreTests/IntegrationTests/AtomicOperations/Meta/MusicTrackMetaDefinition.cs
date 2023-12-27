using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Meta;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class MusicTrackMetaDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter)
    : HitCountingResourceDefinition<MusicTrack, Guid>(resourceGraph, hitCounter)
{
    protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.GetMeta;

    public override IDictionary<string, object?> GetMeta(MusicTrack resource)
    {
        base.GetMeta(resource);

        return new Dictionary<string, object?>
        {
            ["Copyright"] = $"(C) {resource.ReleasedAt.Year}. All rights reserved."
        };
    }
}
