using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class UpdatingDataTests
    {
        private TestFixture<TestStartup> _fixture;
        private AppDbContext _context;
        private Faker<TodoItem> _todoItemFaker;
        private Faker<Person> _personFaker;

        public UpdatingDataTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();

            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());
            _personFaker = new Faker<Person>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName());
        }


        [Fact]
        public async Task Response400IfUpdatingNotSettableAttribute()
        {
            // Arrange
            var builder = new WebHostBuilder().UseStartup<Startup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();

            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var serializer = _fixture.GetSerializer<TodoItem>(ti => new { ti.CalculatedValue });
            var content = serializer.Serialize(todoItem);
            var request = PrepareRequest("PATCH", $"/api/v1/todo-items/{todoItem.Id}", content);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(422, Convert.ToInt32(response.StatusCode));
        }

        [Fact]
        public async Task Respond_404_If_EntityDoesNotExist()
        {
            // Arrange
            var maxPersonId = _context.TodoItems.LastOrDefault()?.Id ?? 0;
            var todoItem = _todoItemFaker.Generate();
            todoItem.Id = maxPersonId + 100;
            todoItem.CreatedDate = DateTime.Now;
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var serializer = _fixture.GetSerializer<TodoItem>(ti => new { ti.Description, ti.Ordinal, ti.CreatedDate });
            var content = serializer.Serialize(todoItem);
            var request = PrepareRequest("PATCH", $"/api/v1/todo-items/{maxPersonId + 100}", content);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Respond_400_If_IdNotInAttributeList()
        {
            // Arrange
            var maxPersonId = _context.TodoItems.LastOrDefault()?.Id ?? 0;
            var todoItem = _todoItemFaker.Generate();
            todoItem.CreatedDate = DateTime.Now;
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var serializer = _fixture.GetSerializer<TodoItem>(ti => new { ti.Description, ti.Ordinal, ti.CreatedDate });
            var content = serializer.Serialize(todoItem);
            var request = PrepareRequest("PATCH", $"/api/v1/todo-items/{maxPersonId}", content);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(422, Convert.ToInt32(response.StatusCode));

        }

        [Fact]
        public async Task Can_Patch_Entity()
        {
            // arrange
            _context.RemoveRange(_context.TodoItemCollections);
            _context.RemoveRange(_context.TodoItems);
            _context.RemoveRange(_context.People);
            _context.SaveChanges();

            var todoItem = _todoItemFaker.Generate();
            var person = _personFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var newTodoItem = _todoItemFaker.Generate();
            newTodoItem.Id = todoItem.Id;
            var builder = new WebHostBuilder().UseStartup<Startup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var serializer = _fixture.GetSerializer<TodoItem>(p => new { p.Description, p.Ordinal });

            var request = PrepareRequest("PATCH", $"/api/v1/todo-items/{todoItem.Id}", serializer.Serialize(newTodoItem));

            // Act
            var response = await client.SendAsync(request);

            // Assert -- response
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(body);
            Assert.NotNull(document);
            Assert.NotNull(document.Data);
            Assert.NotNull(document.SingleData.Attributes);
            Assert.Equal(newTodoItem.Description, document.SingleData.Attributes["description"]);
            Assert.Equal(newTodoItem.Ordinal, (long)document.SingleData.Attributes["ordinal"]);
            Assert.True(document.SingleData.Relationships.ContainsKey("owner"));
            Assert.Null(document.SingleData.Relationships["owner"].SingleData);

            // Assert -- database
            var updatedTodoItem = _context.TodoItems.AsNoTracking()
                .Include(t => t.Owner)
                .SingleOrDefault(t => t.Id == todoItem.Id);
            Assert.Equal(person.Id, todoItem.OwnerId);
            Assert.Equal(newTodoItem.Description, updatedTodoItem.Description);
            Assert.Equal(newTodoItem.Ordinal, updatedTodoItem.Ordinal);
        }

        [Fact]
        public async Task Patch_Entity_With_HasMany_Does_Not_Included_Relationships()
        {
            // arrange
            var todoItem = _todoItemFaker.Generate();
            var person = _personFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var newPerson = _personFaker.Generate();
            newPerson.Id = person.Id;
            var builder = new WebHostBuilder().UseStartup<Startup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var serializer = _fixture.GetSerializer<Person>(p => new { p.LastName, p.FirstName });

            var request = PrepareRequest("PATCH", $"/api/v1/people/{person.Id}", serializer.Serialize(newPerson));

            // Act
            var response = await client.SendAsync(request);

            // Assert -- response
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(body);
            Console.WriteLine(body);
            Assert.NotNull(document);
            Assert.NotNull(document.Data);
            Assert.NotNull(document.SingleData.Attributes);
            Assert.Equal(newPerson.LastName, document.SingleData.Attributes["last-name"]);
            Assert.Equal(newPerson.FirstName, document.SingleData.Attributes["first-name"]);
            Assert.True(document.SingleData.Relationships.ContainsKey("todo-items"));
            Assert.Null(document.SingleData.Relationships["todo-items"].Data);
        }

        [Fact]
        public async Task Can_Patch_Entity_And_HasOne_Relationships()
        {
            // arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.CreatedDate = DateTime.Now;
            var person = _personFaker.Generate();
            _context.TodoItems.Add(todoItem);
            _context.People.Add(person);
            _context.SaveChanges();
            todoItem.Owner = person;

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var serializer = _fixture.GetSerializer<TodoItem>(ti => new { ti.Description, ti.Ordinal, ti.CreatedDate }, ti => new { ti.Owner });
            var content = serializer.Serialize(todoItem);
            var request = PrepareRequest("PATCH", $"/api/v1/todo-items/{todoItem.Id}", content);

            // Act
            var response = await client.SendAsync(request);
            var updatedTodoItem = _context.TodoItems.AsNoTracking()
                .Include(t => t.Owner)
                .SingleOrDefault(t => t.Id == todoItem.Id);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(person.Id, updatedTodoItem.OwnerId);
        }

        private HttpRequestMessage PrepareRequest(string method, string route, string content)
        {
            var httpMethod = new HttpMethod(method);
            var request = new HttpRequestMessage(httpMethod, route);

            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");
            return request;
        }
    }
}
