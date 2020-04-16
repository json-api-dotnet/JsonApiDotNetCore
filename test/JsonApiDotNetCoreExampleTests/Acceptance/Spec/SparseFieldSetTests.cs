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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;
using System.Net;
using JsonApiDotNetCore.Serialization.Client;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCoreExampleTests.Helpers.Models;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public sealed class SparseFieldSetTests
    {
        private readonly TestFixture<Startup> _fixture;
        private readonly AppDbContext _dbContext;
        private readonly IResourceGraph _resourceGraph;
        private readonly Faker<Person> _personFaker;
        private readonly Faker<TodoItem> _todoItemFaker;

        public SparseFieldSetTests(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
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
            var todoItem = new TodoItem
            {
                Description = "description",
                Ordinal = 1,
                CreatedDate = DateTime.Now,
                AchievedDate = DateTime.Now.AddDays(2)
            };
            _dbContext.TodoItems.Add(todoItem);
            await _dbContext.SaveChangesAsync();

            var properties = _resourceGraph
                .GetAttributes<TodoItem>(e => new {e.Id, e.Description, e.CreatedDate, e.AchievedDate})
                .Select(x => x.PropertyInfo.Name);

            // Act
            var query = _dbContext
                .TodoItems
                .Where(t => t.Id == todoItem.Id)
                .Select(properties);

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

            var route = $"/api/v1/todoItems/{todoItem.Id}?fields=description,createdDate";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            // Assert
            Assert.Equal(todoItem.StringId, deserializeBody.SingleData.Id);
            Assert.Equal(2, deserializeBody.SingleData.Attributes.Count);
            Assert.Equal(todoItem.Description, deserializeBody.SingleData.Attributes["description"]);
            Assert.Equal(todoItem.CreatedDate.ToString("G"), ((DateTime)deserializeBody.SingleData.Attributes["createdDate"]).ToString("G"));
            Assert.DoesNotContain("guidProperty", deserializeBody.SingleData.Attributes.Keys);
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
            var route = $"/api/v1/todoItems/{todoItem.Id}?fields[todoItems]=description,createdDate";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
            Assert.StartsWith("Square bracket notation in 'filter' is now reserved for relationships only", errorDocument.Errors[0].Title);
            Assert.Equal("Use '?fields=...' instead of '?fields[todoItems]=...'.", errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Fields_Query_Selects_All_Fieldset_From_HasOne()
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

            var builder = new WebHostBuilder().UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            using var server = new TestServer(builder);
            var client = server.CreateClient();

            var route = "/api/v1/todoItems?include=owner&fields[owner]=firstName,the-Age";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            Assert.Equal(todoItem.Description, deserializeBody.ManyData[0].Attributes["description"]);
            Assert.Equal(todoItem.Ordinal, deserializeBody.ManyData[0].Attributes["ordinal"]);

            Assert.NotNull(deserializeBody.Included);
            Assert.NotEmpty(deserializeBody.Included);
            Assert.Equal(owner.StringId, deserializeBody.Included[0].Id);
            Assert.Equal(owner.FirstName, deserializeBody.Included[0].Attributes["firstName"]);
            Assert.Equal((long)owner.Age, deserializeBody.Included[0].Attributes["the-Age"]);
            Assert.DoesNotContain("lastName", deserializeBody.Included[0].Attributes.Keys);
        }

        [Fact]
        public async Task Fields_Query_Selects_Fieldset_From_HasOne()
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

            var route = $"/api/v1/todoItems/{todoItem.Id}?include=owner&fields[owner]=firstName,the-Age";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            Assert.Equal(todoItem.Description, deserializeBody.SingleData.Attributes["description"]);
            Assert.Equal(todoItem.Ordinal, deserializeBody.SingleData.Attributes["ordinal"]);

            Assert.Equal(owner.StringId, deserializeBody.Included[0].Id);
            Assert.Equal(owner.FirstName, deserializeBody.Included[0].Attributes["firstName"]);
            Assert.Equal((long)owner.Age, deserializeBody.Included[0].Attributes["the-Age"]);
            Assert.DoesNotContain("lastName", deserializeBody.Included[0].Attributes.Keys);
        }

        [Fact]
        public async Task Fields_Query_Selects_Fieldset_From_Self_And_HasOne()
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

            var route = $"/api/v1/todoItems/{todoItem.Id}?include=owner&fields=ordinal&fields[owner]=firstName,the-Age";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            Assert.Equal(todoItem.Ordinal, deserializeBody.SingleData.Attributes["ordinal"]);
            Assert.DoesNotContain("description", deserializeBody.SingleData.Attributes.Keys);

            Assert.NotNull(deserializeBody.Included);
            Assert.NotEmpty(deserializeBody.Included);
            Assert.Equal(owner.StringId, deserializeBody.Included[0].Id);
            Assert.Equal(owner.FirstName, deserializeBody.Included[0].Attributes["firstName"]);
            Assert.Equal((long)owner.Age, deserializeBody.Included[0].Attributes["the-Age"]);
            Assert.DoesNotContain("lastName", deserializeBody.Included[0].Attributes.Keys);
        }

        [Fact]
        public async Task Fields_Query_Selects_Fieldset_From_Self_With_HasOne_Include()
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

            var route = $"/api/v1/todoItems/{todoItem.Id}?include=owner&fields=ordinal";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            Assert.Equal(todoItem.Ordinal, deserializeBody.SingleData.Attributes["ordinal"]);
            Assert.DoesNotContain("description", deserializeBody.SingleData.Attributes.Keys);

            Assert.NotNull(deserializeBody.Included);
            Assert.NotEmpty(deserializeBody.Included);
            Assert.Equal(owner.StringId, deserializeBody.Included[0].Id);
            Assert.Equal(owner.FirstName, deserializeBody.Included[0].Attributes["firstName"]);
            Assert.Equal((long)owner.Age, deserializeBody.Included[0].Attributes["the-Age"]);
        }

        [Fact]
        public async Task Fields_Query_Selects_Fieldset_From_HasMany()
        {
            // Arrange
            var owner = _personFaker.Generate();
            owner.TodoItems = _todoItemFaker.Generate(2);

            _dbContext.People.Add(owner);
            _dbContext.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            using var server = new TestServer(builder);
            var client = server.CreateClient();

            var route = $"/api/v1/people/{owner.Id}?include=todoItems&fields[todoItems]=description";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            Assert.Equal(owner.FirstName, deserializeBody.SingleData.Attributes["firstName"]);
            Assert.Equal(owner.LastName, deserializeBody.SingleData.Attributes["lastName"]);

            foreach (var include in deserializeBody.Included)
            {
                var todoItem = owner.TodoItems.Single(i => i.StringId == include.Id);

                Assert.Equal(todoItem.Description, include.Attributes["description"]);
                Assert.DoesNotContain("ordinal", include.Attributes.Keys);
                Assert.DoesNotContain("createdDate", include.Attributes.Keys);
            }
        }

        [Fact]
        public async Task Fields_Query_Selects_Fieldset_From_Self_And_HasMany()
        {
            // Arrange
            var owner = _personFaker.Generate();
            owner.TodoItems = _todoItemFaker.Generate(2);

            _dbContext.People.Add(owner);
            _dbContext.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            using var server = new TestServer(builder);
            var client = server.CreateClient();

            var route = $"/api/v1/people/{owner.Id}?include=todoItems&fields=firstName&fields[todoItems]=description";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            Assert.Equal(owner.FirstName, deserializeBody.SingleData.Attributes["firstName"]);
            Assert.DoesNotContain("lastName", deserializeBody.SingleData.Attributes.Keys);

            // check owner attributes
            Assert.NotNull(deserializeBody.Included);
            Assert.Equal(2, deserializeBody.Included.Count);
            foreach (var includedItem in deserializeBody.Included)
            {
                var todoItem = owner.TodoItems.FirstOrDefault(i => i.StringId == includedItem.Id);
                Assert.NotNull(todoItem);
                Assert.Equal(todoItem.Description, includedItem.Attributes["description"]);
                Assert.DoesNotContain("ordinal", includedItem.Attributes.Keys);
                Assert.DoesNotContain("createdDate", includedItem.Attributes.Keys);
            }
        }

        [Fact]
        public async Task Fields_Query_Selects_Fieldset_From_Self_With_HasMany_Include()
        {
            // Arrange
            var owner = _personFaker.Generate();
            owner.TodoItems = _todoItemFaker.Generate(2);

            _dbContext.People.Add(owner);
            _dbContext.SaveChanges();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            using var server = new TestServer(builder);
            var client = server.CreateClient();

            var route = $"/api/v1/people/{owner.Id}?include=todoItems&fields=firstName";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            Assert.Equal(owner.FirstName, deserializeBody.SingleData.Attributes["firstName"]);
            Assert.DoesNotContain("lastName", deserializeBody.SingleData.Attributes.Keys);

            Assert.NotNull(deserializeBody.Included);
            Assert.Equal(2, deserializeBody.Included.Count);
            foreach (var includedItem in deserializeBody.Included)
            {
                var todoItem = owner.TodoItems.Single(i => i.StringId == includedItem.Id);
                Assert.Equal(todoItem.Description, includedItem.Attributes["description"]);
            }
        }
    }
}
