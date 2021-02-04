using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Links
{
    public sealed class RelativeLinksWithoutNamespaceTests
        : IClassFixture<ExampleIntegrationTestContext<RelativeLinksNoNamespaceStartup<LinksDbContext>, LinksDbContext>>
    {
        private readonly ExampleIntegrationTestContext<RelativeLinksNoNamespaceStartup<LinksDbContext>, LinksDbContext> _testContext;
        private readonly LinksFakers _fakers = new LinksFakers();

        public RelativeLinksWithoutNamespaceTests(ExampleIntegrationTestContext<RelativeLinksNoNamespaceStartup<LinksDbContext>, LinksDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));
            });

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeTotalResourceCount = true;
        }

        [Fact]
        public async Task Get_primary_resource_by_ID_returns_relative_links()
        {
            // Arrange
            var album = _fakers.PhotoAlbum.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.PhotoAlbums.Add(album);
                await dbContext.SaveChangesAsync();
            });

            var route = "/photoAlbums/" + album.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be($"/photoAlbums/{album.StringId}");
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().BeNull();
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Links.Self.Should().Be($"/photoAlbums/{album.StringId}");
            responseDocument.SingleData.Relationships["photos"].Links.Self.Should().Be($"/photoAlbums/{album.StringId}/relationships/photos");
            responseDocument.SingleData.Relationships["photos"].Links.Related.Should().Be($"/photoAlbums/{album.StringId}/photos");
        }

        [Fact]
        public async Task Get_primary_resources_with_include_returns_relative_links()
        {
            // Arrange
            var album = _fakers.PhotoAlbum.Generate();
            album.Photos = _fakers.Photo.Generate(1).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<PhotoAlbum>();
                dbContext.PhotoAlbums.Add(album);
                await dbContext.SaveChangesAsync();
            });

            var route = "/photoAlbums?include=photos";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be("/photoAlbums?include=photos");
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().Be("/photoAlbums?include=photos");
            responseDocument.Links.Last.Should().Be("/photoAlbums?include=photos");
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Links.Self.Should().Be($"/photoAlbums/{album.StringId}");
            responseDocument.ManyData[0].Relationships["photos"].Links.Self.Should().Be($"/photoAlbums/{album.StringId}/relationships/photos");
            responseDocument.ManyData[0].Relationships["photos"].Links.Related.Should().Be($"/photoAlbums/{album.StringId}/photos");

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Links.Self.Should().Be($"/photos/{album.Photos.ElementAt(0).StringId}");
            responseDocument.Included[0].Relationships["album"].Links.Self.Should().Be($"/photos/{album.Photos.ElementAt(0).StringId}/relationships/album");
            responseDocument.Included[0].Relationships["album"].Links.Related.Should().Be($"/photos/{album.Photos.ElementAt(0).StringId}/album");
        }

        [Fact]
        public async Task Get_secondary_resource_returns_relative_links()
        {
            // Arrange
            var photo = _fakers.Photo.Generate();
            photo.Album = _fakers.PhotoAlbum.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Photos.Add(photo);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/photos/{photo.StringId}/album";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be($"/photos/{photo.StringId}/album");
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().BeNull();
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Links.Self.Should().Be($"/photoAlbums/{photo.Album.StringId}");
            responseDocument.SingleData.Relationships["photos"].Links.Self.Should().Be($"/photoAlbums/{photo.Album.StringId}/relationships/photos");
            responseDocument.SingleData.Relationships["photos"].Links.Related.Should().Be($"/photoAlbums/{photo.Album.StringId}/photos");
        }

        [Fact]
        public async Task Get_secondary_resources_returns_relative_links()
        {
            // Arrange
            var album = _fakers.PhotoAlbum.Generate();
            album.Photos = _fakers.Photo.Generate(1).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.PhotoAlbums.Add(album);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/photoAlbums/{album.StringId}/photos";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be($"/photoAlbums/{album.StringId}/photos");
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().Be($"/photoAlbums/{album.StringId}/photos");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Links.Self.Should().Be($"/photos/{album.Photos.ElementAt(0).StringId}");
            responseDocument.ManyData[0].Relationships["album"].Links.Self.Should().Be($"/photos/{album.Photos.ElementAt(0).StringId}/relationships/album");
            responseDocument.ManyData[0].Relationships["album"].Links.Related.Should().Be($"/photos/{album.Photos.ElementAt(0).StringId}/album");
        }

        [Fact]
        public async Task Get_HasOne_relationship_returns_relative_links()
        {
            // Arrange
            var photo = _fakers.Photo.Generate();
            photo.Album = _fakers.PhotoAlbum.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Photos.Add(photo);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/photos/{photo.StringId}/relationships/album";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be($"/photos/{photo.StringId}/relationships/album");
            responseDocument.Links.Related.Should().Be($"/photos/{photo.StringId}/album");
            responseDocument.Links.First.Should().BeNull();
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Links.Should().BeNull();
            responseDocument.SingleData.Relationships.Should().BeNull();
        }

        [Fact]
        public async Task Get_HasMany_relationship_returns_relative_links()
        {
            // Arrange
            var album = _fakers.PhotoAlbum.Generate();
            album.Photos = _fakers.Photo.Generate(1).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.PhotoAlbums.Add(album);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/photoAlbums/{album.StringId}/relationships/photos";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be($"/photoAlbums/{album.StringId}/relationships/photos");
            responseDocument.Links.Related.Should().Be($"/photoAlbums/{album.StringId}/photos");
            responseDocument.Links.First.Should().Be($"/photoAlbums/{album.StringId}/relationships/photos");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Links.Should().BeNull();
            responseDocument.ManyData[0].Relationships.Should().BeNull();
        }

        [Fact]
        public async Task Create_resource_with_side_effects_and_include_returns_relative_links()
        {
            // Arrange
            var existingPhoto = _fakers.Photo.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Photos.Add(existingPhoto);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "photoAlbums",
                    relationships = new
                    {
                        photos = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "photos",
                                    id = existingPhoto.StringId
                                }
                            }
                        }
                    }
                }
            };

            var route = "/photoAlbums?include=photos";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Links.Self.Should().Be("/photoAlbums?include=photos");
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().BeNull();
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            var newAlbumId = responseDocument.SingleData.Id;

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Links.Self.Should().Be($"/photoAlbums/{newAlbumId}");
            responseDocument.SingleData.Relationships["photos"].Links.Self.Should().Be($"/photoAlbums/{newAlbumId}/relationships/photos");
            responseDocument.SingleData.Relationships["photos"].Links.Related.Should().Be($"/photoAlbums/{newAlbumId}/photos");

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Links.Self.Should().Be($"/photos/{existingPhoto.StringId}");
            responseDocument.Included[0].Relationships["album"].Links.Self.Should().Be($"/photos/{existingPhoto.StringId}/relationships/album");
            responseDocument.Included[0].Relationships["album"].Links.Related.Should().Be($"/photos/{existingPhoto.StringId}/album");
        }

        [Fact]
        public async Task Update_resource_with_side_effects_and_include_returns_relative_links()
        {
            // Arrange
            var existingPhoto = _fakers.Photo.Generate();
            var existingAlbum = _fakers.PhotoAlbum.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingPhoto, existingAlbum);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "photos",
                    id = existingPhoto.StringId,
                    relationships = new
                    {
                        album = new
                        {
                            data = new
                            {
                                type = "photoAlbums",
                                id = existingAlbum.StringId
                            }
                        }
                    }
                }
            };

            var route = $"/photos/{existingPhoto.StringId}?include=album";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be($"/photos/{existingPhoto.StringId}?include=album");
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().BeNull();
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Links.Self.Should().Be($"/photos/{existingPhoto.StringId}");
            responseDocument.SingleData.Relationships["album"].Links.Self.Should().Be($"/photos/{existingPhoto.StringId}/relationships/album");
            responseDocument.SingleData.Relationships["album"].Links.Related.Should().Be($"/photos/{existingPhoto.StringId}/album");

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Links.Self.Should().Be($"/photoAlbums/{existingAlbum.StringId}");
            responseDocument.Included[0].Relationships["photos"].Links.Self.Should().Be($"/photoAlbums/{existingAlbum.StringId}/relationships/photos");
            responseDocument.Included[0].Relationships["photos"].Links.Related.Should().Be($"/photoAlbums/{existingAlbum.StringId}/photos");
        }
    }
}
