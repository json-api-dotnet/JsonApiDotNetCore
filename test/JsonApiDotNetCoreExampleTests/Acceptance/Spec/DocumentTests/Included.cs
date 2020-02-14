using System;
using System.Linq;
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

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec.DocumentTests
{
    [Collection("WebHostCollection")]
    public class Included
    {
        private readonly AppDbContext _context;
        private readonly Faker<Person> _personFaker;
        private readonly Faker<TodoItem> _todoItemFaker;
        private readonly Faker<TodoItemCollection> _todoItemCollectionFaker;

        public Included(TestFixture<Startup> fixture)
        {
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
        public async Task GET_Included_Contains_SideLoadedData_ForManyToOne()
        {
            // Arrange
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.RemoveRange(_context.TodoItems);
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var builder = new WebHostBuilder().UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todoItems?include=owner";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var json = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Document>(json);
            // we only care about counting the todoItems that have owners
            var expectedCount = documents.ManyData.Count(d => d.Relationships["owner"].SingleData != null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(documents.Included);
            Assert.Equal(expectedCount, documents.Included.Count);

            server.Dispose();
            request.Dispose();
            response.Dispose();
        }

        [Fact]
        public async Task GET_ById_Included_Contains_SideLoadedData_ForManyToOne()
        {
            // Arrange
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");

            var route = $"/api/v1/todoItems/{todoItem.Id}?include=owner";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(document.Included);
            Assert.Equal(person.Id.ToString(), document.Included[0].Id);
            Assert.Equal(person.FirstName, document.Included[0].Attributes["firstName"]);
            Assert.Equal(person.LastName, document.Included[0].Attributes["lastName"]);

            server.Dispose();
            request.Dispose();
            response.Dispose();
        }

        [Fact]
        public async Task GET_Included_Contains_SideLoadedData_OneToMany()
        {
            // Arrange
            _context.People.RemoveRange(_context.People); // ensure all people have todoItems
            _context.TodoItems.RemoveRange(_context.TodoItems);
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/people?include=todoItems";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var documents = JsonConvert.DeserializeObject<Document>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(documents.Included);
            Assert.Equal(documents.ManyData.Count, documents.Included.Count);

            server.Dispose();
            request.Dispose();
            response.Dispose();
        }

        [Fact]
        public async Task GET_Included_DoesNot_Duplicate_Records_ForMultipleRelationshipsOfSameType()
        {
            // Arrange
            _context.RemoveRange(_context.TodoItems);
            _context.RemoveRange(_context.TodoItemCollections);
            _context.RemoveRange(_context.People); // ensure all people have todoItems
            _context.SaveChanges();
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            todoItem.Assignee = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todoItems/{todoItem.Id}?include=owner&include=assignee";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var documents = JsonConvert.DeserializeObject<Document>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(documents.Included);
            Assert.Single(documents.Included);

            server.Dispose();
            request.Dispose();
            response.Dispose();
        }

        [Fact]
        public async Task GET_Included_DoesNot_Duplicate_Records_If_HasOne_Exists_Twice()
        {
            // Arrange
            _context.TodoItemCollections.RemoveRange(_context.TodoItemCollections);
            _context.People.RemoveRange(_context.People); // ensure all people have todoItems
            _context.TodoItems.RemoveRange(_context.TodoItems);
            var person = _personFaker.Generate();
            var todoItem1 = _todoItemFaker.Generate();
            var todoItem2 = _todoItemFaker.Generate();
            todoItem1.Owner = person;
            todoItem2.Owner = person;
            _context.TodoItems.AddRange(new[] { todoItem1, todoItem2 });
            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todoItems?include=owner";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var documents = JsonConvert.DeserializeObject<Document>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(documents.Included);
            Assert.Single(documents.Included);

            server.Dispose();
            request.Dispose();
            response.Dispose();
        }

        [Fact]
        public async Task GET_ById_Included_Contains_SideloadeData_ForOneToMany()
        {
            // Arrange
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

            var route = $"/api/v1/people/{person.Id}?include=todoItems";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(document.Included);
            Assert.Equal(numberOfTodoItems, document.Included.Count);

            server.Dispose();
            request.Dispose();
            response.Dispose();
        }

        [Fact]
        public async Task Can_Include_MultipleRelationships()
        {
            // Arrange
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

            var route = $"/api/v1/people/{person.Id}?include=todoItems,todoCollections";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(document.Included);
            Assert.Equal(numberOfTodoItems + 1, document.Included.Count);

            server.Dispose();
            request.Dispose();
            response.Dispose();
        }

        [Fact]
        public async Task Request_ToIncludeUnknownRelationship_Returns_400()
        {
            // Arrange
            var person = _context.People.First();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");

            var route = $"/api/v1/people/{person.Id}?include=nonExistentRelationship";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            server.Dispose();
            request.Dispose();
            response.Dispose();
        }

        [Fact]
        public async Task Request_ToIncludeDeeplyNestedRelationships_Returns_400()
        {
            // Arrange
            var person = _context.People.First();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");

            var route = $"/api/v1/people/{person.Id}?include=owner.name";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            server.Dispose();
            request.Dispose();
            response.Dispose();
        }

        [Fact]
        public async Task Request_ToIncludeRelationshipMarkedCanIncludeFalse_Returns_400()
        {
            // Arrange
            var person = _context.People.First();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");

            var route = $"/api/v1/people/{person.Id}?include=unincludeableItem";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            server.Dispose();
            request.Dispose();
            response.Dispose();
        }

        [Fact]
        public async Task Can_Ignore_Null_Parent_In_Nested_Include()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = _personFaker.Generate();
            todoItem.CreatedDate = DateTime.Now;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var todoItemWithNullOwner = _todoItemFaker.Generate();
            todoItemWithNullOwner.Owner = null;
            todoItemWithNullOwner.CreatedDate = DateTime.Now;
            _context.TodoItems.Add(todoItemWithNullOwner);
            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");
          
            var route = $"/api/v1/todoItems?sort=-createdDate&page[size]=2&include=owner.role"; // last two todoItems

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Document>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(documents.Included);

            var ownerValueNull = documents.ManyData
                .First(i => i.Id == todoItemWithNullOwner.StringId)
                .Relationships.First(i => i.Key == "owner")
                .Value.SingleData;

            Assert.Null(ownerValueNull);

            var ownerValue = documents.ManyData
                .First(i => i.Id == todoItem.StringId)
                .Relationships.First(i => i.Key == "owner")
                .Value.SingleData;

            Assert.NotNull(ownerValue);

            server.Dispose();
            request.Dispose();
            response.Dispose();
        }
    }
}
