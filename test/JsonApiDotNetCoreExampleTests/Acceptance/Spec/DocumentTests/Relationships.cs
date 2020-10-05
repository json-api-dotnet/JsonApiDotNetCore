using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec.DocumentTests
{
    [Collection("WebHostCollection")]
    public sealed class Relationships
    {
        private readonly AppDbContext _context;
        private readonly Faker<TodoItem> _todoItemFaker;
        private readonly Faker<Person> _personFaker;

        public Relationships(TestFixture<TestStartup> fixture)
        {
            _context = fixture.GetRequiredService<AppDbContext>();
             _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());
             _personFaker = new Faker<Person>()
                 .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                 .RuleFor(p => p.LastName, f => f.Name.LastName());
        }

        [Fact]
        public async Task Correct_RelationshipObjects_For_ManyToOne_Relationships()
        {
            // Arrange
            await _context.ClearTableAsync<TodoItem>();
            await _context.SaveChangesAsync();

            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems";

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Document>(responseString).ManyData[0];
            var expectedOwnerSelfLink = $"http://localhost/api/v1/todoItems/{todoItem.Id}/relationships/owner";
            var expectedOwnerRelatedLink = $"http://localhost/api/v1/todoItems/{todoItem.Id}/owner";

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedOwnerSelfLink, data.Relationships["owner"].Links.Self);
            Assert.Equal(expectedOwnerRelatedLink, data.Relationships["owner"].Links.Related);
        }

        [Fact]
        public async Task Correct_RelationshipObjects_For_ManyToOne_Relationships_ById()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todoItems/{todoItem.Id}";

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Document>(responseString).SingleData;
            var expectedOwnerSelfLink = $"http://localhost/api/v1/todoItems/{todoItem.Id}/relationships/owner";
            var expectedOwnerRelatedLink = $"http://localhost/api/v1/todoItems/{todoItem.Id}/owner";

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedOwnerSelfLink, data.Relationships["owner"].Links.Self);
            Assert.Equal(expectedOwnerRelatedLink, data.Relationships["owner"].Links.Related);
        }

        [Fact]
        public async Task Correct_RelationshipObjects_For_OneToMany_Relationships()
        {
            // Arrange
            await _context.ClearTableAsync<Person>();
            await _context.SaveChangesAsync();

            var person = _personFaker.Generate();
            _context.People.Add(person);
            await _context.SaveChangesAsync();

            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/people";

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Document>(responseString).ManyData[0];
            var expectedOwnerSelfLink = $"http://localhost/api/v1/people/{person.Id}/relationships/todoItems";
            var expectedOwnerRelatedLink = $"http://localhost/api/v1/people/{person.Id}/todoItems";

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedOwnerSelfLink, data.Relationships["todoItems"].Links.Self);
            Assert.Equal(expectedOwnerRelatedLink, data.Relationships["todoItems"].Links.Related);
        }

        [Fact]
        public async Task Correct_RelationshipObjects_For_OneToMany_Relationships_ById()
        {
            // Arrange
            var person = _personFaker.Generate();
            _context.People.Add(person);
            await _context.SaveChangesAsync();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/people/{person.Id}";

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Document>(responseString).SingleData;
            var expectedOwnerSelfLink = $"http://localhost/api/v1/people/{person.Id}/relationships/todoItems";
            var expectedOwnerRelatedLink = $"http://localhost/api/v1/people/{person.Id}/todoItems";

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedOwnerSelfLink, data.Relationships["todoItems"].Links.Self);
            Assert.Equal(expectedOwnerRelatedLink, data.Relationships["todoItems"].Links.Related);
        }
    }
}
