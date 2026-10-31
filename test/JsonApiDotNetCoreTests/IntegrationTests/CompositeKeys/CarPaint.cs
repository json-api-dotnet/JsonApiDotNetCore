using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CarPaint
{
    [Attr]
    public string Color { get; set; } = null!;

    [Attr]
    public List<string> Coats { get; set; } = [];
}