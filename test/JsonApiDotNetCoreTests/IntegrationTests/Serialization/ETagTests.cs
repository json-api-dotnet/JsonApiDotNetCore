using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization;

public sealed class ETagTests : IClassFixture<IntegrationTestContext<TestableStartup<SerializationDbContext>, SerializationDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<SerializationDbContext>, SerializationDbContext> _testContext;
    private readonly SerializationFakers _fakers = new();

    public ETagTests(IntegrationTestContext<TestableStartup<SerializationDbContext>, SerializationDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<MeetingAttendeesController>();
        testContext.UseController<MeetingsController>();
    }

    [Fact]
    public async Task Returns_ETag_for_HEAD_request()
    {
        // Arrange
        List<Meeting> meetings = _fakers.Meeting.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Meeting>();
            dbContext.Meetings.AddRange(meetings);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/meetings";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteHeadAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Headers.ETag.Should().NotBeNull();
        httpResponse.Headers.ETag.IsWeak.Should().BeFalse();
        httpResponse.Headers.ETag.Tag.Should().NotBeNullOrEmpty();

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Returns_ETag_for_GET_request()
    {
        // Arrange
        List<Meeting> meetings = _fakers.Meeting.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Meeting>();
            dbContext.Meetings.AddRange(meetings);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/meetings";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Headers.ETag.Should().NotBeNull();
        httpResponse.Headers.ETag.IsWeak.Should().BeFalse();
        httpResponse.Headers.ETag.Tag.Should().NotBeNullOrEmpty();

        responseDocument.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Returns_no_ETag_for_failed_GET_request()
    {
        // Arrange
        string route = $"/meetings/{Unknown.StringId.For<Meeting, Guid>()}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        httpResponse.Headers.ETag.Should().BeNull();

        responseDocument.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Returns_no_ETag_for_POST_request()
    {
        // Arrange
        var newId = Guid.NewGuid();
        string newTitle = _fakers.Meeting.GenerateOne().Title;

        var requestBody = new
        {
            data = new
            {
                type = "meetings",
                id = newId,
                attributes = new
                {
                    title = newTitle
                }
            }
        };

        const string route = "/meetings";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        httpResponse.Headers.ETag.Should().BeNull();

        responseDocument.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Fails_on_ETag_in_PATCH_request()
    {
        // Arrange
        Meeting existingMeeting = _fakers.Meeting.GenerateOne();

        string newTitle = _fakers.Meeting.GenerateOne().Title;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Meetings.Add(existingMeeting);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "meetings",
                id = existingMeeting.StringId,
                attributes = new
                {
                    title = newTitle
                }
            }
        };

        string route = $"/meetings/{existingMeeting.StringId}";

        Action<HttpRequestHeaders> setRequestHeaders = headers => headers.IfMatch.ParseAdd("\"12345\"");

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.PreconditionFailed);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
        error.Title.Should().Be("Detection of mid-air edit collisions using ETags is not supported.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Header.Should().Be("If-Match");
    }

    [Fact]
    public async Task Returns_NotModified_for_matching_ETag()
    {
        // Arrange
        List<Meeting> meetings = _fakers.Meeting.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Meeting>();
            dbContext.Meetings.AddRange(meetings);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/meetings";

        (HttpResponseMessage httpResponse1, _) = await _testContext.ExecuteGetAsync<string>(route);

        string responseETag = httpResponse1.Headers.ETag!.Tag;

        Action<HttpRequestHeaders> setRequestHeaders2 = headers => headers.IfNoneMatch.ParseAdd($"\"12345\", W/\"67890\", {responseETag}");

        // Act
        (HttpResponseMessage httpResponse2, string responseDocument2) = await _testContext.ExecuteGetAsync<string>(route, setRequestHeaders2);

        // Assert
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.NotModified);

        httpResponse2.Headers.ETag.Should().NotBeNull();
        httpResponse2.Headers.ETag.IsWeak.Should().BeFalse();
        httpResponse2.Headers.ETag.Tag.Should().NotBeNullOrEmpty();

        responseDocument2.Should().BeEmpty();
    }

    [Fact]
    public async Task Returns_content_for_mismatching_ETag()
    {
        // Arrange
        List<Meeting> meetings = _fakers.Meeting.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Meeting>();
            dbContext.Meetings.AddRange(meetings);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/meetings";

        Action<HttpRequestHeaders> setRequestHeaders = headers => headers.IfNoneMatch.ParseAdd("\"Not-a-matching-value\"");

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Headers.ETag.Should().NotBeNull();
        httpResponse.Headers.ETag.IsWeak.Should().BeFalse();
        httpResponse.Headers.ETag.Tag.Should().NotBeNullOrEmpty();

        responseDocument.Should().NotBeEmpty();
    }
}
