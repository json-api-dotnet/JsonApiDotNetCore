using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

internal sealed class QueryStringFakers
{
    private readonly Lazy<Faker<Blog>> _lazyBlogFaker = new(() => new Faker<Blog>()
        .MakeDeterministic()
        .RuleFor(blog => blog.Title, faker => faker.Lorem.Word())
        .RuleFor(blog => blog.PlatformName, faker => faker.Company.CompanyName()));

    private readonly Lazy<Faker<BlogPost>> _lazyBlogPostFaker = new(() => new Faker<BlogPost>()
        .MakeDeterministic()
        .RuleFor(blogPost => blogPost.Caption, faker => faker.Lorem.Sentence())
        .RuleFor(blogPost => blogPost.Url, faker => faker.Internet.Url()));

    private readonly Lazy<Faker<Label>> _lazyLabelFaker = new(() => new Faker<Label>()
        .MakeDeterministic()
        .RuleFor(label => label.Name, faker => faker.Lorem.Word())
        .RuleFor(label => label.Color, faker => faker.PickRandom<LabelColor>()));

    private readonly Lazy<Faker<Comment>> _lazyCommentFaker = new(() => new Faker<Comment>()
        .MakeDeterministic()
        .RuleFor(comment => comment.Text, faker => faker.Lorem.Paragraph())
        .RuleFor(comment => comment.CreatedAt, faker => faker.Date.Past().TruncateToWholeMilliseconds())
        .RuleFor(comment => comment.NumStars, faker => faker.Random.Int(0, 10)));

    private readonly Lazy<Faker<WebAccount>> _lazyWebAccountFaker = new(() => new Faker<WebAccount>()
        .MakeDeterministic()
        .RuleFor(webAccount => webAccount.UserName, faker => faker.Person.UserName)
        .RuleFor(webAccount => webAccount.Password, faker => faker.Internet.Password())
        .RuleFor(webAccount => webAccount.DisplayName, faker => faker.Person.FullName)
        .RuleFor(webAccount => webAccount.DateOfBirth, faker => faker.Person.DateOfBirth.TruncateToWholeMilliseconds())
        .RuleFor(webAccount => webAccount.EmailAddress, faker => faker.Internet.Email()));

    private readonly Lazy<Faker<LoginAttempt>> _lazyLoginAttemptFaker = new(() => new Faker<LoginAttempt>()
        .MakeDeterministic()
        .RuleFor(loginAttempt => loginAttempt.TriedAt, faker => faker.Date.PastOffset().TruncateToWholeMilliseconds())
        .RuleFor(loginAttempt => loginAttempt.IsSucceeded, faker => faker.Random.Bool()));

    private readonly Lazy<Faker<AccountPreferences>> _lazyAccountPreferencesFaker = new(() => new Faker<AccountPreferences>()
        .MakeDeterministic()
        .RuleFor(accountPreferences => accountPreferences.UseDarkTheme, faker => faker.Random.Bool()));

    private readonly Lazy<Faker<Man>> _lazyManFaker = new(() => new Faker<Man>()
        .MakeDeterministic()
        .RuleFor(man => man.Name, faker => faker.Name.FullName())
        .RuleFor(man => man.HasBeard, faker => faker.Random.Bool())
        .RuleFor(man => man.Age, faker => faker.Random.Int(10, 90)));

    private readonly Lazy<Faker<Woman>> _lazyWomanFaker = new(() => new Faker<Woman>()
        .MakeDeterministic()
        .RuleFor(woman => woman.Name, faker => faker.Name.FullName())
        .RuleFor(woman => woman.MaidenName, faker => faker.Name.LastName())
        .RuleFor(woman => woman.Age, faker => faker.Random.Int(10, 90)));

    private readonly Lazy<Faker<Calendar>> _lazyCalendarFaker = new(() => new Faker<Calendar>()
        .MakeDeterministic()
        .RuleFor(calendar => calendar.TimeZone, faker => faker.Date.TimeZoneString())
        .RuleFor(calendar => calendar.ShowWeekNumbers, faker => faker.Random.Bool())
        .RuleFor(calendar => calendar.DefaultAppointmentDurationInMinutes, faker => faker.PickRandom(15, 30, 45, 60)));

    private readonly Lazy<Faker<Appointment>> _lazyAppointmentFaker = new(() => new Faker<Appointment>()
        .MakeDeterministic()
        .RuleFor(appointment => appointment.Title, faker => faker.Random.Word())
        .RuleFor(appointment => appointment.Description, faker => faker.Lorem.Sentence())
        .RuleFor(appointment => appointment.StartTime, faker => faker.Date.FutureOffset().TruncateToWholeMilliseconds())
        .RuleFor(appointment => appointment.EndTime, (faker, appointment) => appointment.StartTime.AddHours(faker.Random.Double(1, 4))));

    private readonly Lazy<Faker<Reminder>> _lazyReminderFaker = new(() => new Faker<Reminder>()
        .MakeDeterministic()
        .RuleFor(reminder => reminder.RemindsAt, faker => faker.Date.Future().TruncateToWholeMilliseconds()));

    public Faker<Blog> Blog => _lazyBlogFaker.Value;
    public Faker<BlogPost> BlogPost => _lazyBlogPostFaker.Value;
    public Faker<Label> Label => _lazyLabelFaker.Value;
    public Faker<Comment> Comment => _lazyCommentFaker.Value;
    public Faker<WebAccount> WebAccount => _lazyWebAccountFaker.Value;
    public Faker<LoginAttempt> LoginAttempt => _lazyLoginAttemptFaker.Value;
    public Faker<AccountPreferences> AccountPreferences => _lazyAccountPreferencesFaker.Value;
    public Faker<Man> Man => _lazyManFaker.Value;
    public Faker<Woman> Woman => _lazyWomanFaker.Value;
    public Faker<Calendar> Calendar => _lazyCalendarFaker.Value;
    public Faker<Appointment> Appointment => _lazyAppointmentFaker.Value;
    public Faker<Reminder> Reminder => _lazyReminderFaker.Value;
}
