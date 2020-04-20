using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class ContentNegotiationTests
    {
        private readonly TestFixture<Startup> _fixture;

        public ContentNegotiationTests(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Server_Sends_Correct_ContentType_Header()
        {
            // Arrange
            var builder = new WebHostBuilder().UseStartup<Startup>();
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(HeaderConstants.MediaType, response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task Respond_415_If_Content_Type_Header_Is_Not_JsonApi_Media_Type()
        {
            // Arrange
            var builder = new WebHostBuilder().UseStartup<Startup>();
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(new HttpMethod("POST"), route) {Content = new StringContent(string.Empty)};
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The specified Content-Type header value is not supported.", errorDocument.Errors[0].Title);
            Assert.Equal("Please specify 'application/vnd.api+json' instead of 'text/html' for the Content-Type header value.", errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Respond_201_If_Content_Type_Header_Is_JsonApi_Media_Type()
        {
            // Arrange
            var serializer = _fixture.GetSerializer<TodoItem>(e => new { e.Description });
            var todoItem = new TodoItem {Description = "something not to forget"};

            var builder = new WebHostBuilder().UseStartup<Startup>();
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, route) {Content = new StringContent(serializer.Serialize(todoItem))};
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Respond_415_If_Content_Type_Header_Is_JsonApi_Media_Type_With_Profile()
        {
            // Arrange
            var builder = new WebHostBuilder().UseStartup<Startup>();
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(new HttpMethod("POST"), route) {Content = new StringContent(string.Empty)};
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(HeaderConstants.MediaType + "; profile=something");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The specified Content-Type header value is not supported.", errorDocument.Errors[0].Title);
            Assert.Equal("Please specify 'application/vnd.api+json' instead of 'application/vnd.api+json; profile=something' for the Content-Type header value.", errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Respond_415_If_Content_Type_Header_Is_JsonApi_Media_Type_With_Extension()
        {
            // Arrange
            var builder = new WebHostBuilder().UseStartup<Startup>();
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(new HttpMethod("POST"), route) {Content = new StringContent(string.Empty)};
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(HeaderConstants.MediaType + "; ext=something");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The specified Content-Type header value is not supported.", errorDocument.Errors[0].Title);
            Assert.Equal("Please specify 'application/vnd.api+json' instead of 'application/vnd.api+json; ext=something' for the Content-Type header value.", errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Respond_415_If_Content_Type_Header_Is_JsonApi_Media_Type_With_CharSet()
        {
            // Arrange
            var builder = new WebHostBuilder().UseStartup<Startup>();
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, route) {Content = new StringContent(string.Empty)};
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(HeaderConstants.MediaType + "; charset=ISO-8859-4");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The specified Content-Type header value is not supported.", errorDocument.Errors[0].Title);
            Assert.Equal("Please specify 'application/vnd.api+json' instead of 'application/vnd.api+json; charset=ISO-8859-4' for the Content-Type header value.", errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Respond_415_If_Content_Type_Header_Is_JsonApi_Media_Type_With_Unknown()
        {
            // Arrange
            var builder = new WebHostBuilder().UseStartup<Startup>();
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, route) {Content = new StringContent(string.Empty)};
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(HeaderConstants.MediaType + "; unknown=unexpected");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The specified Content-Type header value is not supported.", errorDocument.Errors[0].Title);
            Assert.Equal("Please specify 'application/vnd.api+json' instead of 'application/vnd.api+json; unknown=unexpected' for the Content-Type header value.", errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Respond_200_If_Accept_Headers_Are_Missing()
        {
            // Arrange
            var builder = new WebHostBuilder().UseStartup<Startup>();
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Respond_200_If_Accept_Headers_Include_Any()
        {
            // Arrange
            var builder = new WebHostBuilder().UseStartup<Startup>();
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html"));
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("*/*"));
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Respond_200_If_Accept_Headers_Include_Application_Prefix()
        {
            // Arrange
            var builder = new WebHostBuilder().UseStartup<Startup>();
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html"));
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/*"));
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Respond_200_If_Accept_Headers_Contain_JsonApi_Media_Type()
        {
            // Arrange
            var builder = new WebHostBuilder().UseStartup<Startup>();
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html"));
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; profile=some"));
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; ext=other"));
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; unknown=unexpected"));
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType));
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Respond_406_If_Accept_Headers_Only_Contain_JsonApi_Media_Type_With_Parameters()
        {
            // Arrange
            var builder = new WebHostBuilder().UseStartup<Startup>();
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; profile=some"));
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; ext=other"));
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; unknown=unexpected"));
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; charset=ISO-8859-4"));
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.NotAcceptable, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The specified Accept header value does not contain any supported media types.", errorDocument.Errors[0].Title);
            Assert.Equal("Please include 'application/vnd.api+json' in the Accept header values.", errorDocument.Errors[0].Detail);
        }
    }
}
