using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc.Testing;
using MultiDbContextExample;
using Newtonsoft.Json;
using TestBuildingBlocks;
using Xunit;

namespace MultiDbContextTests
{
    public sealed class ResourceTests : IClassFixture<WebApplicationFactory<Startup>>
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
            var client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/resourceAs");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.OK);

            string responseBody = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseBody);

            document.ManyData.Should().HaveCount(1);
            document.ManyData[0].Attributes["nameA"].Should().Be("SampleA");
        }

        [Fact]
        public async Task Can_get_ResourceBs()
        {
            // Arrange
            var client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/resourceBs");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.OK);

            string responseBody = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseBody);

            document.ManyData.Should().HaveCount(1);
            document.ManyData[0].Attributes["nameB"].Should().Be("SampleB");
        }
    }
}
