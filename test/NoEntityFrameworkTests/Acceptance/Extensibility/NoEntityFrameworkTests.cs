using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCoreExample.Models;
using Newtonsoft.Json;
using Xunit;

namespace NoEntityFrameworkTests.Acceptance.Extensibility
{
    public class NoEntityFrameworkTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public NoEntityFrameworkTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Get_TodoItems()
        {
            // arrange
            _fixture.Context.TodoItems.Add(new TodoItem());
            _fixture.Context.SaveChanges();

            var client = _fixture.Server.CreateClient();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/custom-todo-items";

            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<TodoItem>(responseBody).Data;

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.NotEmpty(deserializedBody);
        }

        [Fact]
        public async Task Can_Get_TodoItems_By_Id()
        {
            // arrange
            var todoItem = new TodoItem();
            _fixture.Context.TodoItems.Add(todoItem);
            _fixture.Context.SaveChanges();

            var client = _fixture.Server.CreateClient();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/custom-todo-items/{todoItem.Id}";

            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeSingle<TodoItem>(responseBody).Data;

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.Equal(todoItem.Id, deserializedBody.Id);
        }

        [Fact]
        public async Task Can_Create_TodoItems()
        {
            // Arrange
            var description = Guid.NewGuid().ToString();
            var client = _fixture.Server.CreateClient();
            var httpMethod = new HttpMethod("POST");
            var route = $"/api/v1/custom-todo-items/";
            var content = new
            {
                data = new
                {
                    type = "custom-todo-items",
                    attributes = new
                    {
                        description,
                        ordinal = 1
                    }
                }
            };

            var request = new HttpRequestMessage(httpMethod, route);
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

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
