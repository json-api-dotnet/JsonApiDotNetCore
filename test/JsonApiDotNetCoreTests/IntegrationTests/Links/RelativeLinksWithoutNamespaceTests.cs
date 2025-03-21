using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Links;

public sealed class RelativeLinksWithoutNamespaceTests : IClassFixture<IntegrationTestContext<RelativeLinksNoNamespaceStartup<LinksDbContext>, LinksDbContext>>
{
    private const string HostPrefix = "";
    private const string PathPrefix = "";

    private readonly IntegrationTestContext<RelativeLinksNoNamespaceStartup<LinksDbContext>, LinksDbContext> _testContext;
    private readonly LinksFakers _fakers = new();

    public RelativeLinksWithoutNamespaceTests(IntegrationTestContext<RelativeLinksNoNamespaceStartup<LinksDbContext>, LinksDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PhotoAlbumsController>();
        testContext.UseController<PhotosController>();

        testContext.ConfigureServices(services => services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>)));

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeTotalResourceCount = true;
    }

    [Fact]
    public async Task Get_primary_resource_by_ID_returns_relative_links()
    {
        // Arrange
        PhotoAlbum album = _fakers.PhotoAlbum.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.PhotoAlbums.Add(album);
            await dbContext.SaveChangesAsync();
        });

        string route = $"{PathPrefix}/photoAlbums/{album.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().BeNull();
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
        responseDocument.Links.DescribedBy.Should().BeNull();

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Links.Should().NotBeNull();
        responseDocument.Data.SingleValue.Links.Self.Should().Be($"{HostPrefix}{route}");

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("photos").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().Be($"{HostPrefix}{route}/relationships/photos");
            value.Links.Related.Should().Be($"{HostPrefix}{route}/photos");
        });
    }

    [Fact]
    public async Task Get_primary_resources_with_include_returns_relative_links()
    {
        // Arrange
        PhotoAlbum album = _fakers.PhotoAlbum.GenerateOne();
        album.Photos = _fakers.Photo.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<PhotoAlbum>();
            dbContext.PhotoAlbums.Add(album);
            await dbContext.SaveChangesAsync();
        });

        const string route = $"{PathPrefix}/photoAlbums?include=photos";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().BeNull();
        responseDocument.Links.First.Should().Be(responseDocument.Links.Self);
        responseDocument.Links.Last.Should().Be(responseDocument.Links.Self);
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
        responseDocument.Links.DescribedBy.Should().BeNull();

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].With(resource =>
        {
            string albumLink = $"{HostPrefix}{PathPrefix}/photoAlbums/{album.StringId}";

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be(albumLink);

            resource.Relationships.Should().ContainKey("photos").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"{albumLink}/relationships/photos");
                value.Links.Related.Should().Be($"{albumLink}/photos");
            });
        });

        responseDocument.Included.Should().HaveCount(1);

        responseDocument.Included[0].With(resource =>
        {
            string photoLink = $"{HostPrefix}{PathPrefix}/photos/{album.Photos.ElementAt(0).StringId}";

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be(photoLink);

            resource.Relationships.Should().ContainKey("album").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"{photoLink}/relationships/album");
                value.Links.Related.Should().Be($"{photoLink}/album");
            });
        });
    }

    [Fact]
    public async Task Get_secondary_resource_returns_relative_links()
    {
        // Arrange
        Photo photo = _fakers.Photo.GenerateOne();
        photo.Album = _fakers.PhotoAlbum.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Photos.Add(photo);
            await dbContext.SaveChangesAsync();
        });

        string route = $"{PathPrefix}/photos/{photo.StringId}/album";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().BeNull();
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
        responseDocument.Links.DescribedBy.Should().BeNull();

        string albumLink = $"{HostPrefix}{PathPrefix}/photoAlbums/{photo.Album.StringId}";

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Links.Should().NotBeNull();
        responseDocument.Data.SingleValue.Links.Self.Should().Be(albumLink);

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("photos").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().Be($"{albumLink}/relationships/photos");
            value.Links.Related.Should().Be($"{albumLink}/photos");
        });
    }

    [Fact]
    public async Task Get_secondary_resources_returns_relative_links()
    {
        // Arrange
        PhotoAlbum album = _fakers.PhotoAlbum.GenerateOne();
        album.Photos = _fakers.Photo.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.PhotoAlbums.Add(album);
            await dbContext.SaveChangesAsync();
        });

        string route = $"{PathPrefix}/photoAlbums/{album.StringId}/photos";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().BeNull();
        responseDocument.Links.First.Should().Be(responseDocument.Links.Self);
        responseDocument.Links.Last.Should().Be(responseDocument.Links.Self);
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
        responseDocument.Links.DescribedBy.Should().BeNull();

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].With(resource =>
        {
            string photoLink = $"{HostPrefix}{PathPrefix}/photos/{album.Photos.ElementAt(0).StringId}";

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be(photoLink);

            resource.Relationships.Should().ContainKey("album").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"{photoLink}/relationships/album");
                value.Links.Related.Should().Be($"{photoLink}/album");
            });
        });
    }

    [Fact]
    public async Task Get_ToOne_relationship_returns_relative_links()
    {
        // Arrange
        Photo photo = _fakers.Photo.GenerateOne();
        photo.Album = _fakers.PhotoAlbum.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Photos.Add(photo);
            await dbContext.SaveChangesAsync();
        });

        string route = $"{PathPrefix}/photos/{photo.StringId}/relationships/album";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().Be($"{HostPrefix}{PathPrefix}/photos/{photo.StringId}/album");
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
        responseDocument.Links.DescribedBy.Should().BeNull();

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Links.Should().BeNull();
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();
    }

    [Fact]
    public async Task Get_ToMany_relationship_returns_relative_links()
    {
        // Arrange
        PhotoAlbum album = _fakers.PhotoAlbum.GenerateOne();
        album.Photos = _fakers.Photo.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.PhotoAlbums.Add(album);
            await dbContext.SaveChangesAsync();
        });

        string route = $"{PathPrefix}/photoAlbums/{album.StringId}/relationships/photos";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().Be($"{HostPrefix}{PathPrefix}/photoAlbums/{album.StringId}/photos");
        responseDocument.Links.First.Should().Be(responseDocument.Links.Self);
        responseDocument.Links.Last.Should().Be(responseDocument.Links.Self);
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
        responseDocument.Links.DescribedBy.Should().BeNull();

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Links.Should().BeNull();
        responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();
    }

    [Fact]
    public async Task Create_resource_with_side_effects_and_include_returns_relative_links()
    {
        // Arrange
        Photo existingPhoto = _fakers.Photo.GenerateOne();

        string newAlbumName = _fakers.PhotoAlbum.GenerateOne().Name;

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
                attributes = new
                {
                    name = newAlbumName
                },
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

        const string route = $"{PathPrefix}/photoAlbums?include=photos";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().BeNull();
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
        responseDocument.Links.DescribedBy.Should().BeNull();

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Links.Should().NotBeNull();

        string albumLink = $"{HostPrefix}{PathPrefix}/photoAlbums/{responseDocument.Data.SingleValue.Id}";

        responseDocument.Data.SingleValue.Links.Self.Should().Be(albumLink);

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("photos").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().Be($"{albumLink}/relationships/photos");
            value.Links.Related.Should().Be($"{albumLink}/photos");
        });

        responseDocument.Included.Should().HaveCount(1);

        responseDocument.Included[0].With(resource =>
        {
            string photoLink = $"{HostPrefix}{PathPrefix}/photos/{existingPhoto.StringId}";

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be(photoLink);

            resource.Relationships.Should().ContainKey("album").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"{photoLink}/relationships/album");
                value.Links.Related.Should().Be($"{photoLink}/album");
            });
        });

        httpResponse.Headers.Location.Should().Be(albumLink);
    }

    [Fact]
    public async Task Update_resource_with_side_effects_and_include_returns_relative_links()
    {
        // Arrange
        Photo existingPhoto = _fakers.Photo.GenerateOne();
        PhotoAlbum existingAlbum = _fakers.PhotoAlbum.GenerateOne();

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

        string route = $"{PathPrefix}/photos/{existingPhoto.StringId}?include=album";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().BeNull();
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
        responseDocument.Links.DescribedBy.Should().BeNull();

        string photoLink = $"{HostPrefix}{PathPrefix}/photos/{existingPhoto.StringId}";

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Links.Should().NotBeNull();
        responseDocument.Data.SingleValue.Links.Self.Should().Be(photoLink);

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("album").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().Be($"{photoLink}/relationships/album");
            value.Links.Related.Should().Be($"{photoLink}/album");
        });

        responseDocument.Included.Should().HaveCount(1);

        responseDocument.Included[0].With(resource =>
        {
            string albumLink = $"{HostPrefix}{PathPrefix}/photoAlbums/{existingAlbum.StringId}";

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be(albumLink);

            resource.Relationships.Should().ContainKey("photos").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"{albumLink}/relationships/photos");
                value.Links.Related.Should().Be($"{albumLink}/photos");
            });
        });
    }
}
