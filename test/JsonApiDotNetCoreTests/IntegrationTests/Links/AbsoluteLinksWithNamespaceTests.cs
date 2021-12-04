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

public sealed class AbsoluteLinksWithNamespaceTests
    : IClassFixture<IntegrationTestContext<AbsoluteLinksInApiNamespaceStartup<LinksDbContext>, LinksDbContext>>
{
    private const string HostPrefix = "http://localhost";
    private const string PathPrefix = "/api";

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

        string route = $"{PathPrefix}/photoAlbums/{album.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().BeNull();
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Links.Self.Should().Be($"{HostPrefix}{route}");

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("photos").With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.Should().Be($"{HostPrefix}{route}/relationships/photos");
            value.Links.Related.Should().Be($"{HostPrefix}{route}/photos");
        });
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

        const string route = $"{PathPrefix}/photoAlbums?include=photos";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().BeNull();
        responseDocument.Links.First.Should().Be(responseDocument.Links.Self);
        responseDocument.Links.Last.Should().Be(responseDocument.Links.Self);
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].With(resource =>
        {
            string albumLink = $"{HostPrefix}{PathPrefix}/photoAlbums/{album.StringId}";

            resource.Links.ShouldNotBeNull();
            resource.Links.Self.Should().Be(albumLink);

            resource.Relationships.ShouldContainKey("photos").With(value =>
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.Should().Be($"{albumLink}/relationships/photos");
                value.Links.Related.Should().Be($"{albumLink}/photos");
            });
        });

        responseDocument.Included.ShouldHaveCount(1);

        responseDocument.Included[0].With(resource =>
        {
            string photoLink = $"{HostPrefix}{PathPrefix}/photos/{album.Photos.ElementAt(0).StringId}";

            resource.Links.ShouldNotBeNull();
            resource.Links.Self.Should().Be(photoLink);

            resource.Relationships.ShouldContainKey("album").With(value =>
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.Should().Be($"{photoLink}/relationships/album");
                value.Links.Related.Should().Be($"{photoLink}/album");
            });
        });
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

        string route = $"{PathPrefix}/photos/{photo.StringId}/album";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().BeNull();
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();

        string albumLink = $"{HostPrefix}{PathPrefix}/photoAlbums/{photo.Album.StringId}";

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Links.Self.Should().Be(albumLink);

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("photos").With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.Should().Be($"{albumLink}/relationships/photos");
            value.Links.Related.Should().Be($"{albumLink}/photos");
        });
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

        string route = $"{PathPrefix}/photoAlbums/{album.StringId}/photos";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().BeNull();
        responseDocument.Links.First.Should().Be(responseDocument.Links.Self);
        responseDocument.Links.Last.Should().Be(responseDocument.Links.Self);
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].With(resource =>
        {
            string photoLink = $"{HostPrefix}{PathPrefix}/photos/{album.Photos.ElementAt(0).StringId}";

            resource.Links.ShouldNotBeNull();
            resource.Links.Self.Should().Be(photoLink);

            resource.Relationships.ShouldContainKey("album").With(value =>
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.Should().Be($"{photoLink}/relationships/album");
                value.Links.Related.Should().Be($"{photoLink}/album");
            });
        });
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

        string route = $"{PathPrefix}/photos/{photo.StringId}/relationships/album";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().Be($"{HostPrefix}{PathPrefix}/photos/{photo.StringId}/album");
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();

        responseDocument.Data.SingleValue.ShouldNotBeNull();
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

        string route = $"{PathPrefix}/photoAlbums/{album.StringId}/relationships/photos";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().Be($"{HostPrefix}{PathPrefix}/photoAlbums/{album.StringId}/photos");
        responseDocument.Links.First.Should().Be(responseDocument.Links.Self);
        responseDocument.Links.Last.Should().Be(responseDocument.Links.Self);
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Links.Should().BeNull();
        responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();
    }

    [Fact]
    public async Task Create_resource_with_side_effects_and_include_returns_absolute_links()
    {
        // Arrange
        Photo existingPhoto = _fakers.Photo.Generate();

        string newAlbumName = _fakers.PhotoAlbum.Generate().Name;

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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().BeNull();
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull();

        string albumLink = $"{HostPrefix}{PathPrefix}/photoAlbums/{responseDocument.Data.SingleValue.Id}";

        responseDocument.Data.SingleValue.Links.Self.Should().Be(albumLink);

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("photos").With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.Should().Be($"{albumLink}/relationships/photos");
            value.Links.Related.Should().Be($"{albumLink}/photos");
        });

        responseDocument.Included.ShouldHaveCount(1);

        responseDocument.Included[0].With(resource =>
        {
            string photoLink = $"{HostPrefix}{PathPrefix}/photos/{existingPhoto.StringId}";

            resource.Links.ShouldNotBeNull();
            resource.Links.Self.Should().Be(photoLink);

            resource.Relationships.ShouldContainKey("album").With(value =>
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.Should().Be($"{photoLink}/relationships/album");
                value.Links.Related.Should().Be($"{photoLink}/album");
            });
        });
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

        string route = $"{PathPrefix}/photos/{existingPhoto.StringId}?include=album";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.Related.Should().BeNull();
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();

        string photoLink = $"{HostPrefix}{PathPrefix}/photos/{existingPhoto.StringId}";

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Links.Self.Should().Be(photoLink);

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("album").With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.Should().Be($"{photoLink}/relationships/album");
            value.Links.Related.Should().Be($"{photoLink}/album");
        });

        responseDocument.Included.ShouldHaveCount(1);

        responseDocument.Included[0].With(resource =>
        {
            string albumLink = $"{HostPrefix}{PathPrefix}/photoAlbums/{existingAlbum.StringId}";

            resource.Links.ShouldNotBeNull();
            resource.Links.Self.Should().Be(albumLink);

            resource.Relationships.ShouldContainKey("photos").With(value =>
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.Should().Be($"{albumLink}/relationships/photos");
                value.Links.Related.Should().Be($"{albumLink}/photos");
            });
        });
    }
}
