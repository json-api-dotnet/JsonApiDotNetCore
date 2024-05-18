using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization;

internal sealed class SerializationFakers
{
    private static readonly TimeSpan[] MeetingDurations =
    [
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromMinutes(45),
        TimeSpan.FromMinutes(60)
    ];

    private readonly Lazy<Faker<Meeting>> _lazyMeetingFaker = new(() => new Faker<Meeting>()
        .MakeDeterministic()
        .RuleFor(meeting => meeting.Title, faker => faker.Lorem.Word())
        .RuleFor(meeting => meeting.StartTime, faker => faker.Date.FutureOffset().TruncateToWholeMilliseconds())
        .RuleFor(meeting => meeting.Duration, faker => faker.PickRandom(MeetingDurations))
        .RuleFor(meeting => meeting.Latitude, faker => faker.Address.Latitude())
        .RuleFor(meeting => meeting.Longitude, faker => faker.Address.Longitude()));

    private readonly Lazy<Faker<MeetingAttendee>> _lazyMeetingAttendeeFaker = new(() => new Faker<MeetingAttendee>()
        .MakeDeterministic()
        .RuleFor(attendee => attendee.DisplayName, faker => faker.Random.Utf16String())
        .RuleFor(attendee => attendee.HomeAddress, faker => new Address
        {
            Street = faker.Address.StreetAddress(),
            ZipCode = faker.Address.ZipCode(),
            City = faker.Address.City(),
            Country = faker.Address.Country()
        }));

    public Faker<Meeting> Meeting => _lazyMeetingFaker.Value;
    public Faker<MeetingAttendee> MeetingAttendee => _lazyMeetingAttendeeFaker.Value;
}
