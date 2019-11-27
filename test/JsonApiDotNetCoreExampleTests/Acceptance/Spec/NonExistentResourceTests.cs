using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class NonExistentResourceTests
    {
        private TestFixture<Startup> _fixture;
        private Faker<TodoItem> _todoItemFaker;
        private readonly Faker<Person> _personFaker;

        public NonExistentResourceTests(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
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
        public async Task Resource_UserNonExistent_ShouldReturn404WithCorrectError()
        {
            // Arrange
            var context = _fixture.GetService<AppDbContext>();
            context.TodoItems.RemoveRange(context.TodoItems.ToList());
            var todoItem = _todoItemFaker.Generate();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();
            var nonExistentId = todoItem.Id;
            context.TodoItems.Remove(todoItem);
            context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todoItems/{nonExistentId}";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            var errorResult = JsonConvert.DeserializeObject<ErrorMessage>(body);
            var title = errorResult.Errors.First().Title;
            Assert.Contains(title, "todoitem");
            Assert.Contains(title, "found");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
