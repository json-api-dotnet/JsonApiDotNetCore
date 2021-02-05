using System;
using Bogus;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    internal sealed class QueryStringFakers : FakerContainer
    {
        private readonly Lazy<Faker<Blog>> _lazyBlogFaker = new Lazy<Faker<Blog>>(() =>
            new Faker<Blog>()
                .UseSeed(GetFakerSeed())
                .RuleFor(blog => blog.Title, f => f.Lorem.Word())
                .RuleFor(blog => blog.PlatformName, f => f.Company.CompanyName()));

        private readonly Lazy<Faker<BlogPost>> _lazyBlogPostFaker = new Lazy<Faker<BlogPost>>(() =>
            new Faker<BlogPost>()
                .UseSeed(GetFakerSeed())
                .RuleFor(blogPost => blogPost.Caption, f => f.Lorem.Sentence())
                .RuleFor(blogPost => blogPost.Url, f => f.Internet.Url()));

        private readonly Lazy<Faker<Label>> _lazyLabelFaker = new Lazy<Faker<Label>>(() =>
            new Faker<Label>()
                .UseSeed(GetFakerSeed())
                .RuleFor(label => label.Name, f => f.Lorem.Word())
                .RuleFor(label => label.Color, f => f.PickRandom<LabelColor>()));

        private readonly Lazy<Faker<Comment>> _lazyCommentFaker = new Lazy<Faker<Comment>>(() =>
            new Faker<Comment>()
                .UseSeed(GetFakerSeed())
                .RuleFor(comment => comment.Text, f => f.Lorem.Paragraph())
                .RuleFor(comment => comment.CreatedAt, f => f.Date.Past()));

        private readonly Lazy<Faker<WebAccount>> _lazyWebAccountFaker = new Lazy<Faker<WebAccount>>(() =>
            new Faker<WebAccount>()
                .UseSeed(GetFakerSeed())
                .RuleFor(webAccount => webAccount.UserName, f => f.Person.UserName)
                .RuleFor(webAccount => webAccount.Password, f => f.Internet.Password())
                .RuleFor(webAccount => webAccount.DisplayName, f => f.Person.FullName)
                .RuleFor(webAccount => webAccount.DateOfBirth, f => f.Person.DateOfBirth)
                .RuleFor(webAccount => webAccount.EmailAddress, f => f.Internet.Email()));

        private readonly Lazy<Faker<AccountPreferences>> _lazyAccountPreferencesFaker = new Lazy<Faker<AccountPreferences>>(() =>
            new Faker<AccountPreferences>()
                .UseSeed(GetFakerSeed())
                .RuleFor(accountPreferences => accountPreferences.UseDarkTheme, f => f.Random.Bool()));

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

        public Faker<Blog> Blog => _lazyBlogFaker.Value;
        public Faker<BlogPost> BlogPost => _lazyBlogPostFaker.Value;
        public Faker<Label> Label => _lazyLabelFaker.Value;
        public Faker<Comment> Comment => _lazyCommentFaker.Value;
        public Faker<WebAccount> WebAccount => _lazyWebAccountFaker.Value;
        public Faker<AccountPreferences> AccountPreferences => _lazyAccountPreferencesFaker.Value;
        public Faker<Calendar> Calendar => _lazyCalendarFaker.Value;
        public Faker<Appointment> Appointment => _lazyAppointmentFaker.Value;
    }
}
