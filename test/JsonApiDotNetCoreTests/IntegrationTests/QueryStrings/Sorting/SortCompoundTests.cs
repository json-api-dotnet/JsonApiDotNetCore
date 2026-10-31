using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Sorting;

public sealed class SortCompoundTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public SortCompoundTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AppointmentsController>();
        testContext.UseController<CalendarsController>();
        testContext.UseController<RemindersController>();
        testContext.UseController<BlogsController>();
        testContext.UseController<BlogPostsController>();
    }

    [Fact]
    public async Task Can_sort_on_nested_attribute_ascending_and_descending()
    {
        // Arrange
        List<Appointment> appointments = _fakers.Appointment.GenerateList(4);
        appointments[0].Cost.Amount = 30;
        appointments[1].Cost.Amount = 10;
        appointments[2].Cost.Amount = 20;
        appointments[3].Cost.Amount = 10;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.AddRange(appointments);
            await dbContext.SaveChangesAsync();
        });

        const string ascendingRoute = "/appointments?sort=cost.amount,id";
        const string descendingRoute = "/appointments?sort=-cost.amount,id";

        // Act
        (HttpResponseMessage ascendingResponse, Document ascendingDocument) = await _testContext.ExecuteGetAsync<Document>(ascendingRoute);
        (HttpResponseMessage descendingResponse, Document descendingDocument) = await _testContext.ExecuteGetAsync<Document>(descendingRoute);

        // Assert
        ascendingResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        ascendingDocument.Data.ManyValue.Should().HaveCount(4);
        ascendingDocument.Data.ManyValue[0].Id.Should().Be(appointments[1].StringId);
        ascendingDocument.Data.ManyValue[1].Id.Should().Be(appointments[3].StringId);
        ascendingDocument.Data.ManyValue[2].Id.Should().Be(appointments[2].StringId);
        ascendingDocument.Data.ManyValue[3].Id.Should().Be(appointments[0].StringId);

        descendingResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        descendingDocument.Data.ManyValue.Should().HaveCount(4);
        descendingDocument.Data.ManyValue[0].Id.Should().Be(appointments[0].StringId);
        descendingDocument.Data.ManyValue[1].Id.Should().Be(appointments[2].StringId);
        descendingDocument.Data.ManyValue[2].Id.Should().Be(appointments[1].StringId);
        descendingDocument.Data.ManyValue[3].Id.Should().Be(appointments[3].StringId);
    }

    [Theory]
    [InlineData("/appointments?sort=location.address.city,location.address.street", "2,0,1,3")]
    [InlineData("/appointments?sort=location.address.city,-location.address.street", "2,1,0,3")]
    [InlineData("/appointments?sort=-location.address.city,location.address.street", "3,0,1,2")]
    public async Task Can_sort_on_multiple_attributes_of_nested_compound(string route, string expectedOrder)
    {
        // Arrange
        List<Appointment> appointments = _fakers.Appointment.GenerateList(4);

        appointments[0].Location = new AppointmentLocation
        {
            Address = new AppointmentAddress
            {
                City = "Smith",
                Street = "A"
            }
        };

        appointments[1].Location = new AppointmentLocation
        {
            Address = new AppointmentAddress
            {
                City = "Smith",
                Street = "Z"
            }
        };

        appointments[2].Location = new AppointmentLocation
        {
            Address = new AppointmentAddress
            {
                City = "Adams",
                Street = "M"
            }
        };

        appointments[3].Location = new AppointmentLocation
        {
            Address = null
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.AddRange(appointments);
            await dbContext.SaveChangesAsync();
        });

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string[] expectedIds = expectedOrder.Split(',').Select(index => appointments[int.Parse(index)].StringId!).ToArray();
        responseDocument.GetPrimaryIds().Should().Equal(expectedIds);
    }

    [Fact]
    public async Task Can_sort_through_to_one_and_compound()
    {
        // Arrange
        List<Calendar> calendars = _fakers.Calendar.GenerateList(5);

        // No most recent appointment.
        calendars[0].MostRecentAppointment = null;

        // Appointment without location.
        calendars[1].MostRecentAppointment = _fakers.Appointment.GenerateOne();
        calendars[1].MostRecentAppointment!.Location = null;

        calendars[2].MostRecentAppointment = _fakers.Appointment.GenerateOne();
        calendars[2].MostRecentAppointment!.Location = new AppointmentLocation
        {
            Address = new AppointmentAddress
            {
                City = "Berlin"
            }
        };

        calendars[3].MostRecentAppointment = _fakers.Appointment.GenerateOne();
        calendars[3].MostRecentAppointment!.Location = new AppointmentLocation
        {
            Address = new AppointmentAddress
            {
                City = "Amsterdam"
            }
        };

        // Location without address.
        calendars[4].MostRecentAppointment = _fakers.Appointment.GenerateOne();
        calendars[4].MostRecentAppointment!.Location = new AppointmentLocation
        {
            Name = "Office",
            Address = null
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.AddRange(calendars);
            await dbContext.SaveChangesAsync();
        });

        const string ascendingRoute = "/calendars?sort=mostRecentAppointment.location.address.city,id";
        const string descendingRoute = "/calendars?sort=-mostRecentAppointment.location.address.city,id";

        // Act
        (HttpResponseMessage ascendingResponse, Document ascendingDocument) = await _testContext.ExecuteGetAsync<Document>(ascendingRoute);
        (HttpResponseMessage descendingResponse, Document descendingDocument) = await _testContext.ExecuteGetAsync<Document>(descendingRoute);

        // Assert
        ascendingResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        // PostgreSQL sorts null last when ascending and first when descending.
        ascendingDocument.GetPrimaryIds().Take(2).Should().Equal(calendars[3].StringId, calendars[2].StringId);

        ascendingDocument.GetPrimaryIds().Skip(2).Should().Equal(calendars[0].StringId, calendars[1].StringId,
            calendars[4].StringId);

        descendingResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        descendingDocument.GetPrimaryIds().Take(3).Should().Equal(calendars[0].StringId, calendars[1].StringId,
            calendars[4].StringId);

        descendingDocument.GetPrimaryIds().Skip(3).Should().Equal(calendars[2].StringId, calendars[3].StringId);
    }

    [Theory]
    [InlineData("/appointments?sort=count(attendees),id", "1,2,0")]
    [InlineData("/appointments?sort=-count(attendees),id", "0,2,1")]
    [InlineData("/appointments?sort=count(keywords),id", "2,1,0")]
    [InlineData("/appointments?sort=-count(keywords),id", "0,1,2")]
    public async Task Can_sort_appointments_by_count_of_collection(string route, string expectedOrder)
    {
        // Arrange
        List<Appointment> appointments = _fakers.Appointment.GenerateList(3);

        appointments[0].Attendees =
        [
            new Attendee
            {
                Name = "A"
            },
            new Attendee
            {
                Name = "B"
            }
        ];

        appointments[1].Attendees = [];

        appointments[2].Attendees =
        [
            new Attendee
            {
                Name = "C"
            }
        ];

        appointments[0].Keywords = ["a", "b", "c"];
        appointments[1].Keywords = ["a"];
        appointments[2].Keywords = [];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.AddRange(appointments);
            await dbContext.SaveChangesAsync();
        });

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string[] expectedIds = expectedOrder.Split(',').Select(index => appointments[int.Parse(index)].StringId!).ToArray();
        responseDocument.GetPrimaryIds().Should().Equal(expectedIds);
    }

    [Theory]
    [InlineData("/calendars?sort=count(holidays),id", "2,1,0")]
    [InlineData("/calendars?sort=-count(holidays),id", "0,1,2")]
    [InlineData("/calendars?sort=count(workweek.workDays),id", "1,2,0")]
    [InlineData("/calendars?sort=-count(workweek.workDays),id", "0,2,1")]
    [InlineData("/calendars?sort=count(workweek.breaks),id", "2,0,1")]
    [InlineData("/calendars?sort=-count(workweek.breaks),id", "1,0,2")]
    [InlineData("/calendars?sort=count(pinnedAppointments),id", "2,0,1")]
    [InlineData("/calendars?sort=-count(pinnedAppointments),id", "1,0,2")]
    public async Task Can_sort_calendars_by_count_of_collection(string route, string expectedOrder)
    {
        // Arrange
        List<Calendar> calendars = _fakers.Calendar.GenerateList(3);

        calendars[0].Holidays =
        [
            new Holiday
            {
                Name = "A"
            },
            new Holiday
            {
                Name = "B"
            }
        ];

        calendars[1].Holidays =
        [
            new Holiday
            {
                Name = "C"
            }
        ];

        calendars[2].Holidays = [];

        calendars[0].Workweek = new Workweek
        {
            WorkDays =
            [
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Friday
            ],
            Breaks = [new OfficeHours()]
        };

        calendars[1].Workweek = new Workweek
        {
            WorkDays = [],
            Breaks =
            [
                new OfficeHours(),
                new OfficeHours()
            ]
        };

        calendars[2].Workweek = new Workweek
        {
            WorkDays = [DayOfWeek.Monday],
            Breaks = []
        };

        calendars[0].PinnedAppointments = _fakers.Appointment.GenerateSet(1);
        calendars[1].PinnedAppointments = _fakers.Appointment.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.AddRange(calendars);
            await dbContext.SaveChangesAsync();
        });

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string[] expectedIds = expectedOrder.Split(',').Select(index => calendars[int.Parse(index)].StringId!).ToArray();
        responseDocument.GetPrimaryIds().Should().Equal(expectedIds);
    }

    [Theory]
    [InlineData("/blogs?sort=count(tags),id", "1,0")]
    [InlineData("/blogs?sort=-count(tags),id", "0,1")]
    [InlineData("/blogs?sort=count(posts),id", "1,0")]
    [InlineData("/blogs?sort=-count(posts),id", "0,1")]
    public async Task Can_sort_blogs_by_count_of_collection(string route, string expectedOrder)
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);
        blogs[0].Tags = ["a", "b"];
        blogs[0].Posts = _fakers.BlogPost.GenerateList(2);
        blogs[1].Tags = ["a"];
        blogs[1].Posts = _fakers.BlogPost.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string[] expectedIds = expectedOrder.Split(',').Select(index => blogs[int.Parse(index)].StringId!).ToArray();
        responseDocument.GetPrimaryIds().Should().Equal(expectedIds);
    }

    [Fact]
    public async Task Can_sort_included_collection_of_compound_free_top()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(3);
        blog.Posts[0].Caption = "B";
        blog.Posts[1].Caption = "C";
        blog.Posts[2].Caption = "A";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?include=posts&sort[posts]=-caption";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetLinkageIds("posts").Should().Equal(blog.Posts[1].StringId, blog.Posts[0].StringId, blog.Posts[2].StringId);

        responseDocument.GetIncludedIds().Should().Equal(blog.Posts[1].StringId, blog.Posts[0].StringId, blog.Posts[2].StringId);
    }

    [Fact]
    public async Task Can_sort_included_collection_of_compound_top_with_compound_children()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(3);
        Appointment[] appointments = calendar.PinnedAppointments.ToArray();
        appointments[0].Cost.Amount = 20;
        appointments[1].Cost.Amount = 30;
        appointments[2].Cost.Amount = 10;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?include=pinnedAppointments&sort[pinnedAppointments]=-cost.amount";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetLinkageIds("pinnedAppointments").Should().Equal(appointments[1].StringId, appointments[0].StringId,
            appointments[2].StringId);

        responseDocument.GetIncludedIds().Should().Equal(appointments[1].StringId, appointments[0].StringId, appointments[2].StringId);
    }

    [Fact]
    public async Task Can_sort_included_collection_of_compound_top_with_compound_free_children()
    {
        // Arrange
        Appointment appointment = _fakers.Appointment.GenerateOne();
        appointment.Reminders = _fakers.Reminder.GenerateList(3);
        appointment.Reminders[0].RemindsAt = new DateTime(2030, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        appointment.Reminders[1].RemindsAt = new DateTime(2030, 1, 3, 0, 0, 0, DateTimeKind.Utc);
        appointment.Reminders[2].RemindsAt = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/appointments?include=reminders&sort[reminders]=-remindsAt";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetLinkageIds("reminders").Should().Equal(appointment.Reminders[1].StringId, appointment.Reminders[0].StringId,
            appointment.Reminders[2].StringId);

        responseDocument.GetIncludedIds().Should().Equal(appointment.Reminders[1].StringId, appointment.Reminders[0].StringId,
            appointment.Reminders[2].StringId);
    }

    [Fact]
    public async Task Can_sort_secondary_collection_of_compound_free_top()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(3);
        blog.Posts[0].Caption = "B";
        blog.Posts[1].Caption = "C";
        blog.Posts[2].Caption = "A";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/posts?sort=-caption";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetPrimaryIds().Should().Equal(blog.Posts[1].StringId, blog.Posts[0].StringId, blog.Posts[2].StringId);
    }

    [Fact]
    public async Task Can_sort_secondary_collection_of_compound_parent_with_compound_children()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(3);
        Appointment[] appointments = calendar.PinnedAppointments.ToArray();
        appointments[0].Cost.Amount = 20;
        appointments[1].Cost.Amount = 30;
        appointments[2].Cost.Amount = 10;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}/pinnedAppointments?sort=-cost.amount";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetPrimaryIds().Should().Equal(appointments[1].StringId, appointments[0].StringId,
            appointments[2].StringId);
    }

    [Fact]
    public async Task Can_sort_secondary_collection_of_compound_parent_with_compound_free_children()
    {
        // Arrange
        Appointment appointment = _fakers.Appointment.GenerateOne();
        appointment.Reminders = _fakers.Reminder.GenerateList(3);
        appointment.Reminders[0].RemindsAt = new DateTime(2030, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        appointment.Reminders[1].RemindsAt = new DateTime(2030, 1, 3, 0, 0, 0, DateTimeKind.Utc);
        appointment.Reminders[2].RemindsAt = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/appointments/{appointment.StringId}/reminders?sort=-remindsAt";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetPrimaryIds().Should().Equal(appointment.Reminders[1].StringId, appointment.Reminders[0].StringId,
            appointment.Reminders[2].StringId);
    }

    [Theory]
    [InlineData("/calendars?sort=workweek", "Field chain on resource type")]
    [InlineData("/calendars?sort=workweek.workDays", "Field chain on resource type")]
    [InlineData("/calendars?sort=holidays", "Field chain on resource type")]
    [InlineData("/calendars?sort=workweek.officeHours", "Field chain on resource type")]
    [InlineData("/appointments?sort=attendees", "Field chain on resource type")]
    [InlineData("/appointments?sort=keywords", "Field chain on resource type")]
    [InlineData("/blogs?sort=tags", "Field chain on resource type")]
    [InlineData("/calendars?sort=count(workweek)", "Field chain on resource type")]
    public async Task Cannot_sort_on_compound_or_collection(string route, string expectedDetailPrefix)
    {
        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().StartWith(expectedDetailPrefix);
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("sort");
    }

    [Theory]
    [InlineData("/calendars?sort=workweek.internalNote", "Sorting on attribute 'internalNote' is not allowed.")]
    [InlineData("/calendars?sort=count(workweek.internalNote)", "Field chain on resource type")]
    public async Task Cannot_sort_on_blocked_nested_attribute(string route, string expectedDetailPrefix)
    {
        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().StartWith(expectedDetailPrefix);
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("sort");
    }
}
