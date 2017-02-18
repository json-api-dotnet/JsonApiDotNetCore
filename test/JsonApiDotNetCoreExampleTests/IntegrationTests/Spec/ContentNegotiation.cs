using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetCoreDocs;
using DotNetCoreDocs.Models;
using DotNetCoreDocs.Writers;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Spec
{
    [Collection("WebHostCollection")]
    public class ContentNegotiation
    {
        private DocsFixture<Startup, JsonDocWriter> _fixture;
        public ContentNegotiation(DocsFixture<Startup, JsonDocWriter> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Server_Sends_Correct_ContentType_Header()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todo-items";
            var description = new RequestProperties("Server Sends Correct Content Type Header");
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            
            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/vnd.api+json", response.Content.Headers.ContentType.ToString());
        }
    }
}
