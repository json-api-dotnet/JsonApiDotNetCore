using JsonApiDotNetCore.Serialization;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NoEntityFrameworkExample;
using System.Net.Http;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
using System.Threading.Tasks;
using System.Net;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExample.Data;
using System;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace NoEntityFrameworkTests.Acceptance.Extensibility
{
    public class NoEntityFrameworkTests
    {
        private readonly TestServer _server;
        private readonly AppDbContext _context;

        public NoEntityFrameworkTests()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            _server = new TestServer(builder);
            _context = _server.GetService<AppDbContext>();
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task Can_Get_TodoItems()
        {
            // arrange
            _context.TodoItems.Add(new TodoItem());
            _context.SaveChanges();

            var client = _server.CreateClient();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/custom-todo-items";

            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _server.GetService<IJsonApiDeSerializer>()
                .DeserializeList<TodoItem>(responseBody);

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
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var client = _server.CreateClient();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/custom-todo-items/{todoItem.Id}";

            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = (TodoItem)_server.GetService<IJsonApiDeSerializer>()
                .Deserialize(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.Equal(todoItem.Id, deserializedBody.Id);
        }

        [Fact]
        public async Task Can_Create_TodoItems()
        {
            // arrange
            var description = Guid.NewGuid().ToString();
            var client = _server.CreateClient();
            var httpMethod = new HttpMethod("POST");
            var route = $"/api/v1/custom-todo-items/";
            var content = new
            {
                data = new
                {
                    type = "custom-todo-items",
                    attributes = new
                    {
                        description = description,
                        ordinal = 1
                    }
                }
            };

            var request = new HttpRequestMessage(httpMethod, route);
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = (TodoItem)_server.GetService<IJsonApiDeSerializer>()
                .Deserialize(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.Equal(description, deserializedBody.Description);
        }
    }
}
