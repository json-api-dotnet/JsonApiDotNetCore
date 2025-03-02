using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Links;

public sealed class LinkInclusionIncludeTests : IClassFixture<IntegrationTestContext<TestableStartup<LinksDbContext>, LinksDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<LinksDbContext>, LinksDbContext> _testContext;
    private readonly LinksFakers _fakers = new();

    public LinkInclusionIncludeTests(IntegrationTestContext<TestableStartup<LinksDbContext>, LinksDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PhotoLocationsController>();
    }

    [Fact]
    public async Task Hides_links_for_unregistered_controllers()
    {
        // Arrange
        PhotoLocation location = _fakers.PhotoLocation.GenerateOne();
        location.Photo = _fakers.Photo.GenerateOne();
        location.Album = _fakers.PhotoAlbum.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.PhotoLocations.Add(location);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/photoLocations/{location.StringId}?include=photo,album";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("photo").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Links.Should().NotBeNull();
        });

        responseDocument.Included.Should().HaveCount(2);

        responseDocument.Included.Should().ContainSingle(resource => resource.Type == "photos").Subject.With(resource =>
        {
            resource.Links.Should().BeNull();
            resource.Relationships.Should().BeNull();
        });

        responseDocument.Included.Should().ContainSingle(resource => resource.Type == "photoAlbums").Subject.With(resource =>
        {
            resource.Links.Should().BeNull();
            resource.Relationships.Should().BeNull();
        });
    }
}
