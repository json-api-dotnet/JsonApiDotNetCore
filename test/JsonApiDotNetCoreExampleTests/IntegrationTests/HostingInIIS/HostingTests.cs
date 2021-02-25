using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.HostingInIIS
{
    public sealed class HostingTests : IClassFixture<ExampleIntegrationTestContext<HostingStartup<HostingDbContext>, HostingDbContext>>
    {
        private const string HostPrefix = "http://localhost";

        private readonly ExampleIntegrationTestContext<HostingStartup<HostingDbContext>, HostingDbContext> _testContext;
        private readonly HostingFakers _fakers = new HostingFakers();

        public HostingTests(ExampleIntegrationTestContext<HostingStartup<HostingDbContext>, HostingDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Get_primary_resources_with_include_returns_links()
        {
            // Arrange
            ArtGallery gallery = _fakers.ArtGallery.Generate();
            gallery.Paintings = _fakers.Painting.Generate(1).ToHashSet();

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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be(HostPrefix + route);
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().Be(HostPrefix + route);
            responseDocument.Links.Last.Should().Be(HostPrefix + route);
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            string galleryLink = HostPrefix + $"/iis-application-virtual-directory/public-api/artGalleries/{gallery.StringId}";

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Links.Self.Should().Be(galleryLink);
            responseDocument.ManyData[0].Relationships["paintings"].Links.Self.Should().Be(galleryLink + "/relationships/paintings");
            responseDocument.ManyData[0].Relationships["paintings"].Links.Related.Should().Be(galleryLink + "/paintings");

            // TODO: The next link is wrong: it should use the custom route.
            // https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/956
            string paintingLink = HostPrefix + $"/iis-application-virtual-directory/public-api/paintings/{gallery.Paintings.ElementAt(0).StringId}";

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Links.Self.Should().Be(paintingLink);
            responseDocument.Included[0].Relationships["exposedAt"].Links.Self.Should().Be(paintingLink + "/relationships/exposedAt");
            responseDocument.Included[0].Relationships["exposedAt"].Links.Related.Should().Be(paintingLink + "/exposedAt");
        }

        [Fact]
        public async Task Get_primary_resources_with_include_on_custom_route_returns_links()
        {
            // Arrange
            Painting painting = _fakers.Painting.Generate();
            painting.ExposedAt = _fakers.ArtGallery.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Painting>();
                dbContext.Paintings.Add(painting);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/iis-application-virtual-directory/custom/path/to/paintings?include=exposedAt";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be(HostPrefix + route);
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().Be(HostPrefix + route);
            responseDocument.Links.Last.Should().Be(HostPrefix + route);
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            string paintingLink = HostPrefix + $"/iis-application-virtual-directory/custom/path/to/paintings/{painting.StringId}";

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Links.Self.Should().Be(paintingLink);
            responseDocument.ManyData[0].Relationships["exposedAt"].Links.Self.Should().Be(paintingLink + "/relationships/exposedAt");
            responseDocument.ManyData[0].Relationships["exposedAt"].Links.Related.Should().Be(paintingLink + "/exposedAt");

            // TODO: The next link is wrong: it should not use the custom route.
            // https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/956
            string galleryLink = HostPrefix + $"/iis-application-virtual-directory/custom/path/to/artGalleries/{painting.ExposedAt.StringId}";

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Links.Self.Should().Be(galleryLink);
            responseDocument.Included[0].Relationships["paintings"].Links.Self.Should().Be(galleryLink + "/relationships/paintings");
            responseDocument.Included[0].Relationships["paintings"].Links.Related.Should().Be(galleryLink + "/paintings");
        }
    }
}
