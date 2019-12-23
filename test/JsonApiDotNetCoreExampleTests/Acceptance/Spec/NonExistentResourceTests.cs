using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization.Client;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    public class NonExistentResourceTests : FunctionalTestCollection<StandardApplicationFactory>
    {
        private StandardApplicationFactory _factory;
        private Faker<TodoItem> _todoItemFaker;
        private readonly Faker<Person> _personFaker;

        public NonExistentResourceTests(StandardApplicationFactory factory) : base(factory)
        {
            _factory = factory;
            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());

            _personFaker = new Faker<Person>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName());
        }

        public class ErrorInnerMessage
        {
            [JsonProperty("title")]
            public string Title;
            [JsonProperty("status")]
            public string Status;
        }
        public class ErrorMessage
        {
            [JsonProperty("errors")]
            public List<ErrorInnerMessage> Errors;
        }
        [Fact]
        public async Task Resource_PersonNonExistent_ShouldReturn404WithCorrectError()
        {
            // Arrange
            var context = _factory.GetService<AppDbContext>();
            var person = _personFaker.Generate();
            context.People.Add(person);
            await context.SaveChangesAsync();
            var nonExistingId = person.Id;
            context.People.Remove(person);
            context.SaveChanges();

            var route = $"/api/v1/people/{nonExistingId}";

            // Act
            var response = (await Get(route)).Response;
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            var errorResult = JsonConvert.DeserializeObject<ErrorMessage>(body);
            var errorParsed = errorResult.Errors.First();
            var title = errorParsed.Title;
            var code = errorParsed.Status;
            Assert.Contains("found", title);
            Assert.Contains("people", title);
            Assert.Contains(nonExistingId.ToString(), title);
            Assert.Equal("404", code);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        [Fact]
        public async Task ResourceRelatedHasOne_TodoItemExistentToOneRelationshipIsNonExistent_ShouldReturn200WithNullData()
        {
            // Arrange
            var context = _factory.GetService<AppDbContext>();
            context.TodoItems.RemoveRange(context.TodoItems.ToList());
            var todoItem = _todoItemFaker.Generate();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();
            var existingId = todoItem.Id;
            var deserializer = new ResponseDeserializer(_factory.GetService<IResourceGraph>());

            var route = $"/api/v1/todoItems/{existingId}/oneToOnePerson";

            // Act
            var response = (await Get(route)).Response;
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            var document = deserializer.DeserializeList<Person>(body);
            Assert.Null(document.Data);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ResourceRelatedHasMany_TodoItemExistsToManyRelationshipHasNoData_ShouldReturn200WithNullData()
        {
            // Arrange
            var context = _factory.GetService<AppDbContext>();
            context.TodoItems.RemoveRange(context.TodoItems.ToList());
            var todoItem = _todoItemFaker.Generate();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();
            var existingId = todoItem.Id;

            var httpMethod = HttpMethod.Get;
            var route = $"/api/v1/todoItems/{existingId}/stakeHolders";
            var request = new HttpRequestMessage(httpMethod, route);

            var deserializer = new ResponseDeserializer(_factory.GetService<IResourceGraph>());

            // Act
            var response = await _factory.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            var parsed = deserializer.DeserializeList<TodoItem>(body);
            Assert.Null(parsed.Data);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
