using System;
using Bogus;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Serialization
{
    internal sealed class SerializationFakers : FakerContainer
    {
        private static readonly TimeSpan[] _meetingDurations =
        {
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(45),
            TimeSpan.FromMinutes(60)
        };

        private readonly Lazy<Faker<Meeting>> _lazyMeetingFaker = new Lazy<Faker<Meeting>>(() =>
            new Faker<Meeting>()
                .UseSeed(GetFakerSeed())
                .RuleFor(meeting => meeting.Title, f => f.Lorem.Word())
                .RuleFor(meeting => meeting.StartTime, f => TruncateToWholeMilliseconds(f.Date.FutureOffset()))
                .RuleFor(meeting => meeting.Duration, f => f.PickRandom(_meetingDurations))
                .RuleFor(meeting => meeting.Latitude, f => f.Address.Latitude())
                .RuleFor(meeting => meeting.Longitude, f => f.Address.Longitude()));

        private readonly Lazy<Faker<MeetingAttendee>> _lazyMeetingAttendeeFaker = new Lazy<Faker<MeetingAttendee>>(() =>
            new Faker<MeetingAttendee>()
                .UseSeed(GetFakerSeed())
                .RuleFor(attendee => attendee.DisplayName, f => f.Random.Utf16String()));

        public Faker<Meeting> Meeting => _lazyMeetingFaker.Value;
        public Faker<MeetingAttendee> MeetingAttendee => _lazyMeetingAttendeeFaker.Value;

        private static DateTimeOffset TruncateToWholeMilliseconds(DateTimeOffset value)
        {
            var ticksToSubtract = value.DateTime.Ticks % TimeSpan.TicksPerMillisecond;
            var ticksInWholeMilliseconds = value.DateTime.Ticks - ticksToSubtract;

            return new DateTimeOffset(new DateTime(ticksInWholeMilliseconds), value.Offset);
        }
    }
}
