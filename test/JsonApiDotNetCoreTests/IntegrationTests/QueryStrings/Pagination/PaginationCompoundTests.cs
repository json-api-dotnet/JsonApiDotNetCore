using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Pagination;

/// <summary>
/// Verifies pagination for types that have compound attributes (Calendar and Appointment), using Blog, BlogPost and Comment (without compound
/// attributes) as controls.
/// </summary>
public sealed class PaginationCompoundTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private const string HostPrefix = "http://localhost";
    private const int DefaultPageSize = 2;
    private const int MaximumPageSize = 3;

    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public PaginationCompoundTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AppointmentsController>();
        testContext.UseController<CalendarsController>();
        testContext.UseController<BlogsController>();
        testContext.UseController<BlogPostsController>();

        var options = (JsonApiOptions)testContext.App.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeTotalResourceCount = true;
        options.DefaultPageSize = new PageSize(DefaultPageSize);
        options.MaximumPageSize = new PageSize(MaximumPageSize);
        options.MaximumPageNumber = null;
        options.AllowUnknownQueryStringParameters = true;
    }

    [Fact]
    public async Task Can_page_primary_resources_with_compound()
    {
        // Arrange
        List<Calendar> calendars = _fakers.Calendar.GenerateList(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.AddRange(calendars);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?page[size]=1&page[number]=2";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetPrimaryIds().Should().Equal(calendars[1].StringId);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{HostPrefix}/calendars?page%5Bsize%5D=1");
        responseDocument.Links.Last.Should().Be($"{HostPrefix}/calendars?page%5Bsize%5D=1&page%5Bnumber%5D=3");
        responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
        responseDocument.Links.Next.Should().Be(responseDocument.Links.Last);
    }

    [Fact]
    public async Task Can_page_included_collection_of_compound_free_top()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);
        blogs[0].Posts = _fakers.BlogPost.GenerateList(3);
        blogs[1].Posts = _fakers.BlogPost.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?include=posts&page[size]=posts:1&sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].GetLinkageIds("posts").Should().Equal(blogs[0].Posts[0].StringId);
        responseDocument.Data.ManyValue[1].GetLinkageIds("posts").Should().Equal(blogs[1].Posts[0].StringId);

        responseDocument.GetIncludedIds().Should().BeEquivalentTo(blogs[0].Posts[0].StringId, blogs[1].Posts[0].StringId);
    }

    [Fact]
    public async Task Can_page_included_collection_of_compound_top()
    {
        // Arrange
        List<Calendar> calendars = _fakers.Calendar.GenerateList(2);
        calendars[0].PinnedAppointments = _fakers.Appointment.GenerateSet(3);
        calendars[1].PinnedAppointments = _fakers.Appointment.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Calendar>();
            dbContext.Calendars.AddRange(calendars);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/calendars?include=pinnedAppointments&page[size]=pinnedAppointments:1&sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].GetLinkageIds("pinnedAppointments").Should().HaveCount(1);
        responseDocument.Data.ManyValue[1].GetLinkageIds("pinnedAppointments").Should().HaveCount(1);

        responseDocument.GetIncludedIds().Should().HaveCount(2);
    }

    [Fact]
    public async Task Applies_default_page_size_to_included_collection_of_compound_free_top()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?include=posts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetLinkageIds("posts").Should().HaveCount(DefaultPageSize);
        responseDocument.GetIncludedIds().Should().HaveCount(DefaultPageSize);
    }

    [Fact]
    public async Task Applies_default_page_size_to_included_collection_of_compound_top()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(3);

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

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].GetLinkageIds("pinnedAppointments").Should().HaveCount(DefaultPageSize);
        responseDocument.GetIncludedIds().Should().HaveCount(DefaultPageSize);
    }

    [Fact]
    public async Task Can_page_secondary_collection_of_compound_free_top_with_consistent_links()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/posts?page[size]=1&page[number]=2";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetPrimaryIds().Should().Equal(blog.Posts[1].StringId);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.First.Should().Be($"{HostPrefix}/blogs/{blog.StringId}/posts?page%5Bsize%5D=1");
        responseDocument.Links.Last.Should().Be($"{HostPrefix}/blogs/{blog.StringId}/posts?page%5Bsize%5D=1&page%5Bnumber%5D=3");
        responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
        responseDocument.Links.Next.Should().Be(responseDocument.Links.Last);
    }

    [Fact]
    public async Task Can_page_secondary_collection_of_compound_top_with_consistent_links()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(3);
        Appointment[] appointments = calendar.PinnedAppointments.ToArray();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}/pinnedAppointments?page[size]=1&page[number]=2";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetPrimaryIds().Should().Equal(appointments[1].StringId);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.First.Should().Be($"{HostPrefix}/calendars/{calendar.StringId}/pinnedAppointments?page%5Bsize%5D=1");
        responseDocument.Links.Last.Should().Be($"{HostPrefix}/calendars/{calendar.StringId}/pinnedAppointments?page%5Bsize%5D=1&page%5Bnumber%5D=3");
        responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
        responseDocument.Links.Next.Should().Be(responseDocument.Links.Last);
    }

    [Fact]
    public async Task Can_page_relationship_endpoint_of_compound_top_with_consistent_links()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();
        calendar.PinnedAppointments = _fakers.Appointment.GenerateSet(3);
        Appointment[] appointments = calendar.PinnedAppointments.ToArray();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Calendars.Add(calendar);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/calendars/{calendar.StringId}/relationships/pinnedAppointments?page[size]=1&page[number]=3";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.GetPrimaryIds().Should().Equal(appointments[2].StringId);

        responseDocument.Links.Should().NotBeNull();

        responseDocument.Links.Last.Should()
            .Be($"{HostPrefix}/calendars/{calendar.StringId}/relationships/pinnedAppointments?page%5Bsize%5D=1&page%5Bnumber%5D=3");

        responseDocument.Links.Next.Should().BeNull();
    }

    [Fact]
    public async Task Can_page_in_scope_of_relationship_with_blocked_inclusion()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?page[size]=parent.posts:1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(DefaultPageSize);
    }

    [Fact]
    public async Task Cannot_page_through_compound_scope()
    {
        // Arrange
        var parameterValue = new MarkedText("^workweek:2", '^');
        string route = $"/calendars?page[size]={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified pagination is invalid.");
        error.Detail.Should().Be($"To-many relationship 'workweek' does not exist on resource type 'calendars'. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("page[size]");
    }

    [Fact]
    public async Task Enforces_maximum_page_size_on_included_collection_of_compound_top()
    {
        // Arrange
        const int pageSize = MaximumPageSize + 1;
        var parameterValue = new MarkedText($"pinnedAppointments:^{pageSize}", '^');
        string route = $"/calendars?include=pinnedAppointments&page[size]={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified pagination is invalid.");
        error.Detail.Should().Be($"Page size cannot be greater than {MaximumPageSize}. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("page[size]");
    }
}
