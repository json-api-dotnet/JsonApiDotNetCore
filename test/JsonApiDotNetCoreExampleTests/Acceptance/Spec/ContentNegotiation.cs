using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public sealed class ContentNegotiation
    {
        [Fact]
        public async Task Server_Sends_Correct_ContentType_Header()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/vnd.api+json", response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task Server_Responds_415_With_MediaType_Parameters()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route) {Content = new StringContent(string.Empty)};
            request.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/vnd.api+json") {CharSet = "ISO-8859-4"};

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The specified Content-Type header value is not supported.", errorDocument.Errors[0].Title);
            Assert.Equal("Please specify 'application/vnd.api+json' for the Content-Type header value.", errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task ServerResponds_406_If_RequestAcceptHeader_Contains_MediaTypeParameters()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var acceptHeader = new MediaTypeWithQualityHeaderValue("application/vnd.api+json") {CharSet = "ISO-8859-4"};
            client.DefaultRequestHeaders
                    .Accept
                    .Add(acceptHeader);
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.NotAcceptable, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The specified Accept header value is not supported.", errorDocument.Errors[0].Title);
            Assert.Equal("Please specify 'application/vnd.api+json' for the Accept header value.", errorDocument.Errors[0].Detail);
        }
    }
}
