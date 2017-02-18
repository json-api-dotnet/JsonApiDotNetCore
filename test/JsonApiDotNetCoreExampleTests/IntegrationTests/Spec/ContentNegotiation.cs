using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        [Fact]
        public async Task Server_Responds_415_With_MediaType_Parameters()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todo-items";
            var description = new RequestProperties("Server responds with 415 if request contains media type parameters");
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            request.Content = new StringContent(string.Empty);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");
            request.Content.Headers.ContentType.CharSet = "ISO-8859-4";

            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }

        [Fact]
        public async Task ServerResponds_406_If_RequestAcceptHeader_Contains_MediaTypeParameters()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todo-items";
            var description = new RequestProperties("Server responds with 406...");
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var acceptHeader = new MediaTypeWithQualityHeaderValue("application/vnd.api+json");
            acceptHeader.CharSet = "ISO-8859-4";
            client.DefaultRequestHeaders
                    .Accept
                    .Add(acceptHeader);
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
        }
    }
}
