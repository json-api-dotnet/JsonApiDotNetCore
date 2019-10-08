using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models;
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
    public class FetchingDataTests
    {
        private TestFixture<TestStartup> _fixture;
        private Faker<TodoItem> _todoItemFaker;
        private Faker<Person> _personFaker;

        public FetchingDataTests(TestFixture<TestStartup> fixture)
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
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            context.TodoItems.RemoveRange(context.TodoItems);
            await context.SaveChangesAsync();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todo-items";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var result = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;
            var items = result.Data;
            var meta = result.Meta;

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/vnd.api+json", response.Content.Headers.ContentType.ToString());
            Assert.Empty(items);
            Assert.Equal(0, int.Parse(meta["total-records"].ToString()));
            context.Dispose();
        }

        [Fact]
        public async Task Included_Records_Contain_Relationship_Links()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var todoItem = _todoItemFaker.Generate();
            var person = _personFaker.Generate();
            todoItem.Owner = person;
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items/{todoItem.Id}?include=owner";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = JsonConvert.DeserializeObject<Document>(body);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(person.StringId, deserializedBody.Included[0].Id);
            Assert.NotNull(deserializedBody.Included[0].Relationships);
            Assert.Equal($"http://localhost/api/v1/people/{person.Id}/todo-items", deserializedBody.Included[0].Relationships["todo-items"].Links.Related);
            Assert.Equal($"http://localhost/api/v1/people/{person.Id}/relationships/todo-items", deserializedBody.Included[0].Relationships["todo-items"].Links.Self);
            context.Dispose();
        }
    }
}
