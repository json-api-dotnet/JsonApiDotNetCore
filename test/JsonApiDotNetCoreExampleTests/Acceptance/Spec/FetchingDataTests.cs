using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public sealed class FetchingDataTests
    {
        private readonly TestFixture<Startup> _fixture;
        private readonly Faker<TodoItem> _todoItemFaker;
        private readonly Faker<Person> _personFaker;

        public FetchingDataTests(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());
            _personFaker = new Faker<Person>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName());
        }

        [Fact]
        public async Task Request_ForEmptyCollection_Returns_EmptyDataCollection()
        {
            // Arrange
            var context = _fixture.GetService<AppDbContext>();
            context.TodoItems.RemoveRange(context.TodoItems);
            await context.SaveChangesAsync();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var result = _fixture.GetDeserializer().DeserializeList<TodoItem>(body);
            var items = result.Data;
            var meta = result.Meta;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/vnd.api+json", response.Content.Headers.ContentType.ToString());
            Assert.Empty(items);
            Assert.Equal(0, int.Parse(meta["total-records"].ToString()));
            context.Dispose();
        }

        [Fact]
        public async Task Included_Records_Contain_Relationship_Links()
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
            var route = $"/api/v1/todoItems/{todoItem.Id}?include=owner";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = JsonConvert.DeserializeObject<Document>(body);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(person.StringId, deserializedBody.Included[0].Id);
            Assert.NotNull(deserializedBody.Included[0].Relationships);
            Assert.Equal($"http://localhost/api/v1/people/{person.Id}/todoItems", deserializedBody.Included[0].Relationships["todoItems"].Links.Related);
            Assert.Equal($"http://localhost/api/v1/people/{person.Id}/relationships/todoItems", deserializedBody.Included[0].Relationships["todoItems"].Links.Self);
            context.Dispose();
        }

        [Fact]
        public async Task GetResources_NoDefaultPageSize_ReturnsResources()
        {
            // Arrange
            var context = _fixture.GetService<AppDbContext>();
            var todoItems = _todoItemFaker.Generate(20).ToList();
            context.TodoItems.AddRange(todoItems);
            await context.SaveChangesAsync();

            var builder = new WebHostBuilder()
                .UseStartup<NoDefaultPageSizeStartup>();
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var result = _fixture.GetDeserializer().DeserializeList<TodoItem>(body);

            // Assert
            Assert.True(result.Data.Count >= 20);
        }

        [Fact]
        public async Task GetSingleResource_ResourceDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var context = _fixture.GetService<AppDbContext>();
            context.TodoItems.RemoveRange(context.TodoItems);
            await context.SaveChangesAsync();

            var builder = new WebHostBuilder()
                .UseStartup<NoDefaultPageSizeStartup>();
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems/123";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.NotFound, errorDocument.Errors[0].Status);
            Assert.Equal("NotFound", errorDocument.Errors[0].Title);
        }
    }
}
