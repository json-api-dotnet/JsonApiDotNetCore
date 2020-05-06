using System.Net;
using System.Net.Http;
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
    public sealed class QueryParameterTests
    {
        [Fact]
        public async Task Server_Returns_400_ForUnknownQueryParam()
        {
            // Arrange
            const string queryString = "?someKey=someValue";

            var builder = new WebHostBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/todoItems" + queryString);

            // Act
            var response = await client.SendAsync(request);
            
            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Unknown query string parameter.", errorDocument.Errors[0].Title);
            Assert.Equal("Query string parameter 'someKey' is unknown. Set 'AllowCustomQueryStringParameters' to 'true' in options to ignore unknown parameters.", errorDocument.Errors[0].Detail);
            Assert.Equal("someKey", errorDocument.Errors[0].Source.Parameter);
        }

        [Fact]
        public async Task Server_Returns_400_ForMissingQueryParameterValue()
        {
            // Arrange
            const string queryString = "?include=";

            var builder = new WebHostBuilder().UseStartup<TestStartup>();
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems" + queryString;
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Missing query string parameter value.", errorDocument.Errors[0].Title);
            Assert.Equal("Missing value for 'include' query string parameter.", errorDocument.Errors[0].Detail);
            Assert.Equal("include", errorDocument.Errors[0].Source.Parameter);
        }

        [Fact]
        public async Task Server_Returns_400_ForUnknownQueryParameter_Attribute()
        {
            // Arrange
            const string queryString = "?sort=notSoGood";

            var builder = new WebHostBuilder().UseStartup<TestStartup>();
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems" + queryString;
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The attribute requested in query string does not exist.", errorDocument.Errors[0].Title);
            Assert.Equal("The attribute 'notSoGood' does not exist on resource 'todoItems'.", errorDocument.Errors[0].Detail);
            Assert.Equal("sort", errorDocument.Errors[0].Source.Parameter);
        }

        [Fact]
        public async Task Server_Returns_400_ForUnknownQueryParameter_RelatedAttribute()
        {
            // Arrange
            const string queryString = "?sort=notSoGood.evenWorse";

            var builder = new WebHostBuilder().UseStartup<TestStartup>();
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems" + queryString;
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The relationship requested in query string does not exist.", errorDocument.Errors[0].Title);
            Assert.Equal("The relationship 'notSoGood' does not exist on resource 'todoItems'.", errorDocument.Errors[0].Detail);
            Assert.Equal("sort", errorDocument.Errors[0].Source.Parameter);
        }

        [Theory]
        [InlineData("filter[ordinal]=1")]
        [InlineData("fields=ordinal")]
        [InlineData("sort=ordinal")]
        [InlineData("page[number]=1")]
        [InlineData("page[size]=10")]
        public async Task Server_Returns_400_ForQueryParamOnNestedResource(string queryParameter)
        {
            string parameterName = queryParameter.Split('=')[0];

            var builder = new WebHostBuilder().UseStartup<TestStartup>();
            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/people/1/assignedTodoItems?{queryParameter}";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The specified query string parameter is currently not supported on nested resource endpoints.", errorDocument.Errors[0].Title);
            Assert.Equal($"Query string parameter '{parameterName}' is currently not supported on nested resource endpoints. (i.e. of the form '/article/1/author?parameterName=...')", errorDocument.Errors[0].Detail);
            Assert.Equal(parameterName, errorDocument.Errors[0].Source.Parameter);
        }
    }
}
