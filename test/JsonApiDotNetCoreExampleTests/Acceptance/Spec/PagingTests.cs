using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Models;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class PagingTests : TestFixture<Startup>
    {
        private TestFixture<Startup> _fixture;
        private readonly Faker<TodoItem> _todoItemFaker;

        public PagingTests(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
            _todoItemFaker = new Faker<TodoItem>()
            .RuleFor(t => t.Description, f => f.Lorem.Sentence())
            .RuleFor(t => t.Ordinal, f => f.Random.Number())
            .RuleFor(t => t.CreatedDate, f => f.Date.Past());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(-1)]
        public async Task Pagination_WithPageSizeAndPageNumber_ReturnsCorrectSubsetOfResources(int pageNum)
        {
            // Arrange
            const int expectedEntitiesPerPage = 2;
            var totalCount = expectedEntitiesPerPage * 2;
            var person = new Person();
            var todoItems = _todoItemFaker.Generate(totalCount).ToList();
            foreach (var todoItem in todoItems)
            {
                todoItem.Owner = person;
            }
            Context.TodoItems.RemoveRange(Context.TodoItems);
            Context.TodoItems.AddRange(todoItems);
            Context.SaveChanges();

            // Act
            var route = $"/api/v1/todoItems?page[size]={expectedEntitiesPerPage}&page[number]={pageNum}";
            var response = await Client.GetAsync(route);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;

            if (pageNum < 0)
            {
                todoItems.Reverse();
            }
            var expectedTodoItems = todoItems.Take(expectedEntitiesPerPage).ToList();
            Assert.Equal(expectedTodoItems, deserializedBody, new IdComparer<TodoItem>());

        }

        [Theory]
        [InlineData(1, 1, 1, null, 2, 4)]
        [InlineData(2, 2, 1, 1, 3, 4)]
        [InlineData(3, 3, 1, 2, 4, 4)]
        [InlineData(4, 4, 1, 3, null, 4)]
        [InlineData(-1, -1, -1, null, -2, -4)]
        [InlineData(-2, -2, -1, -1, -3, -4)]
        [InlineData(-3, -3, -1, -2, -4, -4)]
        [InlineData(-4, -4, -1, -3, null, -4)]
        public async Task Pagination_OnGivenPage_DisplaysCorrectTopLevelLinks(int pageNum, int selfLink, int? firstLink, int? prevLink, int? nextLink, int? lastLink)
        {
            // Arrange
            var totalCount = 20;
            var person = new Person();
            var todoItems = _todoItemFaker.Generate(totalCount).ToList();

            foreach (var todoItem in todoItems)
                todoItem.Owner = person;

            Context.TodoItems.RemoveRange(Context.TodoItems);
            Context.TodoItems.AddRange(todoItems);
            Context.SaveChanges();

            string route = $"/api/v1/todoItems";
            if (pageNum != 1)
            {
                route += $"?page[size]=5&page[number]={pageNum}";
            }
            // Act
            var response = await Client.GetAsync(route);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var links = JsonConvert.DeserializeObject<Document>(body).Links;

            Assert.EndsWith($"/api/v1/todoItems?page[size]=5&page[number]={selfLink}", links.Self);
            if (firstLink.HasValue)
            {
                Assert.EndsWith($"/api/v1/todoItems?page[size]=5&page[number]={firstLink.Value}", links.First);
            }
            else
            {
                Assert.Null(links.First);
            }

            if (prevLink.HasValue)
            {
                Assert.EndsWith($"/api/v1/todoItems?page[size]=5&page[number]={prevLink}", links.Prev);
            }
            else
            {
                Assert.Null(links.Prev);
            }

            if (nextLink.HasValue)
            {
                Assert.EndsWith($"/api/v1/todoItems?page[size]=5&page[number]={nextLink}", links.Next);
            }
            else
            {
                Assert.Null(links.Next);
            }

            if (lastLink.HasValue)
            {
                Assert.EndsWith($"/api/v1/todoItems?page[size]=5&page[number]={lastLink}", links.Last);
            }
            else
            {
                Assert.Null(links.Last);
            }
        }

        private class IdComparer<T> : IEqualityComparer<T>
            where T : IIdentifiable
        {
            public bool Equals(T x, T y) => x?.StringId == y?.StringId;

            public int GetHashCode(T obj) => obj?.StringId?.GetHashCode() ?? 0;
        }
    }
}
