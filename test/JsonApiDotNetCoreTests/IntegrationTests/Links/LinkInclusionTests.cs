using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Links;

public sealed class LinkInclusionTests : IClassFixture<IntegrationTestContext<TestableStartup<LinksDbContext>, LinksDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<LinksDbContext>, LinksDbContext> _testContext;
    private readonly LinksFakers _fakers = new();

    public LinkInclusionTests(IntegrationTestContext<TestableStartup<LinksDbContext>, LinksDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PhotosController>();
        testContext.UseController<PhotoLocationsController>();
        testContext.UseController<PhotoAlbumsController>();
    }

    [Fact]
    public async Task Get_primary_resource_with_include_applies_links_visibility_from_ResourceLinksAttribute()
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

        responseDocument.Links.Should().BeNull();

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Links.Should().BeNull();

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("photo").WhoseValue.With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.Should().BeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("album").WhoseValue.With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.Should().BeNull();
        });

        responseDocument.Included.Should().HaveCount(2);

        responseDocument.Included[0].With(resource =>
        {
            resource.Links.ShouldNotBeNull();
            resource.Links.Self.ShouldNotBeNull();

            resource.Relationships.Should().ContainKey("location").WhoseValue.With(value =>
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.ShouldNotBeNull();
                value.Links.Related.ShouldNotBeNull();
            });
        });

        responseDocument.Included[1].With(resource =>
        {
            resource.Links.ShouldNotBeNull();
            resource.Links.Self.ShouldNotBeNull();

            resource.Relationships.Should().ContainKey("photos").WhoseValue.With(value =>
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.ShouldNotBeNull();
                value.Links.Related.ShouldNotBeNull();
            });
        });
    }

    [Fact]
    public async Task Get_secondary_resource_applies_links_visibility_from_ResourceLinksAttribute()
    {
        // Arrange
        Photo photo = _fakers.Photo.GenerateOne();
        photo.Location = _fakers.PhotoLocation.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Photos.Add(photo);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/photos/{photo.StringId}/location";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().BeNull();

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Links.Should().BeNull();

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("photo").WhoseValue.With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.Should().BeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        responseDocument.Data.SingleValue.Relationships.Should().NotContainKey("album");
    }
}
