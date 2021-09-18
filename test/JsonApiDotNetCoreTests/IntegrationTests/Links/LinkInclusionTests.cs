using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Links
{
    public sealed class LinkInclusionTests : IClassFixture<IntegrationTestContext<TestableStartup<LinksDbContext>, LinksDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<LinksDbContext>, LinksDbContext> _testContext;
        private readonly LinksFakers _fakers = new();

        public LinkInclusionTests(IntegrationTestContext<TestableStartup<LinksDbContext>, LinksDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<PhotosController>();
            testContext.UseController<PhotoLocationsController>();
        }

        [Fact]
        public async Task Get_primary_resource_with_include_applies_links_visibility_from_ResourceLinksAttribute()
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

            responseDocument.Links.Should().BeNull();

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Links.Should().BeNull();
            responseDocument.Data.SingleValue.Relationships["photo"].Links.Self.Should().BeNull();
            responseDocument.Data.SingleValue.Relationships["photo"].Links.Related.Should().NotBeNull();
            responseDocument.Data.SingleValue.Relationships["album"].Links.Should().BeNull();

            responseDocument.Included.Should().HaveCount(2);

            responseDocument.Included[0].Links.Self.Should().NotBeNull();
            responseDocument.Included[0].Relationships["location"].Links.Self.Should().NotBeNull();
            responseDocument.Included[0].Relationships["location"].Links.Related.Should().NotBeNull();

            responseDocument.Included[1].Links.Self.Should().NotBeNull();
            responseDocument.Included[1].Relationships["photos"].Links.Self.Should().NotBeNull();
            responseDocument.Included[1].Relationships["photos"].Links.Related.Should().NotBeNull();
        }

        [Fact]
        public async Task Get_secondary_resource_applies_links_visibility_from_ResourceLinksAttribute()
        {
            // Arrange
            Photo photo = _fakers.Photo.Generate();
            photo.Location = _fakers.PhotoLocation.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Photos.Add(photo);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/photos/{photo.StringId}/location";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Should().BeNull();

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Links.Should().BeNull();
            responseDocument.Data.SingleValue.Relationships["photo"].Links.Self.Should().BeNull();
            responseDocument.Data.SingleValue.Relationships["photo"].Links.Related.Should().NotBeNull();
            responseDocument.Data.SingleValue.Relationships.Should().NotContainKey("album");
        }
    }
}
