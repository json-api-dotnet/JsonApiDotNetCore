using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Includes;

/// <summary>
/// Verifies that constraints on included collections do not depend on whether a type involved has a compound attribute. Blog, BlogPost and Comment have
/// no compound attributes (controls), whereas Calendar and Appointment do.
/// </summary>
public sealed class IncludeCompoundTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private const string HostPrefix = "http://localhost";

    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public IncludeCompoundTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AppointmentsController>();
        testContext.UseController<CalendarsController>();
        testContext.UseController<RemindersController>();
        testContext.UseController<BlogsController>();
        testContext.UseController<BlogPostsController>();
        testContext.UseController<CommentsController>();

        var options = (JsonApiOptions)testContext.App.Services.GetRequiredService<IJsonApiOptions>();
        options.MaximumIncludeDepth = null;
        options.IncludeTotalResourceCount = true;
        options.DefaultPageSize = new PageSize(10);
        options.MaximumPageSize = null;
        options.MaximumPageNumber = null;
    }

    [Fact]
    public async Task Can_include_to_many_with_compound()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(1);

        Appointment appointment = calendar.PinnedAppointments.Single();

        appointment.Cost = new AppointmentCost
        {
            Amount = 12.5m,
            Currency = "EUR"
        };

        appointment.Location = new AppointmentLocation
        {
            Name = "Office",
            Address = new AppointmentAddress
            {
                City = "Rotterdam"
            }
        };

        appointment.Attendees =
        [
            new Attendee
            {
                Name = "Alice"
            }
        ];

        appointment.Keywords =
        [
            "a",
            "b"
        ];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?include=pinnedAppointments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(1);
        ResourceObject included = responseDocument.Included[0];
        included.Type.Should().Be("appointments");
        included.Id.Should().Be(appointment.StringId);

        included.GetAttribute("cost").GetProperty("amount").GetDecimal().Should().Be(12.5m);
        included.GetAttribute("cost").GetProperty("currency").GetString().Should().Be("EUR");
        included.GetAttribute("location").GetProperty("address").GetProperty("city").GetString().Should().Be("Rotterdam");
        included.GetAttribute("attendees").GetArrayLength().Should().Be(1);
        included.GetAttribute("keywords").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task Can_include_to_one_with_compound()
    {
        // Arrange
        Appointment appointment = _fakers.Appointment.GenerateOne();
        appointment.Calendar = _fakers.Calendar.GenerateOne();

        appointment.Calendar.Workweek = new Workweek
        {
            FirstDay = DayOfWeek.Monday,
            WorkDays =
            [
                DayOfWeek.Monday,
                DayOfWeek.Tuesday
            ]
        };

        appointment.Calendar.Holidays =
        [
            new Holiday
            {
                Name = "New Year"
            }
        ];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/appointments?include=calendar";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(1);
        ResourceObject included = responseDocument.Included[0];
        included.Type.Should().Be("calendars");

        included.GetAttribute("workweek").GetProperty("workDays").GetArrayLength().Should().Be(2);
        included.GetAttribute("holidays").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task Can_include_to_one_with_compound_from_compound_free_top()
    {
        // Arrange
        Reminder reminder = _fakers.Reminder.GenerateOne();
        reminder.Appointment = _fakers.Appointment.GenerateOne();
        reminder.Appointment.Cost.Amount = 7;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Reminder>();
            dbContext.Reminders.Add(reminder);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/reminders?include=appointment";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].GetAttribute("cost").GetProperty("amount").GetDecimal().Should().Be(7);
    }

    [Fact]
    public async Task Can_include_mixed_chain()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.MostRecentAppointment = _fakers.Appointment.GenerateOne();
        calendar.MostRecentAppointment.Reminders = _fakers.Reminder.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?include=mostRecentAppointment.reminders";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetAttribute("workweek");

        responseDocument.Included.Should().HaveCount(2);
        responseDocument.Included[0].Type.Should().Be("appointments");
        responseDocument.Included[0].GetAttribute("cost");
        responseDocument.Included[1].Type.Should().Be("reminders");
        responseDocument.Included[1].Id.Should().Be(calendar.MostRecentAppointment.Reminders[0].StringId);
        responseDocument.Included[1].GetAttribute("remindsAt");
    }

    [Fact]
    public async Task Can_include_chain_with_cyclic_dependency_on_compound_top()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.MostRecentAppointment = _fakers.Appointment.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();

            calendar.MostRecentAppointment.Calendar = calendar;
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}?include=mostRecentAppointment.calendar.mostRecentAppointment.calendar.mostRecentAppointment";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(calendar.StringId);

        responseDocument.Included.Should().NotBeEmpty();
        responseDocument.Included.Should().Contain(resource => resource.Type == "appointments" && resource.Id == calendar.MostRecentAppointment.StringId);
    }

    [Fact]
    public async Task Can_filter_included_collection_of_compound_free_top()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);
        blogs[0].Posts = _fakers.BlogPost.GenerateList(3);
        blogs[0].Posts[0].Caption = "x";
        blogs[0].Posts[1].Caption = "y";
        blogs[0].Posts[2].Caption = "x";
        blogs[1].Posts = _fakers.BlogPost.GenerateList(1);
        blogs[1].Posts[0].Caption = "y";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?include=posts&filter[posts]=equals(caption,'x')&sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].GetLinkageIds("posts").Should().BeEquivalentTo(blogs[0].Posts[0].StringId, blogs[0].Posts[2].StringId);
        responseDocument.Data.ManyValue[1].GetLinkageIds("posts").Should().BeEmpty();

        responseDocument.GetIncludedIds().Should().BeEquivalentTo(blogs[0].Posts[0].StringId, blogs[0].Posts[2].StringId);
    }

    [Fact]
    public async Task Can_filter_included_collection_of_compound_top_with_compound_children()
    {
        // Arrange
        List<Calendar> calendars = _fakers.Calendar.GenerateList(2);
        calendars[0].PinnedAppointments = _fakers.Appointment.GenerateSet(3);
        Appointment[] appointments = calendars[0].PinnedAppointments.ToArray();
        appointments[0].Title = "x";
        appointments[1].Title = "y";
        appointments[2].Title = "x";
        calendars[1].PinnedAppointments = _fakers.Appointment.GenerateSet(1);
        calendars[1].PinnedAppointments.Single().Title = "y";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.AddRange(calendars);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?include=pinnedAppointments&filter[pinnedAppointments]=equals(title,'x')&sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].GetLinkageIds("pinnedAppointments").Should().BeEquivalentTo(appointments[0].StringId, appointments[2].StringId);
        responseDocument.Data.ManyValue[1].GetLinkageIds("pinnedAppointments").Should().BeEmpty();

        responseDocument.GetIncludedIds().Should().BeEquivalentTo(appointments[0].StringId, appointments[2].StringId);
    }

    [Fact]
    public async Task Can_filter_included_collection_of_compound_top_with_compound_free_children()
    {
        // Arrange
        List<Appointment> appointments = _fakers.Appointment.GenerateList(2);
        appointments[0].Reminders = _fakers.Reminder.GenerateList(3);
        appointments[0].Reminders[0].RemindsAt = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        appointments[0].Reminders[1].RemindsAt = new DateTime(2030, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        appointments[0].Reminders[2].RemindsAt = new DateTime(2030, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        appointments[1].Reminders = _fakers.Reminder.GenerateList(1);
        appointments[1].Reminders[0].RemindsAt = new DateTime(2030, 1, 5, 0, 0, 0, DateTimeKind.Utc);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.AddRange(appointments);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/appointments?include=reminders&filter[reminders]=greaterThan(remindsAt,'2030-02-01T00:00:00Z')&sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);

        responseDocument.Data.ManyValue[0].GetLinkageIds("reminders").Should().BeEquivalentTo(appointments[0].Reminders[1].StringId,
            appointments[0].Reminders[2].StringId);

        responseDocument.Data.ManyValue[1].GetLinkageIds("reminders").Should().BeEmpty();

        responseDocument.GetIncludedIds().Should().BeEquivalentTo(appointments[0].Reminders[1].StringId, appointments[0].Reminders[2].StringId);
    }

    [Fact]
    public async Task Can_filter_included_collection_on_nested_compound_attribute()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(3);
        Appointment[] appointments = calendar.PinnedAppointments.ToArray();
        appointments[0].Cost.Amount = 5;
        appointments[1].Cost.Amount = 15;
        appointments[2].Cost.Amount = 25;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?include=pinnedAppointments&filter[pinnedAppointments]=greaterThan(cost.amount,'10')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetLinkageIds("pinnedAppointments").Should().BeEquivalentTo(appointments[1].StringId, appointments[2].StringId);
        responseDocument.GetIncludedIds().Should().BeEquivalentTo(appointments[1].StringId, appointments[2].StringId);
    }

    [Fact]
    public async Task Can_filter_included_collection_with_has_on_scalar_collection()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(3);
        Appointment[] appointments = calendar.PinnedAppointments.ToArray();

        appointments[0].Keywords =
        [
            "x",
            "y"
        ];

        appointments[1].Keywords = ["y"];
        appointments[2].Keywords = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?include=pinnedAppointments&filter[pinnedAppointments]=has(keywords,equals(_,'x'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetLinkageIds("pinnedAppointments").Should().BeEquivalentTo(appointments[0].StringId);
        responseDocument.GetIncludedIds().Should().BeEquivalentTo(appointments[0].StringId);
    }

    [Fact]
    public async Task Can_filter_included_collection_with_has_on_compound_collection()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(3);
        Appointment[] appointments = calendar.PinnedAppointments.ToArray();

        appointments[0].Attendees =
        [
            new Attendee
            {
                Name = "A",
                Response = AttendeeResponse.Accepted
            }
        ];

        appointments[1].Attendees =
        [
            new Attendee
            {
                Name = "B",
                Response = AttendeeResponse.Declined
            }
        ];

        appointments[2].Attendees = [];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?include=pinnedAppointments&filter[pinnedAppointments]=has(attendees,equals(response,'Accepted'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetLinkageIds("pinnedAppointments").Should().BeEquivalentTo(appointments[0].StringId);
        responseDocument.GetIncludedIds().Should().BeEquivalentTo(appointments[0].StringId);
    }

    [Fact]
    public async Task Can_filter_second_level_included_collection_of_compound_free_top()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(1);
        blog.Posts[0].Comments = _fakers.Comment.GenerateSet(3);
        Comment[] comments = blog.Posts[0].Comments.ToArray();
        comments[0].Text = "x";
        comments[1].Text = "y";
        comments[2].Text = "x";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?include=posts.comments&filter[posts.comments]=equals(text,'x')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetIncludedIds("comments").Should().BeEquivalentTo(comments[0].StringId, comments[2].StringId);
    }

    [Fact]
    public async Task Can_filter_second_level_included_collection_of_compound_top()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(1);
        Appointment appointment = calendar.PinnedAppointments.Single();
        appointment.Reminders = _fakers.Reminder.GenerateList(3);
        appointment.Reminders[0].RemindsAt = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        appointment.Reminders[1].RemindsAt = new DateTime(2030, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        appointment.Reminders[2].RemindsAt = new DateTime(2030, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        const string route =
            "/calendars?include=pinnedAppointments.reminders&filter[pinnedAppointments.reminders]=greaterThan(remindsAt,'2030-02-01T00:00:00Z')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetIncludedIds("reminders").Should().BeEquivalentTo(appointment.Reminders[1].StringId, appointment.Reminders[2].StringId);
    }

    [Fact]
    public async Task Can_include_with_filter_and_sort_and_page_together()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(4);
        Appointment[] appointments = calendar.PinnedAppointments.ToArray();
        appointments[0].Cost.Amount = 3;
        appointments[1].Cost.Amount = 10;
        appointments[2].Cost.Amount = 30;
        appointments[3].Cost.Amount = 20;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        const string route =
            "/calendars?include=pinnedAppointments&filter[pinnedAppointments]=greaterThan(cost.amount,'5')&sort[pinnedAppointments]=-cost.amount&page[size]=pinnedAppointments:2";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetLinkageIds("pinnedAppointments").Should().Equal(appointments[2].StringId, appointments[3].StringId);
        responseDocument.GetIncludedIds().Should().Equal(appointments[2].StringId, appointments[3].StringId);
    }

    [Fact]
    public async Task Applies_secondary_endpoint_constraints_of_compound_free_top()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(4);
        blog.Posts[0].Caption = "a";
        blog.Posts[1].Caption = "b1";
        blog.Posts[2].Caption = "b3";
        blog.Posts[3].Caption = "b2";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/posts?filter=startsWith(caption,'b')&sort=-caption&page[size]=2";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetPrimaryIds().Should().Equal(blog.Posts[2].StringId, blog.Posts[3].StringId);

        responseDocument.Links.Should().NotBeNull();

        responseDocument.Links.Next.Should()
            .Be($"{HostPrefix}/blogs/{blog.StringId}/posts?filter=startsWith(caption,%27b%27)&sort=-caption&page%5Bsize%5D=2&page%5Bnumber%5D=2");

        responseDocument.Links.Last.Should().Be(responseDocument.Links.Next);
    }

    [Fact]
    public async Task Applies_secondary_endpoint_constraints_of_compound_top_with_compound_children()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(4);
        Appointment[] appointments = calendar.PinnedAppointments.ToArray();
        appointments[0].Cost.Amount = 3;
        appointments[1].Cost.Amount = 10;
        appointments[2].Cost.Amount = 30;
        appointments[3].Cost.Amount = 20;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}/pinnedAppointments?filter=greaterThan(cost.amount,'5')&sort=-cost.amount&page[size]=2";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetPrimaryIds().Should().Equal(appointments[2].StringId, appointments[3].StringId);

        // Three appointments match the filter, so there are two pages.
        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Next.Should().NotBeNull();
        responseDocument.Links.Last.Should().Be(responseDocument.Links.Next);
    }

    [Fact]
    public async Task Applies_secondary_endpoint_constraints_of_compound_top_with_compound_free_children()
    {
        // Arrange
        Appointment appointment = _fakers.Appointment.GenerateOne();
        appointment.Reminders = _fakers.Reminder.GenerateList(3);
        appointment.Reminders[0].RemindsAt = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        appointment.Reminders[1].RemindsAt = new DateTime(2030, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        appointment.Reminders[2].RemindsAt = new DateTime(2030, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/appointments/{appointment.StringId}/reminders?filter=greaterThan(remindsAt,'2030-02-01T00:00:00Z')&sort=-remindsAt";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetPrimaryIds().Should().Equal(appointment.Reminders[2].StringId, appointment.Reminders[1].StringId);
    }

    [Fact]
    public async Task Applies_relationship_endpoint_constraints_of_compound_free_top()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(3);
        blog.Posts[0].Caption = "a";
        blog.Posts[1].Caption = "b1";
        blog.Posts[2].Caption = "b2";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/relationships/posts?filter=startsWith(caption,'b')&page[size]=1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetPrimaryIds().Should().Equal(blog.Posts[1].StringId);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Next.Should().NotBeNull();
        responseDocument.Links.Next.Should().Be(responseDocument.Links.Last);
    }

    [Fact]
    public async Task Applies_relationship_endpoint_constraints_of_compound_top()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(3);
        Appointment[] appointments = calendar.PinnedAppointments.ToArray();
        appointments[0].Title = "a";
        appointments[1].Title = "b1";
        appointments[2].Title = "b2";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}/relationships/pinnedAppointments?filter=startsWith(title,'b')&page[size]=1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetPrimaryIds().Should().Equal(appointments[1].StringId);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Next.Should().NotBeNull();
        responseDocument.Links.Next.Should().Be(responseDocument.Links.Last);
    }

    [Fact]
    public async Task Cannot_include_compound_attribute()
    {
        // Arrange
        var parameterValue = new MarkedText("^location", '^');
        string route = $"/appointments?include={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified include is invalid.");
        error.Detail.Should().Be($"Relationship 'location' does not exist on resource type 'appointments'. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("include");
    }

    [Fact]
    public async Task Cannot_include_relationship_when_inclusion_blocked_on_type_with_compound()
    {
        // Arrange
        var parameterValue = new MarkedText("^firstAppointment", '^');
        string route = $"/calendars?include={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified include is invalid.");
        error.Detail.Should().Be($"Including relationship 'firstAppointment' on 'calendars' is not allowed. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("include");
    }

    [Fact]
    public async Task Can_include_chain_with_cyclic_dependency_through_to_many_on_compound_top()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();

            foreach (Appointment appointment in calendar.PinnedAppointments)
            {
                appointment.Calendar = calendar;
            }

            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?include=pinnedAppointments.calendar.pinnedAppointments.calendar";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(calendar.StringId);

        responseDocument.Included.Should().HaveCount(2);
        responseDocument.Included.Should().OnlyContain(resource => resource.Type == "appointments");
        responseDocument.GetIncludedIds().Should().BeEquivalentTo(calendar.PinnedAppointments.Select(appointment => appointment.StringId));
    }

    [Fact]
    public async Task Can_sort_relationship_endpoint_of_compound_top()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(3);
        Appointment[] appointments = calendar.PinnedAppointments.ToArray();
        appointments[0].Title = "b";
        appointments[1].Title = "c";
        appointments[2].Title = "a";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}/relationships/pinnedAppointments?sort=-title";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetPrimaryIds().Should().Equal(appointments[1].StringId, appointments[0].StringId, appointments[2].StringId);
    }

    [Fact]
    public async Task Can_include_resources_with_struct_compounds()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        List<Appointment> appointments = _fakers.Appointment.GenerateList(2);

        appointments[0].MeetingPoint = null;
        appointments[0].Timing = new AppointmentTiming
        {
            BufferMinutes = 5,
            Note = "first"
        };

        appointments[1].MeetingPoint = new AppointmentCoordinates
        {
            Latitude = 52.5,
            Longitude = 4.25,
            NearestAddress = new AppointmentAddress
            {
                City = "Rotterdam"
            }
        };

        appointments[1].Timing = new AppointmentTiming
        {
            BufferMinutes = 7,
            Note = null
        };

        calendar.PinnedAppointments = appointments.ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?include=pinnedAppointments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(2);

        ResourceObject first = responseDocument.Included.Single(resource => resource.Id == appointments[0].StringId);
        first.GetAttribute("meetingPoint").ValueKind.Should().Be(JsonValueKind.Null);
        first.GetAttribute("timing").GetProperty("bufferMinutes").GetInt32().Should().Be(5);
        first.GetAttribute("timing").GetProperty("note").GetString().Should().Be("first");

        ResourceObject second = responseDocument.Included.Single(resource => resource.Id == appointments[1].StringId);
        second.GetAttribute("meetingPoint").GetProperty("latitude").GetDouble().Should().Be(52.5);
        second.GetAttribute("meetingPoint").GetProperty("longitude").GetDouble().Should().Be(4.25);
        second.GetAttribute("meetingPoint").GetProperty("nearestAddress").GetProperty("city").GetString().Should().Be("Rotterdam");
        second.GetAttribute("timing").GetProperty("bufferMinutes").GetInt32().Should().Be(7);
    }

    [Fact]
    public async Task Can_include_resources_with_struct_compounds_and_sparse_fieldset()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        List<Appointment> appointments = _fakers.Appointment.GenerateList(2);

        appointments[0].MeetingPoint = null;
        appointments[1].MeetingPoint = new AppointmentCoordinates
        {
            Latitude = 52.5,
            Longitude = 4.25
        };

        calendar.PinnedAppointments = appointments.ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?include=pinnedAppointments&fields[appointments]=meetingPoint.longitude,timing.bufferMinutes";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(2);

        ResourceObject first = responseDocument.Included.Single(resource => resource.Id == appointments[0].StringId);
        first.Attributes.Should().HaveCount(2);
        first.GetAttribute("meetingPoint").ValueKind.Should().Be(JsonValueKind.Null);
        first.GetAttribute("timing").GetMemberNames().Should().Equal("bufferMinutes");

        ResourceObject second = responseDocument.Included.Single(resource => resource.Id == appointments[1].StringId);
        second.GetAttribute("meetingPoint").GetMemberNames().Should().Equal("longitude");
        second.GetAttribute("meetingPoint").GetProperty("longitude").GetDouble().Should().Be(4.25);
    }

    [Fact]
    public async Task Can_include_to_one_from_resources_with_struct_compounds()
    {
        // Arrange
        Appointment appointment = _fakers.Appointment.GenerateOne();
        appointment.Calendar = _fakers.Calendar.GenerateOne();

        appointment.MeetingPoint = new AppointmentCoordinates
        {
            Latitude = 10,
            Longitude = 20
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/appointments?include=calendar&fields[calendars]=timeZone";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetAttribute("meetingPoint").GetProperty("latitude").GetDouble().Should().Be(10);
        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Attributes.Should().HaveCount(1);
    }
}
