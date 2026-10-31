using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ConstellationOrigin
{
    [Attr]
    public string? Culture { get; set; }

    [Attr]
    public int? CatalogedInCentury { get; set; }
}
