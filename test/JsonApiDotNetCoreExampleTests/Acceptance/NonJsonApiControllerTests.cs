using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    public sealed class NonJsonApiControllerTests
    {
        [Fact]
        public async Task NonJsonApiController_Skips_Middleware_And_Formatters_On_Get()
        {
            // Arrange
            const string route = "testValues";

            var builder = new WebHostBuilder().UseStartup<Startup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("[\"value\"]", body);
        }

        [Fact]
        public async Task NonJsonApiController_Skips_Middleware_And_Formatters_On_Post()
        {
            // Arrange
            const string route = "testValues?name=Jack";

            var builder = new WebHostBuilder().UseStartup<Startup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, route) {Content = new StringContent("XXX")};
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello, Jack", body);
        }

        [Fact]
        public async Task NonJsonApiController_Skips_Middleware_And_Formatters_On_Patch()
        {
            // Arrange
            const string route = "testValues?name=Jack";

            var builder = new WebHostBuilder().UseStartup<Startup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Patch, route) {Content = new StringContent("XXX")};

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello, Jack", body);
        }

        [Fact]
        public async Task NonJsonApiController_Skips_Middleware_And_Formatters_On_Delete()
        {
            // Arrange
            const string route = "testValues";

            var builder = new WebHostBuilder().UseStartup<Startup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Delete, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("Deleted", body);
        }
    }
}
