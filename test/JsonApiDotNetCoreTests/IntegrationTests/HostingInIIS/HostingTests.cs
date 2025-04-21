using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS;

public sealed class HostingTests : IClassFixture<IntegrationTestContext<HostingStartup<HostingDbContext>, HostingDbContext>>
{
    private const string HostPrefix = "http://localhost";

    private readonly IntegrationTestContext<HostingStartup<HostingDbContext>, HostingDbContext> _testContext;
    private readonly HostingFakers _fakers = new();

    public HostingTests(IntegrationTestContext<HostingStartup<HostingDbContext>, HostingDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PaintingsController>();
        testContext.UseController<ArtGalleriesController>();
    }

    [Fact]
    public async Task Get_primary_resources_with_include_returns_links()
    {
        // Arrange
        ArtGallery gallery = _fakers.ArtGallery.GenerateOne();
        gallery.Paintings = _fakers.Painting.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<ArtGallery>();
            dbContext.ArtGalleries.Add(gallery);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/iis-application-virtual-directory/public-api/artGalleries?include=paintings";

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

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].With(resource =>
        {
            string galleryLink = $"{HostPrefix}/iis-application-virtual-directory/public-api/artGalleries/{gallery.StringId}";

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be(galleryLink);

            resource.Relationships.Should().ContainKey("paintings").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"{galleryLink}/relationships/paintings");
                value.Links.Related.Should().Be($"{galleryLink}/paintings");
            });
        });

        string paintingLink = $"{HostPrefix}/iis-application-virtual-directory/custom/path/to/paintings-of-the-world/{gallery.Paintings.ElementAt(0).StringId}";

        responseDocument.Included.Should().HaveCount(1);

        responseDocument.Included[0].With(resource =>
        {
            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be(paintingLink);

            resource.Relationships.Should().ContainKey("exposedAt").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"{paintingLink}/relationships/exposedAt");
                value.Links.Related.Should().Be($"{paintingLink}/exposedAt");
            });
        });
    }

    [Fact]
    public async Task Get_primary_resources_with_include_on_custom_route_returns_links()
    {
        // Arrange
        Painting painting = _fakers.Painting.GenerateOne();
        painting.ExposedAt = _fakers.ArtGallery.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Painting>();
            dbContext.Paintings.Add(painting);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/iis-application-virtual-directory/custom/path/to/paintings-of-the-world?include=exposedAt";

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

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].With(resource =>
        {
            string paintingLink = $"{HostPrefix}/iis-application-virtual-directory/custom/path/to/paintings-of-the-world/{painting.StringId}";

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be(paintingLink);

            resource.Relationships.Should().ContainKey("exposedAt").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"{paintingLink}/relationships/exposedAt");
                value.Links.Related.Should().Be($"{paintingLink}/exposedAt");
            });
        });

        responseDocument.Included.Should().HaveCount(1);

        responseDocument.Included[0].With(resource =>
        {
            string galleryLink = $"{HostPrefix}/iis-application-virtual-directory/public-api/artGalleries/{painting.ExposedAt.StringId}";

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be(galleryLink);

            resource.Relationships.Should().ContainKey("paintings").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"{galleryLink}/relationships/paintings");
                value.Links.Related.Should().Be($"{galleryLink}/paintings");
            });
        });
    }
}
