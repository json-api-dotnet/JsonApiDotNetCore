using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc.Testing;
using MultiDbContextExample;
using TestBuildingBlocks;
using Xunit;

namespace MultiDbContextTests
{
    public sealed class ResourceTests : IntegrationTest, IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public ResourceTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Can_get_ResourceAs()
        {
            // Arrange
            var route = "/resourceAs";

            // Act
            var (httpResponse, responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["nameA"].Should().Be("SampleA");
        }

        [Fact]
        public async Task Can_get_ResourceBs()
        {
            // Arrange
            var route = "/resourceBs";

            // Act
            var (httpResponse, responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["nameB"].Should().Be("SampleB");
        }

        protected override HttpClient CreateClient()
        {
            return _factory.CreateClient();
        }
    }
}
