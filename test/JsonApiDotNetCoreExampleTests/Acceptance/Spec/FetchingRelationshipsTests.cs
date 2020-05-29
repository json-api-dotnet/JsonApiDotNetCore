using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public sealed class FetchingRelationshipsTests
    {
        private readonly TestFixture<TestStartup> _fixture;
        private readonly Faker<TodoItem> _todoItemFaker;

        public FetchingRelationshipsTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());
        }

        [Fact]
        public async Task When_getting_existing_ToOne_relationship_it_should_succeed()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = new Person();

            var context = _fixture.GetService<AppDbContext>();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var route = $"/api/v1/todoItems/{todoItem.Id}/relationships/owner";

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = JsonConvert.DeserializeObject<JObject>(body).ToString();

            Assert.Equal(@"{
  ""links"": {
    ""self"": ""http://localhost/api/v1/todoItems/" + todoItem.StringId + @"/relationships/owner"",
    ""related"": ""http://localhost/api/v1/todoItems/" + todoItem.StringId + @"/owner""
  },
  ""data"": {
    ""type"": ""people"",
    ""id"": """ + todoItem.Owner.StringId + @"""
  }
}", json);
        }

        [Fact]
        public async Task When_getting_existing_ToMany_relationship_it_should_succeed()
        {
            // Arrange
            var author = new Author
            {
                LastName = "X",
                Articles = new List<Article>
                {
                    new Article
                    {
                        Caption = "Y"
                    },
                    new Article
                    {
                        Caption = "Z"
                    }
                }
            };

            var context = _fixture.GetService<AppDbContext>();
            context.AuthorDifferentDbContextName.Add(author);
            await context.SaveChangesAsync();

            var route = $"/api/v1/authors/{author.Id}/relationships/articles";

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = JsonConvert.DeserializeObject<JObject>(body).ToString();

            Assert.Equal(@"{
  ""links"": {
    ""self"": ""http://localhost/api/v1/authors/" + author.StringId + @"/relationships/articles"",
    ""related"": ""http://localhost/api/v1/authors/" + author.StringId + @"/articles""
  },
  ""data"": [
    {
      ""type"": ""articles"",
      ""id"": """ + author.Articles[0].StringId + @"""
    },
    {
      ""type"": ""articles"",
      ""id"": """ + author.Articles[1].StringId + @"""
    }
  ]
}", json);
        }

        [Fact]
        public async Task When_getting_related_missing_to_one_resource_it_should_succeed_with_null_data()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = null;

            var context = _fixture.GetService<AppDbContext>();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var route = $"/api/v1/todoItems/{todoItem.StringId}/owner";

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = JsonConvert.DeserializeObject<JObject>(body).ToString();
            Assert.Equal(@"{
  ""meta"": {
    ""copyright"": ""Copyright 2015 Example Corp."",
    ""authors"": [
      ""Jared Nance"",
      ""Maurits Moeys"",
      ""Harro van der Kroft""
    ]
  },
  ""links"": {
    ""self"": ""http://localhost/api/v1/todoItems/" + todoItem.StringId + @"/owner""
  },
  ""data"": null
}", json);
        }

        [Fact]
        public async Task When_getting_relationship_for_missing_to_one_resource_it_should_succeed_with_null_data()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = null;

            var context = _fixture.GetService<AppDbContext>();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var route = $"/api/v1/todoItems/{todoItem.Id}/relationships/owner";

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = JsonConvert.DeserializeObject<Document>(body);
            Assert.False(doc.IsManyData);
            Assert.Null(doc.Data);
        }

        [Fact]
        public async Task When_getting_related_missing_to_many_resource_it_should_succeed_with_null_data()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.ChildrenTodos = new List<TodoItem>();

            var context = _fixture.GetService<AppDbContext>();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var route = $"/api/v1/todoItems/{todoItem.Id}/childrenTodos";

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = JsonConvert.DeserializeObject<Document>(body);
            Assert.True(doc.IsManyData);
            Assert.Empty(doc.ManyData);
        }

        [Fact]
        public async Task When_getting_relationship_for_missing_to_many_resource_it_should_succeed_with_null_data()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.ChildrenTodos = new List<TodoItem>();

            var context = _fixture.GetService<AppDbContext>();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var route = $"/api/v1/todoItems/{todoItem.Id}/relationships/childrenTodos";

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = JsonConvert.DeserializeObject<Document>(body);
            Assert.True(doc.IsManyData);
            Assert.Empty(doc.ManyData);
        }

        [Fact]
        public async Task When_getting_related_for_missing_parent_resource_it_should_fail()
        {
            // Arrange
            var route = "/api/v1/todoItems/99999999/owner";

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.NotFound, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The requested resource does not exist.", errorDocument.Errors[0].Title);
            Assert.Equal("Resource of type 'todoItems' with id '99999999' does not exist.",errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task When_getting_relationship_for_missing_parent_resource_it_should_fail()
        {
            // Arrange
            var route = "/api/v1/todoItems/99999999/relationships/owner";

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, route);

            // Act
            var response = await client.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.NotFound, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The requested resource does not exist.", errorDocument.Errors[0].Title);
            Assert.Equal("Resource of type 'todoItems' with id '99999999' does not exist.",errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task When_getting_unknown_related_resource_it_should_fail()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();

            var context = _fixture.GetService<AppDbContext>();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var route = $"/api/v1/todoItems/{todoItem.Id}/invalid";

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, route);
            
            // Act
            var response = await client.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.NotFound, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The requested relationship does not exist.", errorDocument.Errors[0].Title);
            Assert.Equal("The resource 'todoItems' does not contain a relationship named 'invalid'.",errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task When_getting_unknown_relationship_for_resource_it_should_fail()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();

            var context = _fixture.GetService<AppDbContext>();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var route = $"/api/v1/todoItems/{todoItem.Id}/relationships/invalid";

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, route);
            
            // Act
            var response = await client.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.NotFound, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The requested relationship does not exist.", errorDocument.Errors[0].Title);
            Assert.Equal("The resource 'todoItems' does not contain a relationship named 'invalid'.",errorDocument.Errors[0].Detail);
        }
    }
}
