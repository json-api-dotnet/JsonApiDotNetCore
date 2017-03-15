using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using DotNetCoreDocs;
using DotNetCoreDocs.Writers;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class CreatingDataTests
    {
        private DocsFixture<Startup, JsonDocWriter> _fixture;
        private IJsonApiContext _jsonApiContext;
        private Faker<TodoItem> _todoItemFaker;

        public CreatingDataTests(DocsFixture<Startup, JsonDocWriter> fixture)
        {
            _fixture = fixture;
            _jsonApiContext = fixture.GetService<IJsonApiContext>();
             _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number());
        }

        [Fact]
        public async Task Request_With_ClientGeneratedId_Returns_403()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("POST");
            var route = "/api/v1/todo-items";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            var todoItem = _todoItemFaker.Generate();
            var content = new
            {
                data = new
                {
                    type = "todo-items",
                    id = "9999",
                    attributes = new
                    {
                        description = todoItem.Description,
                        ordinal = todoItem.Ordinal
                    }
                }
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");
            
            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReceiveLocationHeader_InResponse()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("POST");
            var route = "/api/v1/todo-items";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            var todoItem = _todoItemFaker.Generate();
            var content = new
            {
                data = new
                {
                    type = "todo-items",
                    attributes = new
                    {
                        description = todoItem.Description,
                        ordinal = todoItem.Ordinal
                    }
                }
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");
            
            // act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = (TodoItem)JsonApiDeSerializer.Deserialize(body, _jsonApiContext);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"/api/v1/todo-items/{deserializedBody.Id}", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task Respond_409_ToIncorrectEntityType()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("POST");
            var route = "/api/v1/todo-items";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            var todoItem = _todoItemFaker.Generate();
            var content = new
            {
                data = new
                {
                    type = "people",
                    attributes = new
                    {
                        description = todoItem.Description,
                        ordinal = todoItem.Ordinal
                    }
                }
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");
            
            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
    }
}
