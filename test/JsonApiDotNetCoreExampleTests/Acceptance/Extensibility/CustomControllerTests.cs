using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Extensibility
{
    [Collection("WebHostCollection")]
    public sealed class CustomControllerTests
    {
        private readonly TestFixture<TestStartup> _fixture;
        private readonly Faker<TodoItem> _todoItemFaker;
        private readonly Faker<Person> _personFaker;

        public CustomControllerTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number());
            _personFaker = new Faker<Person>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName());
        }

        [Fact]
        public async Task CustomRouteControllers_Uses_Dasherized_Collection_Route()
        {
            // Arrange
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();
            var httpMethod = new HttpMethod("GET");
            var route = "/custom/route/todoItems";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CustomRouteControllers_Uses_Dasherized_Item_Route()
        {
            // Arrange
            var context = _fixture.GetRequiredService<AppDbContext>();
            var todoItem = _todoItemFaker.Generate();
            var person = _personFaker.Generate();
            todoItem.Owner = person;
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();
            var httpMethod = new HttpMethod("GET");
            var route = $"/custom/route/todoItems/{todoItem.Id}";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CustomRouteControllers_Creates_Proper_Relationship_Links()
        {
            // Arrange
            var context = _fixture.GetRequiredService<AppDbContext>();
            var todoItem = _todoItemFaker.Generate();
            var person = _personFaker.Generate();
            todoItem.Owner = person;
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var httpMethod = new HttpMethod("GET");
            var route = $"/custom/route/todoItems/{todoItem.Id}";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = JsonConvert.DeserializeObject<JObject>(body);

            var result = deserializedBody["data"]["relationships"]["owner"]["links"]["related"].ToString();
            Assert.EndsWith($"{route}/owner", result);
        }

        [Fact]
        public async Task ApiController_attribute_transforms_NotFound_action_result_without_arguments_into_ProblemDetails()
        {
            // Arrange
            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var route = "/custom/route/todoItems/99999999";

            var requestBody = new
            {
                data = new
                {
                    type = "todoItems",
                    id = "99999999",
                    attributes = new Dictionary<string, object>
                    {
                        ["ordinal"] = 1
                    }
                }
            };

            var content = JsonConvert.SerializeObject(requestBody);

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Patch, route) {Content = new StringContent(content)};
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(responseBody);

            Assert.Single(errorDocument.Errors);
            Assert.Equal("https://tools.ietf.org/html/rfc7231#section-6.5.4", errorDocument.Errors[0].Links.About);
        }
    }
}
