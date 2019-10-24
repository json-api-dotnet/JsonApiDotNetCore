using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using StringExtensions = JsonApiDotNetCoreExampleTests.Helpers.Extensions.StringExtensions;
using Person = JsonApiDotNetCoreExample.Models.Person;
using System.Net;
using JsonApiDotNetCore.Serialization.Client;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCoreExampleTests.Helpers.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.Internal.Contracts;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class SparseFieldSetTests
    {
        private readonly AppDbContext _dbContext;
        private readonly IResourceGraph _resourceGraph;
        private readonly Faker<Person> _personFaker;
        private readonly Faker<TodoItem> _todoItemFaker;

        public SparseFieldSetTests(TestFixture<TestStartup> fixture)
        {
            _dbContext = fixture.GetService<AppDbContext>();
            _resourceGraph = fixture.GetService<IResourceGraph>();
            _personFaker = new Faker<Person>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName())
                .RuleFor(p => p.Age, f => f.Random.Int(20, 80));

            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number(1, 10))
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());
        }

        [Fact]
        public async Task Can_Select_Sparse_Fieldsets()
        {
            // Arrange
            var fields = new List<string> { "Id", "Description", "CreatedDate", "AchievedDate" };
            var todoItem = new TodoItem
            {
                Description = "description",
                Ordinal = 1,
                CreatedDate = DateTime.Now,
                AchievedDate = DateTime.Now.AddDays(2)
            };
            _dbContext.TodoItems.Add(todoItem);
            await _dbContext.SaveChangesAsync();
            var expectedSql = StringExtensions.Normalize($@"SELECT 't'.'Id', 't'.'Description', 't'.'CreatedDate', 't'.'AchievedDate'
                                FROM 'TodoItems' AS 't'
                                WHERE 't'.'Id' = {todoItem.Id}");

            // Act
            var query = _dbContext
                .TodoItems
                .Where(t => t.Id == todoItem.Id)
                .Select(_resourceGraph.GetAttributes<TodoItem>(e => new { e.Id, e.Description, e.CreatedDate, e.AchievedDate }));

            var result = await query.FirstAsync();

            // Assert
            Assert.Equal(0, result.Ordinal);
            Assert.Equal(todoItem.Description, result.Description);
            Assert.Equal(todoItem.CreatedDate.ToString("G"), result.CreatedDate.ToString("G"));
            Assert.Equal(todoItem.AchievedDate.GetValueOrDefault().ToString("G"), result.AchievedDate.GetValueOrDefault().ToString("G"));
        }

        [Fact]
        public async Task Fields_Query_Selects_Sparse_Field_Sets()
        {
            // Arrange
            var todoItem = new TodoItem
            {
                Description = "description",
                Ordinal = 1,
                CreatedDate = DateTime.Now
            };
            _dbContext.TodoItems.Add(todoItem);
            await _dbContext.SaveChangesAsync();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            using var server = new TestServer(builder);
            var client = server.CreateClient();

            var route = $"/api/v1/todo-items/{todoItem.Id}?fields=description,created-date";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            // Assert
            Assert.Equal(todoItem.StringId, deserializeBody.SingleData.Id);
            Assert.Equal(2, deserializeBody.SingleData.Attributes.Count);
            Assert.Equal(todoItem.Description, deserializeBody.SingleData.Attributes["description"]);
            Assert.Equal(todoItem.CreatedDate.ToString("G"), ((DateTime)deserializeBody.SingleData.Attributes["created-date"]).ToString("G"));
        }

        [Fact]
        public async Task Fields_Query_Selects_Sparse_Field_Sets_With_Type_As_Navigation()
        {
            // Arrange
            var todoItem = new TodoItem
            {
                Description = "description",
                Ordinal = 1,
                CreatedDate = DateTime.Now
            };
            _dbContext.TodoItems.Add(todoItem);
            await _dbContext.SaveChangesAsync();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            using var server = new TestServer(builder);
            var client = server.CreateClient();
            var route = $"/api/v1/todo-items/{todoItem.Id}?fields[todo-items]=description,created-date";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("relationships only", body);

        }

        [Fact]
        public async Task Fields_Query_Selects_All_Fieldset_With_HasOne()
        {
            // Arrange
            _dbContext.TodoItems.RemoveRange(_dbContext.TodoItems);
            _dbContext.SaveChanges();
            var owner = _personFaker.Generate();
            var todoItem = new TodoItem
            {
                Description = "s",
                Ordinal = 123,
                CreatedDate = DateTime.Now,
                Owner = owner
            };
            _dbContext.TodoItems.Add(todoItem);
            _dbContext.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            using var server = new TestServer(builder);
            var client = server.CreateClient();

            var route = $"/api/v1/todo-items?include=owner&fields[owner]=first-name,age";
            var request = new HttpRequestMessage(httpMethod, route);
            var resourceGraph = new ResourceGraphBuilder().AddResource<Person>().AddResource<TodoItemClient>("todo-items").Build();
            var deserializer = new ResponseDeserializer(resourceGraph);
            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();

            var deserializedTodoItems = deserializer.DeserializeList<TodoItemClient>(body).Data;

            foreach (var item in deserializedTodoItems.Where(i => i.Owner != null))
            {
                Assert.Null(item.Owner.LastName);
                Assert.NotNull(item.Owner.FirstName);
                Assert.NotEqual(0, item.Owner.Age);
            }
        }

        [Fact]
        public async Task Fields_Query_Selects_Fieldset_With_HasOne()
        {
            // Arrange
            var owner = _personFaker.Generate();
            var todoItem = new TodoItem
            {
                Description = "description",
                Ordinal = 1,
                CreatedDate = DateTime.Now,
                Owner = owner
            };
            _dbContext.TodoItems.Add(todoItem);
            _dbContext.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            using var server = new TestServer(builder);
            var client = server.CreateClient();

            var route = $"/api/v1/todo-items/{todoItem.Id}?include=owner&fields[owner]=first-name,age";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert - check statusc ode
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            // Assert - check owner attributes
            var included = deserializeBody.Included.First();
            Assert.Equal(owner.StringId, included.Id);
            Assert.Equal(owner.FirstName, included.Attributes["first-name"]);
            Assert.Equal((long)owner.Age, included.Attributes["age"]);
            Assert.DoesNotContain("last-name", included.Attributes.Keys);
        }

        [Fact]
        public async Task Fields_Query_Selects_Fieldset_With_HasMany()
        {
            // Arrange
            var owner = _personFaker.Generate();
            var todoItems = _todoItemFaker.Generate(2);

            owner.TodoItems = todoItems;

            _dbContext.People.Add(owner);
            _dbContext.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            using var server = new TestServer(builder);
            var client = server.CreateClient();

            var route = $"/api/v1/people/{owner.Id}?include=todo-items&fields[todo-items]=description";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            // check owner attributes
            foreach (var includedItem in deserializeBody.Included)
            {
                var todoItem = todoItems.FirstOrDefault(i => i.StringId == includedItem.Id);
                Assert.NotNull(todoItem);
                Assert.Equal(todoItem.Description, includedItem.Attributes["description"]);
                Assert.DoesNotContain("ordinal", includedItem.Attributes.Keys);
                Assert.DoesNotContain("created-date", includedItem.Attributes.Keys);
            }
        }
    }
}
