using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.BackgroundProcessing;

public sealed class BackgroundProcessingTests : IClassFixture<IntegrationTestContext<TestableStartup<BackgroundJobDbContext>, BackgroundJobDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<BackgroundJobDbContext>, BackgroundJobDbContext> _testContext;

    public BackgroundProcessingTests(IntegrationTestContext<TestableStartup<BackgroundJobDbContext>, BackgroundJobDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WorkItemsController>();
    }

    [Fact]
    public async Task Returns_202_Accepted_with_Location_header_when_offloading_to_background()
    {
        // Arrange
        string route = "/workItems";
        
        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                attributes = new
                {
                    description = "Process this in background"
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Accepted);
        httpResponse.Headers.Location.Should().NotBeNull();
        httpResponse.Headers.Location!.ToString().Should().StartWith("/workItems/");
    }

    [Fact]
    public async Task Can_retrieve_result_from_Location_after_background_processing()
    {
        // Arrange
        var workItem = new WorkItem
        {
            Description = "Test work item",
            Status = "Completed"
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WorkItems.Add(workItem);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/workItems/{workItem.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(workItem.StringId);
        
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("description")
            .WhoseValue.Should().Be("Test work item");
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("status")
            .WhoseValue.Should().Be("Completed");
    }

    [Fact]
    public async Task Captures_query_string_when_offloading_to_background()
    {
        // Arrange
        string route = "/workItems";
        
        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                attributes = new
                {
                    description = "Query string test"
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Accepted);
        httpResponse.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_404_when_background_job_result_not_yet_available()
    {
        // Arrange
        string nonExistentRoute = "/workItems/99999999";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(nonExistentRoute);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);
        
        responseDocument.Errors.Should().HaveCount(1);
        responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}