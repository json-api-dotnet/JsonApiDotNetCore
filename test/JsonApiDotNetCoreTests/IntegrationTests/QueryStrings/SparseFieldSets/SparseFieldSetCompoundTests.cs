using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.SparseFieldSets;

public sealed class SparseFieldSetCompoundTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public SparseFieldSetCompoundTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AppointmentsController>();
        testContext.UseController<CalendarsController>();
        testContext.UseController<BlogsController>();
        testContext.UseController<RemindersController>();

        testContext.ConfigureServices(services =>
        {
            services.AddResourceRepository<ResultCapturingRepository<Blog, long>>();
            services.AddResourceRepository<ResultCapturingRepository<Calendar, long>>();
            services.AddResourceRepository<ResultCapturingRepository<Appointment, long>>();

            services.AddSingleton<ResourceCaptureStore>();
        });
    }

    private static Appointment CreateAppointmentWithAllCompounds(Appointment appointment)
    {
        appointment.Cost = new AppointmentCost
        {
            Amount = 12.5m,
            Currency = "EUR"
        };

        appointment.Location = new AppointmentLocation
        {
            Name = "Office",
            Room = "R1",
            Address = new AppointmentAddress
            {
                Street = "Main Street",
                City = "Rotterdam",
                CountryCode = "NL"
            }
        };

        appointment.Attendees =
        [
            new Attendee
            {
                Name = "Alice",
                Email = "alice@company.com",
                Response = AttendeeResponse.Accepted
            },
            new Attendee
            {
                Name = "Bob",
                Email = "bob@company.com",
                Response = AttendeeResponse.Declined
            }
        ];

        appointment.Keywords = ["a", "b"];

        return appointment;
    }

    [Fact]
    public async Task Can_select_nested_leaf()
    {
        // Arrange
        Appointment appointment = CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/appointments?fields[appointments]=cost.amount";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        ResourceObject resource = responseDocument.Data.ManyValue[0];
        resource.Id.Should().Be(appointment.StringId);

        resource.Attributes.Should().HaveCount(1);
        resource.GetAttribute("cost").GetMemberNames().Should().Equal("amount");
        resource.GetAttribute("cost").GetProperty("amount").GetDecimal().Should().Be(12.5m);
    }

    [Fact]
    public async Task Can_select_whole_compound()
    {
        // Arrange
        Appointment appointment = CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/appointments?fields[appointments]=location";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        ResourceObject resource = responseDocument.Data.ManyValue[0];

        resource.Attributes.Should().HaveCount(1);
        resource.GetAttribute("location").GetMemberNames().Should().BeEquivalentTo("name", "address", "room");
        resource.GetAttribute("location").GetProperty("address").GetMemberNames().Should().BeEquivalentTo("street", "city", "countryCode");
    }

    [Fact]
    public async Task Can_omit_compound()
    {
        // Arrange
        Appointment appointment = CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/appointments?fields[appointments]=title";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        ResourceObject resource = responseDocument.Data.ManyValue[0];

        resource.Attributes.Should().HaveCount(1);
        resource.Attributes.Should().ContainKey("title").WhoseValue.Should().Be(appointment.Title);
    }

    [Fact]
    public async Task Can_select_multiple_branches()
    {
        // Arrange
        Appointment appointment = CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/appointments?fields[appointments]=location.address.city,location.room,cost.currency";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        ResourceObject resource = responseDocument.Data.ManyValue[0];

        resource.Attributes.Should().HaveCount(2);
        resource.GetAttribute("location").GetMemberNames().Should().BeEquivalentTo("address", "room");
        resource.GetAttribute("location").GetProperty("address").GetMemberNames().Should().Equal("city");
        resource.GetAttribute("cost").GetMemberNames().Should().Equal("currency");
    }

    [Fact]
    public async Task Can_select_nested_leaf_with_top_level_attribute()
    {
        // Arrange
        Appointment appointment = CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/appointments/{appointment.StringId}?fields[appointments]=cost.amount,title";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(2);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("title").WhoseValue.Should().Be(appointment.Title);
        responseDocument.Data.SingleValue.GetAttribute("cost").GetMemberNames().Should().Equal("amount");
    }

    [Fact]
    public async Task Can_select_multiple_members_of_same_nested_compound()
    {
        // Arrange
        Appointment appointment = CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/appointments/{appointment.StringId}?fields[appointments]=location.address.street,location.address.city";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);

        JsonElement location = responseDocument.Data.SingleValue.GetAttribute("location");
        location.GetMemberNames().Should().Equal("address");
        location.GetProperty("address").GetMemberNames().Should().BeEquivalentTo("street", "city");
        location.GetProperty("address").GetProperty("street").GetString().Should().Be("Main Street");
        location.GetProperty("address").GetProperty("city").GetString().Should().Be("Rotterdam");
    }

    [Fact]
    public async Task Can_select_nested_leaf_and_relationship_with_include()
    {
        // Arrange
        Appointment appointment = CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne());
        appointment.Calendar = _fakers.Calendar.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/appointments?include=calendar&fields[appointments]=cost.amount,calendar&fields[calendars]=timeZone";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetAttribute("cost").GetMemberNames().Should().Equal("amount");

        responseDocument.Data.ManyValue[0].Relationships.Should().ContainKey("calendar").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.SingleValue.Should().NotBeNull();
            value.Data.SingleValue.Id.Should().Be(appointment.Calendar.StringId);
        });

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Id.Should().Be(appointment.Calendar.StringId);
        responseDocument.Included[0].Attributes.Should().HaveCount(1);
        responseDocument.Included[0].Attributes.Should().ContainKey("timeZone");
    }

    [Fact]
    public async Task Can_select_member_of_compound_collection()
    {
        // Arrange
        Appointment appointment = CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/appointments?fields[appointments]=attendees.name";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        ResourceObject resource = responseDocument.Data.ManyValue[0];

        resource.Attributes.Should().HaveCount(1);
        JsonElement attendees = resource.GetAttribute("attendees");
        attendees.GetArrayLength().Should().Be(2);
        attendees.EnumerateArray().Should().OnlyContain(element => element.GetMemberNames().SequenceEqual(new[]
        {
            "name"
        }));
    }

    [Fact]
    public async Task Can_select_whole_compound_collection()
    {
        // Arrange
        Appointment appointment = CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/appointments?fields[appointments]=attendees";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        ResourceObject resource = responseDocument.Data.ManyValue[0];

        resource.Attributes.Should().HaveCount(1);
        JsonElement attendees = resource.GetAttribute("attendees");
        attendees.GetArrayLength().Should().Be(2);

        attendees.EnumerateArray().Should().OnlyContain(element => element.GetMemberNames().SequenceEqual(new[]
        {
            "name",
            "email",
            "response",
            "contact"
        }));

        attendees[0].GetProperty("name").GetString().Should().Be("Alice");
        attendees[0].GetProperty("contact").GetMemberNames().Should().Equal("phone", "organization", "title");
        attendees[1].GetProperty("name").GetString().Should().Be("Bob");
        attendees[1].GetProperty("contact").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task Can_select_leaf_of_nested_compound_in_compound_collection()
    {
        // Arrange
        var store = _testContext.App.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        Appointment appointment = CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/appointments/{appointment.StringId}?fields[appointments]=attendees.contact.title";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);

        JsonElement attendees = responseDocument.Data.SingleValue.GetAttribute("attendees");
        attendees.GetArrayLength().Should().Be(2);

        // The element that has the nested compound only exposes the selected leaf.
        attendees[0].GetMemberNames().Should().Equal("contact");
        attendees[0].GetProperty("contact").GetMemberNames().Should().Equal("title");
        attendees[0].GetProperty("contact").GetProperty("title").GetString().Should().Be("Manager");

        // The element without the nested compound still exposes the selected member as null.
        attendees[1].GetMemberNames().Should().Equal("contact");
        attendees[1].GetProperty("contact").ValueKind.Should().Be(JsonValueKind.Null);

        var appointmentCaptured = (Appointment)store.Resources.Should().ContainSingle(resource => resource is Appointment).Which;
        appointmentCaptured.Attendees.Should().HaveCount(2);
        appointmentCaptured.Attendees[0].Contact.Should().NotBeNull();
        appointmentCaptured.Attendees[0].Contact!.Title.Should().Be("Manager");
        appointmentCaptured.Attendees[1].Contact.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_whole_nested_compound_in_compound_collection()
    {
        // Arrange
        Appointment appointment = CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/appointments/{appointment.StringId}?fields[appointments]=attendees.contact";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        JsonElement attendees = responseDocument.Data.SingleValue.GetAttribute("attendees");
        attendees.GetArrayLength().Should().Be(2);

        attendees[0].GetMemberNames().Should().Equal("contact");
        attendees[0].GetProperty("contact").GetMemberNames().Should().Equal("phone", "organization", "title");
        attendees[0].GetProperty("contact").GetProperty("phone").GetString().Should().Be("555-1234");
        attendees[0].GetProperty("contact").GetProperty("organization").GetString().Should().Be("Contoso");
        attendees[0].GetProperty("contact").GetProperty("title").GetString().Should().Be("Manager");

        attendees[1].GetMemberNames().Should().Equal("contact");
        attendees[1].GetProperty("contact").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task Can_select_leaf_of_compound_collection_nested_in_compound()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();

        calendar.Workweek = new Workweek
        {
            FirstDay = DayOfWeek.Monday,
            WorkDays = [DayOfWeek.Monday],
            Breaks =
            [
                new OfficeHours
                {
                    OpensAt = new TimeOnly(10, 0),
                    ClosesAt = new TimeOnly(10, 15)
                },
                new OfficeHours
                {
                    OpensAt = new TimeOnly(12, 0),
                    ClosesAt = new TimeOnly(13, 0)
                }
            ]
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}?fields[calendars]=workweek.breaks.opensAt";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        JsonElement workweek = responseDocument.Data.SingleValue.GetAttribute("workweek");
        workweek.GetMemberNames().Should().Equal("breaks");

        JsonElement breaks = workweek.GetProperty("breaks");
        breaks.GetArrayLength().Should().Be(2);
        breaks.EnumerateArray().Should().OnlyContain(element => element.GetMemberNames().SequenceEqual(new[]
        {
            "opensAt"
        }));

        breaks[0].GetProperty("opensAt").GetString().Should().Be("10:00:00");
        breaks[1].GetProperty("opensAt").GetString().Should().Be("12:00:00");
    }

    [Fact]
    public async Task Can_select_scalar_collection_with_database_projection()
    {
        // Arrange
        var store = _testContext.App.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        Blog blog = _fakers.Blog.GenerateOne();
        blog.Tags = ["a", "b"];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?fields[blogs]=tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetAttribute("tags").GetArrayLength().Should().Be(2);

        var blogCaptured = (Blog)store.Resources.Should().ContainSingle(resource => resource is Blog).Which;
        blogCaptured.Id.Should().Be(blog.Id);
        blogCaptured.Tags.Should().Equal(blog.Tags);
        blogCaptured.Title.Should().BeNull();
        blogCaptured.PlatformName.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_empty_fieldset_with_id_only_projection()
    {
        // Arrange
        var store = _testContext.App.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        Blog blog = _fakers.Blog.GenerateOne();
        blog.Tags = ["a", "b"];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?fields[blogs]=";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(blog.StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().BeNull();
        responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();

        var blogCaptured = (Blog)store.Resources.Should().ContainSingle(resource => resource is Blog).Which;
        blogCaptured.Id.Should().Be(blog.Id);
        blogCaptured.Title.Should().BeNull();
        blogCaptured.PlatformName.Should().BeNull();
        blogCaptured.Tags.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task Can_select_by_id()
    {
        // Arrange
        Appointment appointment = CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/appointments/{appointment.StringId}?fields[appointments]=location.name";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);
        responseDocument.Data.SingleValue.GetAttribute("location").GetMemberNames().Should().Equal("name");
        responseDocument.Data.SingleValue.GetAttribute("location").GetProperty("name").GetString().Should().Be("Office");
    }

    [Fact]
    public async Task Can_select_at_secondary_endpoint()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = new[] { CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne()) }.ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}/pinnedAppointments?fields[appointments]=cost.amount";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetAttribute("cost").GetMemberNames().Should().Equal("amount");
    }

    [Fact]
    public async Task Can_select_different_fieldsets_per_type_with_includes()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.Workweek = new Workweek
        {
            FirstDay = DayOfWeek.Monday,
            WorkDays = [DayOfWeek.Monday, DayOfWeek.Tuesday]
        };

        calendar.PinnedAppointments = new[] { CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne()) }.ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?include=pinnedAppointments&fields[calendars]=workweek.firstDay&fields[appointments]=location.address.city";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetAttribute("workweek").GetMemberNames().Should().Equal("firstDay");

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Attributes.Should().HaveCount(1);
        responseDocument.Included[0].GetAttribute("location").GetMemberNames().Should().Equal("address");
        responseDocument.Included[0].GetAttribute("location").GetProperty("address").GetMemberNames().Should().Equal("city");
    }

    [Fact]
    public async Task Can_select_relationship_only_on_compound_free_type()
    {
        // Arrange
        Reminder reminder = _fakers.Reminder.GenerateOne();
        reminder.Appointment = _fakers.Appointment.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Reminder>();
            dbContext.Reminders.Add(reminder);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/reminders?fields[reminders]=appointment";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().BeNull();
        responseDocument.Data.ManyValue[0].Relationships.Should().ContainKey("appointment").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Links.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task Can_select_relationship_only_on_type_with_compound()
    {
        // Arrange
        Appointment appointment = CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne());
        appointment.Calendar = _fakers.Calendar.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/appointments?fields[appointments]=calendar";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().BeNull();
        responseDocument.Data.ManyValue[0].Relationships.Should().ContainKey("calendar").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Links.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task Can_get_relationship_endpoint_of_type_with_compound()
    {
        // Arrange
        Appointment appointment = CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne());
        appointment.Calendar = _fakers.Calendar.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/appointments/{appointment.StringId}/relationships/calendar";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("calendars");
        responseDocument.Data.SingleValue.Id.Should().Be(appointment.Calendar.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().BeNull();
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_to_many_relationship_endpoint_of_type_with_compound()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();

        calendar.PinnedAppointments = new[]
        {
            CreateAppointmentWithAllCompounds(_fakers.Appointment.GenerateOne())
        }.ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}/relationships/pinnedAppointments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("appointments");
        responseDocument.Data.ManyValue[0].Id.Should().Be(calendar.PinnedAppointments.ElementAt(0).StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().BeNull();
        responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_in_create_response()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "appointments",
                attributes = new
                {
                    title = "Meeting",
                    startTime = "2030-01-01T10:00:00Z",
                    endTime = "2030-01-01T11:00:00Z",
                    cost = new
                    {
                        amount = 12.5m,
                        currency = "EUR"
                    }
                }
            }
        };

        // Act
        (HttpResponseMessage postResponse, Document postDocument) =
            await _testContext.ExecutePostAsync<Document>("/appointments?fields[appointments]=cost.amount", requestBody);

        // Assert
        postResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        postDocument.Data.SingleValue.Should().NotBeNull();
        postDocument.Data.SingleValue.Attributes.Should().HaveCount(1);
        postDocument.Data.SingleValue.GetAttribute("cost").GetMemberNames().Should().Equal("amount");
    }

    [Theory]
    [InlineData("location.unknown", "Field 'unknown' does not")]
    [InlineData("location.", "Field name expected.")]
    [InlineData("title.unknown", "Field chain on resource")]
    public async Task Cannot_select_unknown_nested_attribute(string fieldChain, string expectedDetailPrefix)
    {
        // Arrange
        string route = $"/appointments?fields[appointments]={fieldChain}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified fieldset is invalid.");
        error.Detail.Should().StartWith(expectedDetailPrefix);
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("fields[appointments]");
    }

    [Theory]
    [InlineData("workweek.secret", "Retrieving attribute")]
    public async Task Cannot_select_blocked_nested_attribute(string fieldChain, string expectedDetailPrefix)
    {
        // Arrange
        string route = $"/calendars?fields[calendars]={fieldChain}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified fieldset is invalid.");
        error.Detail.Should().StartWith(expectedDetailPrefix);
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("fields[calendars]");
    }

    [Fact]
    public async Task Selects_compound_without_loading_unrelated_scalar_columns()
    {
        // Arrange
        var store = _testContext.App.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.Workweek = new Workweek
        {
            FirstDay = DayOfWeek.Tuesday,
            WorkDays = [DayOfWeek.Tuesday]
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?fields[calendars]=workweek.firstDay";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetAttribute("workweek").GetMemberNames().Should().Equal("firstDay");

        var calendarCaptured = (Calendar)store.Resources.Should().ContainSingle(resource => resource is Calendar).Which;
        calendarCaptured.Id.Should().Be(calendar.Id);
        calendarCaptured.Workweek.Should().NotBeNull();
        calendarCaptured.Workweek.FirstDay.Should().Be(DayOfWeek.Tuesday);
        calendarCaptured.TimeZone.Should().BeNull();
    }

    private static Appointment CreateAppointmentWithAllCompounds(Appointment appointment)
    {
        appointment.Cost = new AppointmentCost
        {
            Amount = 12.5m,
            Currency = "EUR"
        };

        appointment.Location = new AppointmentLocation
        {
            Name = "Office",
            Room = "R1",
            Address = new AppointmentAddress
            {
                Street = "Main Street",
                City = "Rotterdam",
                CountryCode = "NL"
            }
        };

        appointment.Attendees =
        [
            new Attendee
            {
                Name = "Alice",
                Email = "alice@company.com",
                Response = AttendeeResponse.Accepted,
                Contact = new AttendeeContact
                {
                    Phone = "555-1234",
                    Organization = "Contoso",
                    Title = "Manager"
                }
            },
            new Attendee
            {
                Name = "Bob",
                Email = "bob@company.com",
                Response = AttendeeResponse.Declined
            }
        ];

        appointment.Keywords =
        [
            "a",
            "b"
        ];

        return appointment;
    }
}
