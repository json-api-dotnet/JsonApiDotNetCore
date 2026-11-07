using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.QueryStrings")]
public sealed class Appointment : Identifiable<long>
{
    [Attr]
    public string Title { get; set; } = null!;

    [Attr]
    public string? Description { get; set; }

    [Attr]
    public DateTimeOffset StartTime { get; set; }

    [Attr]
    public DateTimeOffset EndTime { get; set; }

    [Attr(IsCompound = true)]
    public AppointmentLocation? Location { get; set; }

    [Attr(IsCompound = true)]
    public List<Attendee> Attendees { get; set; } = [];

    [Attr(IsCompound = true)]
    public AppointmentCost Cost { get; set; } = new();

    [Attr(IsCompound = true)]
    public AppointmentCoordinates? MeetingPoint { get; set; }

    [Attr(IsCompound = true)]
    public AppointmentTiming Timing { get; set; }

    [Attr(IsCompound = true)]
    public List<Attendee>? WaitingList { get; set; }

    [Attr]
    public List<string>? Keywords { get; set; }

    [HasOne]
    public Calendar? Calendar { get; set; }

    [HasMany]
    public ISet<Calendar> PinnedInCalendars { get; set; } = new HashSet<Calendar>();

    [HasMany(DisablePagination = true)]
    public IList<Reminder> Reminders { get; set; } = new List<Reminder>();
}
