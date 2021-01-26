using System;
using Bogus;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    internal sealed class QueryStringFakers : FakerContainer
    {
        private readonly Lazy<Faker<Calendar>> _lazyCalendarFaker = new Lazy<Faker<Calendar>>(() =>
            new Faker<Calendar>()
                .UseSeed(GetFakerSeed())
                .RuleFor(calendar => calendar.TimeZone, f => f.Date.TimeZoneString())
                .RuleFor(calendar => calendar.DefaultAppointmentDurationInMinutes, f => f.PickRandom(15, 30, 45, 60)));

        private readonly Lazy<Faker<Appointment>> _lazyAppointmentFaker = new Lazy<Faker<Appointment>>(() =>
            new Faker<Appointment>()
                .UseSeed(GetFakerSeed())
                .RuleFor(appointment => appointment.Title, f => f.Random.Word())
                .RuleFor(appointment => appointment.StartTime, f => f.Date.FutureOffset())
                .RuleFor(appointment => appointment.EndTime, (f, appointment) => appointment.StartTime.AddHours(f.Random.Double(1, 4))));

        public Faker<Calendar> Calendar => _lazyCalendarFaker.Value;
        public Faker<Appointment> Appointment => _lazyAppointmentFaker.Value;
    }
}
