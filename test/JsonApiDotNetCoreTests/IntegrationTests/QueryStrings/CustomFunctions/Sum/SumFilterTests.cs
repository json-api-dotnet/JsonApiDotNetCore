using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.Sum;

public sealed class SumFilterTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public SumFilterTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogsController>();
        testContext.UseController<AppointmentsController>();
        testContext.UseController<BlogPostsController>();
        testContext.UseController<CalendarsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddTransient<IFilterParser, SumFilterParser>();
            services.AddTransient<IWhereClauseBuilder, SumWhereClauseBuilder>();
        });
    }

    [Fact]
    public async Task Can_filter_sum_at_primary_endpoint()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.GenerateList(2);

        posts[0].Comments = _fakers.Comment.GenerateSet(2);
        posts[0].Comments.ElementAt(0).NumStars = 0;
        posts[0].Comments.ElementAt(1).NumStars = 1;

        posts[1].Comments = _fakers.Comment.GenerateSet(2);
        posts[1].Comments.ElementAt(0).NumStars = 2;
        posts[1].Comments.ElementAt(1).NumStars = 3;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?filter=greaterThan(sum(comments,numStars),'4')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("blogPosts");
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[1].StringId);
    }

    [Fact]
    public async Task Can_filter_sum_on_count_at_secondary_endpoint()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.GenerateList(2);

        posts[0].Comments = _fakers.Comment.GenerateSet(2);
        posts[0].Comments.ElementAt(0).NumStars = 1;
        posts[0].Comments.ElementAt(1).NumStars = 1;
        posts[0].Contributors = _fakers.Woman.GenerateSet<Woman, Human>(1);

        posts[1].Comments = _fakers.Comment.GenerateSet(2);
        posts[1].Comments.ElementAt(0).NumStars = 2;
        posts[1].Comments.ElementAt(1).NumStars = 2;
        posts[1].Contributors = _fakers.Man.GenerateSet<Man, Human>(2);
        posts[1].Contributors.ElementAt(0).Children = _fakers.Woman.GenerateSet<Woman, Human>(3);
        posts[1].Contributors.ElementAt(1).Children = _fakers.Man.GenerateSet<Man, Human>(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?filter=lessThan(sum(comments,numStars),sum(contributors,count(children)))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("blogPosts");
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[1].StringId);
    }

    [Fact]
    public async Task Can_filter_sum_in_included_resources()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(2);

        blog.Posts[0].Comments = _fakers.Comment.GenerateSet(2);
        blog.Posts[0].Comments.ElementAt(0).NumStars = 1;
        blog.Posts[0].Comments.ElementAt(1).NumStars = 1;

        blog.Posts[1].Comments = _fakers.Comment.GenerateSet(2);
        blog.Posts[1].Comments.ElementAt(0).NumStars = 1;
        blog.Posts[1].Comments.ElementAt(1).NumStars = 2;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?include=posts&filter[posts]=equals(sum(comments,numStars),'3')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("blogs");
        responseDocument.Data.ManyValue[0].Id.Should().Be(blog.StringId);

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Type.Should().Be("blogPosts");
        responseDocument.Included[0].Id.Should().Be(blog.Posts[1].StringId);
    }

    [Fact]
    public async Task Can_sum_compound_member_of_related_to_many()
    {
        // Arrange
        List<Appointment> appointments = _fakers.Appointment.GenerateList(2);

        appointments[0].Cost = new AppointmentCost
        {
            Amount = 30
        };

        appointments[0].Reminders = _fakers.Reminder.GenerateList(2);

        appointments[1].Cost = new AppointmentCost
        {
            Amount = 60
        };

        appointments[1].Reminders = _fakers.Reminder.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Appointment>();
            dbContext.Appointments.AddRange(appointments);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/appointments?filter=greaterThan(sum(reminders,appointment.cost.amount),'100')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("appointments");
        responseDocument.Data.ManyValue[0].Id.Should().Be(appointments[1].StringId);
    }

    [Fact]
    public async Task Cannot_sum_over_to_many_relationship_with_blocked_filter_capability()
    {
        // Arrange
        var parameterValue = new MarkedText("greaterThan(sum(^appointments,cost.amount),'100')", '^');
        string route = $"/calendars?filter={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"Filtering on relationship 'appointments' is not allowed. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    [Fact]
    public async Task Cannot_use_sum_in_scalar_collection_element_scope()
    {
        // Arrange
        var parameterValue = new MarkedText("has(keywords,greaterThan(^sum(reminders,appointment.cost.amount),'1'))", '^');
        string route = $"/appointments?filter={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"Function 'sum' cannot be used on scalar collection elements. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    [Theory]
    [InlineData("appointment.location")]
    [InlineData("appointment.attendees")]
    [InlineData("appointment.keywords")]
    public async Task Cannot_sum_compound_or_collection(string selector)
    {
        // Arrange
        string route = $"/appointments?filter=greaterThan(sum(reminders,{selector}),'1')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");

        error.Detail.Should().StartWith("Field chain on resource type 'reminders' failed to match the pattern: " +
            "zero or more to-one relationships or compound non-collection attributes, followed by a scalar non-collection attribute.");

        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }
}
