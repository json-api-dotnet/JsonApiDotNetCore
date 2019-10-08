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

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class SparseFieldSetTests
    {
        private TestFixture<TestStartup> _fixture;
        private readonly AppDbContext _dbContext;
        private Faker<Person> _personFaker;
        private Faker<TodoItem> _todoItemFaker;

        public SparseFieldSetTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
            _dbContext = fixture.GetService<AppDbContext>();
            _personFaker = new Faker<Person>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName())
                .RuleFor(p => p.Age, f => f.Random.Int(20, 80));

            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number(1,10))
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());
        }

        [Fact]
        public async Task Can_Select_Sparse_Fieldsets()
        {
            // arrange
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

            // act
            var query = _dbContext
                .TodoItems
                .Where(t => t.Id == todoItem.Id)
                .Select(fields);

            var resultSql = StringExtensions.Normalize(query.ToSql());
            var result = await query.FirstAsync();

            // assert
            Assert.Equal(0, result.Ordinal);
            Assert.Equal(todoItem.Description, result.Description);
            Assert.Equal(todoItem.CreatedDate.ToString("G"), result.CreatedDate.ToString("G"));
            Assert.Equal(todoItem.AchievedDate.GetValueOrDefault().ToString("G"), result.AchievedDate.GetValueOrDefault().ToString("G"));
            Assert.Equal(expectedSql, resultSql);
        }

        [Fact]
        public async Task Fields_Query_Selects_Sparse_Field_Sets()
        {
            // arrange
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
            var server = new TestServer(builder);
            var client = server.CreateClient();

            var route = $"/api/v1/todo-items/{todoItem.Id}?fields[todo-items]=description,created-date";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            // assert
            Assert.Equal(todoItem.StringId, deserializeBody.SingleData.Id);
            Assert.Equal(2, deserializeBody.SingleData.Attributes.Count);
            Assert.Equal(todoItem.Description, deserializeBody.SingleData.Attributes["description"]);
            Assert.Equal(todoItem.CreatedDate.ToString("G"), ((DateTime)deserializeBody.SingleData.Attributes["created-date"]).ToString("G"));
        }

        [Fact]
        public async Task Fields_Query_Selects_All_Fieldset_With_HasOne()
        {
            // arrange
            var owner = _personFaker.Generate();
            var ordinal = _dbContext.TodoItems.Count();
            var todoItem = new TodoItem
            {
                Description = "s",
                Ordinal = ordinal,
                CreatedDate = DateTime.Now,
                Owner = owner
            };
            _dbContext.TodoItems.Add(todoItem);
            _dbContext.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var server = new TestServer(builder);
            var client = server.CreateClient();

            var route = $"/api/v1/todo-items?include=owner&fields[owner]=first-name,age";
            var request = new HttpRequestMessage(httpMethod, route);
            var graph = new ResourceGraphBuilder().AddResource<Person>().AddResource<TodoItemClient>("todo-items").Build();
            var deserializer = new ResponseDeserializer(graph);
            // act
            var response = await client.SendAsync(request);

            // assert
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
            // arrange
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
            var server = new TestServer(builder);
            var client = server.CreateClient();

            var route = $"/api/v1/todo-items/{todoItem.Id}?include=owner&fields[owner]=first-name,age";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            // check owner attributes
            var included = deserializeBody.Included.First();
            Assert.Equal(owner.StringId, included.Id);      
            Assert.Equal(owner.FirstName, included.Attributes["first-name"]);
            Assert.Equal((long)owner.Age, included.Attributes["age"]);
            Assert.DoesNotContain("last-name", included.Attributes.Keys);
        }

        [Fact]
        public async Task Fields_Query_Selects_Fieldset_With_HasMany()
        {
            // arrange
            var owner = _personFaker.Generate();
            var todoItems = _todoItemFaker.Generate(2);

            owner.TodoItems = todoItems;

            _dbContext.People.Add(owner);
            _dbContext.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var server = new TestServer(builder);
            var client = server.CreateClient();

            var route = $"/api/v1/people/{owner.Id}?include=todo-items&fields[todo-items]=description";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);

            // assert
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
