using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Links
{
    public sealed class AbsoluteLinksWithNamespaceTests
        : IClassFixture<IntegrationTestContext<AbsoluteLinksInApiNamespaceStartup<LinksDbContext>, LinksDbContext>>
    {
        private const string HostPrefix = "http://localhost";

        private readonly IntegrationTestContext<AbsoluteLinksInApiNamespaceStartup<LinksDbContext>, LinksDbContext> _testContext;
        private readonly LinksFakers _fakers = new();

        public AbsoluteLinksWithNamespaceTests(IntegrationTestContext<AbsoluteLinksInApiNamespaceStartup<LinksDbContext>, LinksDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<PhotoAlbumsController>();
            testContext.UseController<PhotosController>();

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));
            });

            var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeTotalResourceCount = true;
        }

        [Fact]
        public async Task Get_primary_resource_by_ID_returns_absolute_links()
        {
            // Arrange
            PhotoAlbum album = _fakers.PhotoAlbum.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.PhotoAlbums.Add(album);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/api/photoAlbums/{album.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().BeNull();
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Links.Self.Should().Be($"{HostPrefix}{route}");
            responseDocument.Data.SingleValue.Relationships["photos"].Links.Self.Should().Be($"{HostPrefix}{route}/relationships/photos");
            responseDocument.Data.SingleValue.Relationships["photos"].Links.Related.Should().Be($"{HostPrefix}{route}/photos");
        }

        [Fact]
        public async Task Get_primary_resources_with_include_returns_absolute_links()
        {
            // Arrange
            PhotoAlbum album = _fakers.PhotoAlbum.Generate();
            album.Photos = _fakers.Photo.Generate(1).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<PhotoAlbum>();
                dbContext.PhotoAlbums.Add(album);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/api/photoAlbums?include=photos";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().Be($"{HostPrefix}{route}");
            responseDocument.Links.Last.Should().Be($"{HostPrefix}{route}");
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            string albumLink = $"{HostPrefix}/api/photoAlbums/{album.StringId}";

            responseDocument.Data.ManyValue.Should().HaveCount(1);
            responseDocument.Data.ManyValue[0].Links.Self.Should().Be(albumLink);
            responseDocument.Data.ManyValue[0].Relationships["photos"].Links.Self.Should().Be($"{albumLink}/relationships/photos");
            responseDocument.Data.ManyValue[0].Relationships["photos"].Links.Related.Should().Be($"{albumLink}/photos");

            string photoLink = $"{HostPrefix}/api/photos/{album.Photos.ElementAt(0).StringId}";

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Links.Self.Should().Be(photoLink);
            responseDocument.Included[0].Relationships["album"].Links.Self.Should().Be($"{photoLink}/relationships/album");
            responseDocument.Included[0].Relationships["album"].Links.Related.Should().Be($"{photoLink}/album");
        }

        [Fact]
        public async Task Get_secondary_resource_returns_absolute_links()
        {
            // Arrange
            Photo photo = _fakers.Photo.Generate();
            photo.Album = _fakers.PhotoAlbum.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Photos.Add(photo);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/api/photos/{photo.StringId}/album";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().BeNull();
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            string albumLink = $"{HostPrefix}/api/photoAlbums/{photo.Album.StringId}";

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Links.Self.Should().Be(albumLink);
            responseDocument.Data.SingleValue.Relationships["photos"].Links.Self.Should().Be($"{albumLink}/relationships/photos");
            responseDocument.Data.SingleValue.Relationships["photos"].Links.Related.Should().Be($"{albumLink}/photos");
        }

        [Fact]
        public async Task Get_secondary_resources_returns_absolute_links()
        {
            // Arrange
            PhotoAlbum album = _fakers.PhotoAlbum.Generate();
            album.Photos = _fakers.Photo.Generate(1).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.PhotoAlbums.Add(album);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/api/photoAlbums/{album.StringId}/photos";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().Be($"{HostPrefix}{route}");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            string photoLink = $"{HostPrefix}/api/photos/{album.Photos.ElementAt(0).StringId}";

            responseDocument.Data.ManyValue.Should().HaveCount(1);
            responseDocument.Data.ManyValue[0].Links.Self.Should().Be(photoLink);
            responseDocument.Data.ManyValue[0].Relationships["album"].Links.Self.Should().Be($"{photoLink}/relationships/album");
            responseDocument.Data.ManyValue[0].Relationships["album"].Links.Related.Should().Be($"{photoLink}/album");
        }

        [Fact]
        public async Task Get_ToOne_relationship_returns_absolute_links()
        {
            // Arrange
            Photo photo = _fakers.Photo.Generate();
            photo.Album = _fakers.PhotoAlbum.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Photos.Add(photo);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/api/photos/{photo.StringId}/relationships/album";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
            responseDocument.Links.Related.Should().Be($"{HostPrefix}/api/photos/{photo.StringId}/album");
            responseDocument.Links.First.Should().BeNull();
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Links.Should().BeNull();
            responseDocument.Data.SingleValue.Relationships.Should().BeNull();
        }

        [Fact]
        public async Task Get_ToMany_relationship_returns_absolute_links()
        {
            // Arrange
            PhotoAlbum album = _fakers.PhotoAlbum.Generate();
            album.Photos = _fakers.Photo.Generate(1).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.PhotoAlbums.Add(album);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/api/photoAlbums/{album.StringId}/relationships/photos";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
            responseDocument.Links.Related.Should().Be($"{HostPrefix}/api/photoAlbums/{album.StringId}/photos");
            responseDocument.Links.First.Should().Be($"{HostPrefix}{route}");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            responseDocument.Data.ManyValue.Should().HaveCount(1);
            responseDocument.Data.ManyValue[0].Links.Should().BeNull();
            responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();
        }

        [Fact]
        public async Task Create_resource_with_side_effects_and_include_returns_absolute_links()
        {
            // Arrange
            Photo existingPhoto = _fakers.Photo.Generate();

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

            const string route = "/api/photoAlbums?include=photos";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().BeNull();
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            string albumLink = $"{HostPrefix}/api/photoAlbums/{responseDocument.Data.SingleValue.Id}";

            responseDocument.Data.SingleValue.Links.Self.Should().Be(albumLink);
            responseDocument.Data.SingleValue.Relationships["photos"].Links.Self.Should().Be($"{albumLink}/relationships/photos");
            responseDocument.Data.SingleValue.Relationships["photos"].Links.Related.Should().Be($"{albumLink}/photos");

            string photoLink = $"{HostPrefix}/api/photos/{existingPhoto.StringId}";

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Links.Self.Should().Be(photoLink);
            responseDocument.Included[0].Relationships["album"].Links.Self.Should().Be($"{photoLink}/relationships/album");
            responseDocument.Included[0].Relationships["album"].Links.Related.Should().Be($"{photoLink}/album");
        }

        [Fact]
        public async Task Update_resource_with_side_effects_and_include_returns_absolute_links()
        {
            // Arrange
            Photo existingPhoto = _fakers.Photo.Generate();
            PhotoAlbum existingAlbum = _fakers.PhotoAlbum.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddInRange(existingPhoto, existingAlbum);
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

            string route = $"/api/photos/{existingPhoto.StringId}?include=album";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().BeNull();
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            string photoLink = $"{HostPrefix}/api/photos/{existingPhoto.StringId}";

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Links.Self.Should().Be(photoLink);
            responseDocument.Data.SingleValue.Relationships["album"].Links.Self.Should().Be($"{photoLink}/relationships/album");
            responseDocument.Data.SingleValue.Relationships["album"].Links.Related.Should().Be($"{photoLink}/album");

            string albumLink = $"{HostPrefix}/api/photoAlbums/{existingAlbum.StringId}";

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Links.Self.Should().Be(albumLink);
            responseDocument.Included[0].Relationships["photos"].Links.Self.Should().Be($"{albumLink}/relationships/photos");
            responseDocument.Included[0].Relationships["photos"].Links.Related.Should().Be($"{albumLink}/photos");
        }
    }
}
