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
using System;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec.DocumentTests
{
    [Collection("WebHostCollection")]
    public class Included
    {
        private DocsFixture<Startup, JsonDocWriter> _fixture;
        private AppDbContext _context;
        private Faker<Person> _personFaker;
        private Faker<TodoItem> _todoItemFaker;

        public Included(DocsFixture<Startup, JsonDocWriter> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
            _personFaker = new Faker<Person>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName());

            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number());
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
    }
}
