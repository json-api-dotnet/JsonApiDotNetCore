using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class AttributeFilterTests
    {
        private TestFixture<TestStartup> _fixture;
        private Faker<TodoItem> _todoItemFaker;
        private readonly Faker<Person> _personFaker;

        public AttributeFilterTests(TestFixture<TestStartup> fixture)
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

        [Fact]
        public async Task Can_Filter_On_Guid_Properties()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var todoItem = _todoItemFaker.Generate();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?filter[guid-property]={todoItem.GuidProperty}";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture
                .GetService<IJsonApiDeSerializer>()
                .DeserializeList<TodoItem>(body);

            var todoItemResponse = deserializedBody.Single();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(todoItem.Id, todoItemResponse.Id);
            Assert.Equal(todoItem.GuidProperty, todoItemResponse.GuidProperty);
        }

        [Fact]
        public async Task Can_Filter_On_Related_Attrs()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var person = _personFaker.Generate();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?include=owner&filter[owner.first-name]={person.FirstName}";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Documents>(await response.Content.ReadAsStringAsync());
            var included = documents.Included;

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(included);
            Assert.NotEmpty(included);
            foreach (var item in included)
                Assert.Equal(person.FirstName, item.Attributes["first-name"]);
        }

        [Fact]
        public async Task Cannot_Filter_If_Explicitly_Forbidden()
        {
            // arrange
            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?include=owner&filter[achieved-date]={DateTime.UtcNow.Date}";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await _fixture.Client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_Filter_On_Not_Equal_Values()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var todoItem = _todoItemFaker.Generate();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var totalCount = context.TodoItems.Count();
            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?page[size]={totalCount}&filter[ordinal]=ne:{todoItem.Ordinal}";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedTodoItems = _fixture
                .GetService<IJsonApiDeSerializer>()
                .DeserializeList<TodoItem>(body);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(deserializedTodoItems.Any(i => i.Ordinal == todoItem.Ordinal));
        }
    }
}
