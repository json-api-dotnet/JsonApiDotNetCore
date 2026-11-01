using System.Net;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Blobs;

// Unlike BlobTests, this class uses the real change tracker, so the 200/204 decision is not forced.
public sealed class BlobChangeTrackingTests : IClassFixture<IntegrationTestContext<TestableStartup<BlobDbContext>, BlobDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<BlobDbContext>, BlobDbContext> _testContext;
    private readonly BlobFakers _fakers = new();

    public BlobChangeTrackingTests(IntegrationTestContext<TestableStartup<BlobDbContext>, BlobDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ImageContainersController>();
    }

    [Fact]
    public async Task Can_patch_identical_binary_content_and_get_204()
    {
        // Arrange
        ImageContainer existingContainer = _fakers.ImageContainer.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ImageContainers.Add(existingContainer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "imageContainers",
                id = existingContainer.StringId,
                attributes = new
                {
                    data = Convert.ToBase64String(existingContainer.Data),
                    thumbnail = Convert.ToBase64String(existingContainer.Thumbnail!)
                }
            }
        };

        string route = $"/imageContainers/{existingContainer.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseText) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        // byte[] is classified as a collection of bytes, so it is compared by content: identical content is not a change.
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseText.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            ImageContainer containerInDatabase = await dbContext.ImageContainers.FirstWithIdAsync(existingContainer.Id);

            containerInDatabase.Data.Should().Equal(existingContainer.Data);
            containerInDatabase.Thumbnail.Should().Equal(existingContainer.Thumbnail);
        });
    }
}
