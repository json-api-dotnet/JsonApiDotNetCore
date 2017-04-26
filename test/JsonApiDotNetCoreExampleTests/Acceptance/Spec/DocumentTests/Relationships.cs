using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetCoreDocs;
using DotNetCoreDocs.Writers;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Data;
using System.Linq;
using Bogus;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec.DocumentTests
{
    [Collection("WebHostCollection")]
    public class Relationships
    {
        private DocsFixture<Startup, JsonDocWriter> _fixture;
        private AppDbContext _context;
        private Faker<TodoItem> _todoItemFaker;

        public Relationships(DocsFixture<Startup, JsonDocWriter> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
             _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());
        }

        [Fact]
        public async Task Correct_RelationshipObjects_For_ManyToOne_Relationships()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            
            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items/{todoItem.Id}";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var document = JsonConvert.DeserializeObject<Document>(await response.Content.ReadAsStringAsync());
            var data = document.Data;
            var expectedOwnerSelfLink = $"http://localhost/api/v1/todo-items/{data.Id}/relationships/owner";
            var expectedOwnerRelatedLink = $"http://localhost/api/v1/todo-items/{data.Id}/owner";

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedOwnerSelfLink, data.Relationships["owner"].Links.Self);
            Assert.Equal(expectedOwnerRelatedLink, data.Relationships["owner"].Links.Related);
        }

        [Fact]
        public async Task Correct_RelationshipObjects_For_ManyToOne_Relationships_ById()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            
            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items/{todoItem.Id}";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Document>(responseString).Data;
            var expectedOwnerSelfLink = $"http://localhost/api/v1/todo-items/{todoItem.Id}/relationships/owner";
            var expectedOwnerRelatedLink = $"http://localhost/api/v1/todo-items/{todoItem.Id}/owner";

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedOwnerSelfLink, data.Relationships["owner"].Links?.Self);
            Assert.Equal(expectedOwnerRelatedLink, data.Relationships["owner"].Links.Related);
        }

        [Fact]
        public async Task Correct_RelationshipObjects_For_OneToMany_Relationships()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/people";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var documents = JsonConvert.DeserializeObject<Documents>(await response.Content.ReadAsStringAsync());
            var data = documents.Data[0];
            var expectedOwnerSelfLink = $"http://localhost/api/v1/people/{data.Id}/relationships/todo-items";
            var expectedOwnerRelatedLink = $"http://localhost/api/v1/people/{data.Id}/todo-items";

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedOwnerSelfLink, data.Relationships["todo-items"].Links.Self);
            Assert.Equal(expectedOwnerRelatedLink, data.Relationships["todo-items"].Links.Related);
        }

        [Fact]
        public async Task Correct_RelationshipObjects_For_OneToMany_Relationships_ById()
        {
            // arrange
            var personId = _context.People.Last().Id;

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/people/{personId}";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Document>(responseString).Data;
            var expectedOwnerSelfLink = $"http://localhost/api/v1/people/{personId}/relationships/todo-items";
            var expectedOwnerRelatedLink = $"http://localhost/api/v1/people/{personId}/todo-items";

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedOwnerSelfLink, data.Relationships["todo-items"].Links?.Self);
            Assert.Equal(expectedOwnerRelatedLink, data.Relationships["todo-items"].Links.Related);
        }
    }
}
