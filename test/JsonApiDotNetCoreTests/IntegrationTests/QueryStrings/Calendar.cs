using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.QueryStrings")]
public sealed class Calendar : Identifiable<long>
{
    [Attr]
    public string? TimeZone { get; set; }

    [Attr]
    public bool ShowWeekNumbers { get; set; }

    [Attr]
    public int DefaultAppointmentDurationInMinutes { get; set; }

    [Attr(IsCompound = true)]
    public Workweek? Workweek { get; set; }

    [Attr(IsCompound = true)]
    public List<Holiday> Holidays { get; set; } = [];

    [Attr(IsCompound = true, Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowFilter)]
    public CalendarAudit? Audit { get; set; }

    [HasOne]
    public Appointment? MostRecentAppointment { get; set; }

    [HasMany(Capabilities = HasManyCapabilities.All & ~(HasManyCapabilities.AllowView | HasManyCapabilities.AllowFilter))]
    public ISet<Appointment> Appointments { get; set; } = new HashSet<Appointment>();

#pragma warning disable CS0618 // Type or member is obsolete
    [HasOne(CanInclude = false)]
#pragma warning restore CS0618 // Type or member is obsolete
    public Appointment? FirstAppointment { get; set; }

    [HasMany]
    public ISet<Appointment> PinnedAppointments { get; set; } = new HashSet<Appointment>();
}
