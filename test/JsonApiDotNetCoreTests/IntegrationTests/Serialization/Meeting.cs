using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Serialization")]
public sealed class Meeting : Identifiable<Guid>
{
    [Attr]
    public string Title { get; set; } = null!;

    [Attr]
    public DateTimeOffset StartTime { get; set; }

    [Attr]
    public TimeSpan Duration { get; set; }

    [Attr]
    [NotMapped]
    [AllowNull]
    public MeetingLocation Location
    {
        get =>
            new()
            {
                Latitude = Latitude,
                Longitude = Longitude
            };
        set
        {
            Latitude = value?.Latitude ?? double.NaN;
            Longitude = value?.Longitude ?? double.NaN;
        }
    }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    [HasMany]
    public IList<MeetingAttendee> Attendees { get; set; } = new List<MeetingAttendee>();
}
