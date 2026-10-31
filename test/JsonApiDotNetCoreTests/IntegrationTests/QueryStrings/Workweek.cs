using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Workweek
{
    [Attr]
    public DayOfWeek FirstDay { get; set; }

    [Attr(IsCompound = true)]
    public OfficeHours? OfficeHours { get; set; }

    [Attr]
    public List<DayOfWeek> WorkDays { get; set; } = [];

    [Attr(IsCompound = true)]
    public List<OfficeHours> Breaks { get; set; } = [];

    [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort))]
    public string? InternalNote { get; set; }

    [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowView)]
    public string? Secret { get; set; }
}
