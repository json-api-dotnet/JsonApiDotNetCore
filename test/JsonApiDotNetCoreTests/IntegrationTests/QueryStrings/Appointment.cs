using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.QueryStrings")]
public sealed class Appointment : Identifiable<long>
{
    [Attr]
    public required string Title { get; set; }

    [Attr]
    public string? Description { get; set; }

    [Attr]
    public DateTimeOffset StartTime { get; set; }

    [Attr]
    public DateTimeOffset EndTime { get; set; }

    [HasOne]
    public Calendar? Calendar { get; set; }

    [HasMany(DisablePagination = true)]
    public IList<Reminder> Reminders { get; set; } = new List<Reminder>();
}
