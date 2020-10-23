using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc.Testing;
using MultiDbContextExample;
using Newtonsoft.Json;
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
        public async Task Can_Get_ResourceAs()
        {
            // Arrange
            var client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/resourceAs");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            AssertStatusCode(HttpStatusCode.OK, response);

            string responseBody = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseBody);

            document.ManyData.Should().HaveCount(1);
            document.ManyData[0].Attributes["nameA"].Should().Be("SampleA");
        }

        [Fact]
        public async Task Can_Get_ResourceBs()
        {
            // Arrange
            var client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/resourceBs");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            AssertStatusCode(HttpStatusCode.OK, response);

            string responseBody = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseBody);

            document.ManyData.Should().HaveCount(1);
            document.ManyData[0].Attributes["nameB"].Should().Be("SampleB");
        }

        private static void AssertStatusCode(HttpStatusCode expected, HttpResponseMessage response)
        {
            if (expected != response.StatusCode)
            {
                var responseBody = response.Content.ReadAsStringAsync().Result;
                Assert.True(expected == response.StatusCode, $"Got {response.StatusCode} status code instead of {expected}. Response body: {responseBody}");
            }
        }
    }
}
