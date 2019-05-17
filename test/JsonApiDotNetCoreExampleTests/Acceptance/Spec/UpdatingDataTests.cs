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

            var content = new
            {
                datea = new
                {
                    type = "todo-items",
                    attributes = new
                    {
                        calculatedAttribute = "lol"
                    }
                }
            };
            var request = PrepareRequest("PATCH", $"/api/v1/todo-items/{todoItem.Id}", content);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(422, Convert.ToInt32(response.StatusCode));
        }

        [Fact]
        public async Task Respond_404_If_EntityDoesNotExist()
        {
            // Arrange
            var maxPersonId = _context.TodoItems.LastOrDefault()?.Id ?? 0;
            var todoItem = _todoItemFaker.Generate();
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = new
                {
                    type = "todo-items",
                    attributes = new
                    {
                        description = todoItem.Description,
                        ordinal = todoItem.Ordinal,
                        createdDate = DateTime.Now
                    }
                }
            };
            var request = PrepareRequest("PATCH", $"/api/v1/todo-items/{maxPersonId + 100}", content);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        [Fact]
        public async Task Can_Patch_Entity()
        {
            // arrange
            var todoItem = _todoItemFaker.Generate();
            var person = _personFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var newTodoItem = _todoItemFaker.Generate();

            var builder = new WebHostBuilder().UseStartup<Startup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = new
                {
                    type = "todo-items",
                    attributes = new
                    {
                        description = newTodoItem.Description,
                        ordinal = newTodoItem.Ordinal
                    }
                }
            };
            var request = PrepareRequest("PATCH", $"/api/v1/todo-items/{todoItem.Id}", content);

            // Act
            var response = await client.SendAsync(request);

            // Assert -- response
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(body);
            Assert.NotNull(document);
            Assert.NotNull(document.Data);
            Assert.NotNull(document.Data.Attributes);
            Assert.Equal(newTodoItem.Description, document.Data.Attributes["description"]);
            Assert.Equal(newTodoItem.Ordinal, (long)document.Data.Attributes["ordinal"]);
            Assert.True(document.Data.Relationships.ContainsKey("owner"));
            Assert.NotNull(document.Data.Relationships["owner"].SingleData);
            Assert.Equal(person.Id.ToString(), document.Data.Relationships["owner"].SingleData.Id);
            Assert.Equal("people", document.Data.Relationships["owner"].SingleData.Type);

            // Assert -- database
            var updatedTodoItem = _context.TodoItems.AsNoTracking()
                .Include(t => t.Owner)
                .SingleOrDefault(t => t.Id == todoItem.Id);

            Assert.Equal(person.Id, updatedTodoItem.OwnerId);
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

            var builder = new WebHostBuilder().UseStartup<Startup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = new
                {
                    type = "people",
                    attributes = new Dictionary<string, object>
                    {
                        { "last-name",  newPerson.LastName },
                        { "first-name",  newPerson.FirstName},
                    }
                }
            };
            var request = PrepareRequest("PATCH", $"/api/v1/people/{person.Id}", content);

            // Act
            var response = await client.SendAsync(request);

            // Assert -- response
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(body);
            Console.WriteLine(body);
            Assert.NotNull(document);
            Assert.NotNull(document.Data);
            Assert.NotNull(document.Data.Attributes);
            Assert.Equal(newPerson.LastName, document.Data.Attributes["last-name"]);
            Assert.Equal(newPerson.FirstName, document.Data.Attributes["first-name"]);
            Assert.True(document.Data.Relationships.ContainsKey("todo-items"));
            Assert.Null(document.Data.Relationships["todo-items"].ManyData);
            Assert.Null(document.Data.Relationships["todo-items"].SingleData);
        }

        [Fact]
        public async Task Can_Patch_Entity_And_HasOne_Relationships()
        {
            // arrange
            var todoItem = _todoItemFaker.Generate();
            var person = _personFaker.Generate();
            _context.TodoItems.Add(todoItem);
            _context.People.Add(person);
            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = new
                {
                    type = "todo-items",
                    attributes = new
                    {
                        description = todoItem.Description,
                        ordinal = todoItem.Ordinal,
                        createdDate = DateTime.Now
                    },
                    relationships = new
                    {
                        owner = new
                        {
                            data = new
                            {
                                type = "people",
                                id = person.Id.ToString()
                            }
                        }
                    }
                }
            };
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

        private HttpRequestMessage PrepareRequest(string method, string route, object content)
        {
            var httpMethod = new HttpMethod(method);
            var request = new HttpRequestMessage(httpMethod, route);

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");
            return request;
        }
    }
}
