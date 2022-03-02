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
    public async Task Hides_Self_link_in_included_resources_for_unregistered_controllers()
    {
        // Arrange
        PhotoLocation location = _fakers.PhotoLocation.Generate();
        location.Photo = _fakers.Photo.Generate();
        location.Album = _fakers.PhotoAlbum.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.PhotoLocations.Add(location);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/photoLocations/{location.StringId}?include=photo,album";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.ShouldHaveCount(2);

        responseDocument.Included.Should().ContainSingle(resource => resource.Type == "photos").Subject.With(resource =>
        {
            resource.Links.Should().BeNull();

            resource.Relationships.ShouldContainKey("location").With(value =>
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
            });
        });

        responseDocument.Included.Should().ContainSingle(resource => resource.Type == "photoAlbums").Subject.With(resource =>
        {
            resource.Links.Should().BeNull();
        });
    }
}
