using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Extensibility
{
    [Collection("WebHostCollection")]
    public class CustomControllerTests
    {
        private TestFixture<Startup> _fixture;
        private Faker<TodoItem> _todoItemFaker;
        private Faker<Person> _personFaker;

        public CustomControllerTests(TestFixture<Startup> fixture)
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
        public async Task NonJsonApiControllers_DoNotUse_Dasherized_Routes()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var route = $"testValues";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            
            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CustomRouteControllers_Uses_Dasherized_Collection_Route()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var route = $"/custom/route/todo-items";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CustomRouteControllers_Uses_Dasherized_Item_Route()
        {
            // Arrange
            var context = _fixture.GetService<AppDbContext>();
            var todoItem = _todoItemFaker.Generate();
            var person = _personFaker.Generate();
            todoItem.Owner = person;
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var route = $"/custom/route/todo-items/{todoItem.Id}";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CustomRouteControllers_Creates_Proper_Relationship_Links()
        {
            // Arrange
            var context = _fixture.GetService<AppDbContext>();
            var todoItem = _todoItemFaker.Generate();
            var person = _personFaker.Generate();
            todoItem.Owner = person;
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var route = $"/custom/route/todo-items/{todoItem.Id}";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act & assert
            var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = JsonConvert.DeserializeObject<JObject>(body);

            var result =  deserializedBody["data"]["relationships"]["owner"]["links"]["related"].ToString();
            Assert.EndsWith($"{route}/owner", deserializedBody["data"]["relationships"]["owner"]["links"]["related"].ToString());
        }
    }
}
