using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class ConstellationDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter)
    : HitCountingResourceDefinition<Moon, int>(resourceGraph, hitCounter)
{
    protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.Reading;
}
