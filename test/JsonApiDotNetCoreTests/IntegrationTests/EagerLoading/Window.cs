using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[NoResource]
public sealed class Window
{
    public int Id { get; set; }
    public int HeightInCentimeters { get; set; }
    public int WidthInCentimeters { get; set; }
}
