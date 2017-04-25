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
using Person = JsonApiDotNetCoreExample.Models.Person;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Data;
using Bogus;
using JsonApiDotNetCoreExample.Models;
using System.Linq;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec.DocumentTests
{
    [Collection("WebHostCollection")]
    public class Included
    {
        private DocsFixture<Startup, JsonDocWriter> _fixture;
        private AppDbContext _context;
        private Faker<Person> _personFaker;
        private Faker<TodoItem> _todoItemFaker;
        private Faker<TodoItemCollection> _todoItemCollectionFaker;

        public Included(DocsFixture<Startup, JsonDocWriter> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
            _personFaker = new Faker<Person>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName());

            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
				.RuleFor(t => t.CreatedDate, f => f.Date.Past());

            _todoItemCollectionFaker = new Faker<TodoItemCollection>()
                .RuleFor(t => t.Name, f => f.Company.CatchPhrase());
        }

        [Fact]
        public async Task GET_Included_Contains_SideloadedData_ForManyToOne()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?include=owner";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var documents = JsonConvert.DeserializeObject<Documents>(await response.Content.ReadAsStringAsync());
            var data = documents.Data[0];

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(documents.Included);
            Assert.Equal(documents.Data.Count, documents.Included.Count);
        }

        [Fact]
        public async Task GET_ById_Included_Contains_SideloadedData_ForManyToOne()
        {
            // arrange
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");

            var route = $"/api/v1/todo-items/{todoItem.Id}?include=owner";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseString);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(document.Included);
            Assert.Equal(person.Id.ToString(), document.Included[0].Id);
            Assert.Equal(person.FirstName, document.Included[0].Attributes["first-name"]);
            Assert.Equal(person.LastName, document.Included[0].Attributes["last-name"]);
        }

        [Fact]
        public async Task GET_Included_Contains_SideloadedData_OneToMany()
        {
            // arrange
            _context.People.RemoveRange(_context.People); // ensure all people have todo-items
            _context.TodoItems.RemoveRange(_context.TodoItems);
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/people?include=todo-items";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var documents = JsonConvert.DeserializeObject<Documents>(await response.Content.ReadAsStringAsync());
            var data = documents.Data[0];

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(documents.Included);
            Assert.Equal(documents.Data.Count, documents.Included.Count);
        }

        [Fact]
        public async Task GET_Included_DoesNot_Duplicate_Records_ForMultipleRelationshipsOfSameType()
        {
            // arrange
            _context.People.RemoveRange(_context.People); // ensure all people have todo-items
            _context.TodoItems.RemoveRange(_context.TodoItems);
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            todoItem.Assignee = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items/{todoItem.Id}?include=owner&include=assignee";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var documents = JsonConvert.DeserializeObject<Document>(await response.Content.ReadAsStringAsync());
            var data = documents.Data;

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(documents.Included);
            Assert.Equal(1, documents.Included.Count);
        }

        [Fact]
        public async Task GET_ById_Included_Contains_SideloadedData_ForOneToMany()
        {
            // arrange
            const int numberOfTodoItems = 5;
            var person = _personFaker.Generate();
            for (var i = 0; i < numberOfTodoItems; i++)
            {
                var todoItem = _todoItemFaker.Generate();
                todoItem.Owner = person;
                _context.TodoItems.Add(todoItem);
                _context.SaveChanges();
            }

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");

            var route = $"/api/v1/people/{person.Id}?include=todo-items";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseString);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(document.Included);
            Assert.Equal(numberOfTodoItems, document.Included.Count);
        }

        [Fact]
        public async Task Can_Include_MultipleRelationships()
        {
            // arrange
            var person = _personFaker.Generate();
            var todoItemCollection = _todoItemCollectionFaker.Generate();
            todoItemCollection.Owner = person;

            const int numberOfTodoItems = 5;
            for (var i = 0; i < numberOfTodoItems; i++)
            {
                var todoItem = _todoItemFaker.Generate();
                todoItem.Owner = person;
                todoItem.Collection = todoItemCollection;
                _context.TodoItems.Add(todoItem);
                _context.SaveChanges();
            }

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");

            var route = $"/api/v1/people/{person.Id}?include=todo-items,todo-item-collections";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseString);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(document.Included);
            Assert.Equal(numberOfTodoItems + 1, document.Included.Count);
        }

        [Fact]
        public async Task Request_ToIncludeUnknownRelationship_Returns_400()
        {
            // arrange
            var person = _context.People.First();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");

            var route = $"/api/v1/people/{person.Id}?include=non-existent-relationship";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Request_ToIncludeDeeplyNestedRelationships_Returns_400()
        {
            // arrange
            var person = _context.People.First();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");

            var route = $"/api/v1/people/{person.Id}?include=owner.name";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
