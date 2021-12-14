using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class SwimmingPool : Identifiable<int>
{
    [Attr]
    public bool IsIndoor { get; set; }

    [HasMany]
    public IList<WaterSlide> WaterSlides { get; set; } = new List<WaterSlide>();

    [HasMany]
    public IList<DivingBoard> DivingBoards { get; set; } = new List<DivingBoard>();
}
