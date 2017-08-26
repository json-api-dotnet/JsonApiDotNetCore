using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
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
    public class UpdatingRelationshipsTests
    {
        private TestFixture<Startup> _fixture;
        private AppDbContext _context;
        private Bogus.Faker<Person> _personFaker;
        private Faker<TodoItem> _todoItemFaker;

        public UpdatingRelationshipsTests(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
            _personFaker = new Faker<Person>()
                .RuleFor(t => t.FirstName, f => f.Name.FirstName())
                .RuleFor(t => t.LastName, f => f.Name.LastName());

            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());
        }

        [Fact]
        public async Task Can_Update_ToMany_Relationship_ThroughLink()
        {
            // arrange
            var person = _personFaker.Generate();
            _context.People.Add(person);

            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);

            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = new List<object>
                {
                    new {
                        type = "todo-items",
                        id = $"{todoItem.Id}"
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/people/{person.Id}/relationships/todo-items";
            var request = new HttpRequestMessage(httpMethod, route);

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await client.SendAsync(request);
            var personsTodoItems = _context.People.Include(p => p.TodoItems).Single(p => p.Id == person.Id).TodoItems;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(personsTodoItems);
        }

        [Fact]
        public async Task Can_Update_ToOne_Relationship_ThroughLink()
        {
            // arrange
            var person = _personFaker.Generate();
            _context.People.Add(person);

            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);

            _context.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = new
            {
                data = new
                {
                    type = "person",
                    id = $"{person.Id}"
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todo-items/{todoItem.Id}/relationships/owner";
            var request = new HttpRequestMessage(httpMethod, route);

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await client.SendAsync(request);
            var todoItemsOwner = _context.TodoItems.Include(t => t.Owner).Single(t => t.Id == todoItem.Id);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(todoItemsOwner);
        }
    }
}
