using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.QueryStrings")]
public sealed class Calendar : Identifiable<int>
{
    [Attr]
    public string? TimeZone { get; set; }

    [Attr]
    public bool ShowWeekNumbers { get; set; }

    [Attr]
    public int DefaultAppointmentDurationInMinutes { get; set; }

    [HasOne]
    public Appointment? MostRecentAppointment { get; set; }

    [HasMany(Capabilities = HasManyCapabilities.All & ~(HasManyCapabilities.AllowView | HasManyCapabilities.AllowFilter))]
    public ISet<Appointment> Appointments { get; set; } = new HashSet<Appointment>();
}
