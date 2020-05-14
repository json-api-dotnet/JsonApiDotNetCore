using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using NoEntityFrameworkExample;
using NoEntityFrameworkExample.Models;
using Xunit;

namespace NoEntityFrameworkTests.Acceptance.Extensibility
{
    public sealed class NoEntityFrameworkTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public NoEntityFrameworkTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Get_TodoItems()
        {
            // Arrange
            _fixture.Context.TodoItems.Add(new TodoItem());
            _fixture.Context.SaveChanges();

            var client = _fixture.Server.CreateClient();

            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems";

            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<TodoItem>(responseBody).Data;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.NotEmpty(deserializedBody);
        }

        [Fact]
        public async Task Can_Get_TodoItems_By_Id()
        {
            // Arrange
            var todoItem = new TodoItem();
            _fixture.Context.TodoItems.Add(todoItem);
            _fixture.Context.SaveChanges();

            var client = _fixture.Server.CreateClient();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todoItems/{todoItem.Id}";

            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeSingle<TodoItem>(responseBody).Data;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.Equal(todoItem.Id, deserializedBody.Id);
        }

        [Fact]
        public async Task Can_Create_TodoItems()
        {
            // Arrange
            var description = Guid.NewGuid().ToString();
            var httpMethod = new HttpMethod("POST");
            var route = "/api/v1/todoItems/";
            var content = new
            {
                data = new
                {
                    type = "todoItems",
                    attributes = new
                    {
                        description,
                        ordinal = 1
                    }
                }
            };

            var request = new HttpRequestMessage(httpMethod, route)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content))
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();

            // Act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeSingle<TodoItem>(responseBody).Data;

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.Equal(description, deserializedBody.Description);
        }
    }
}
