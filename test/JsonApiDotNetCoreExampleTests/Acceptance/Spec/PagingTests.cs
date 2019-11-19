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

        [Fact]
        public async Task Can_Paginate_TodoItems()
        {
            // Arrange
            const int expectedEntitiesPerPage = 2;
            var totalCount = expectedEntitiesPerPage * 2;
            var person = new Person();
            var todoItems = _todoItemFaker.Generate(totalCount);

            foreach (var todoItem in todoItems)
                todoItem.Owner = person;

            Context.TodoItems.AddRange(todoItems);
            Context.SaveChanges();

            var route = $"/api/v1/todoItems?page[size]={expectedEntitiesPerPage}";

            // Act
            var response = await Client.GetAsync(route);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;

            Assert.NotEmpty(deserializedBody);
            Assert.Equal(expectedEntitiesPerPage, deserializedBody.Count);
        }

        [Fact]
        public async Task Can_Paginate_TodoItems_From_Start()
        {
            // Arrange
            const int expectedEntitiesPerPage = 2;
            var totalCount = expectedEntitiesPerPage * 2;
            var person = new Person();
            var todoItems = _todoItemFaker.Generate(totalCount).ToList();

            foreach (var todoItem in todoItems)
                todoItem.Owner = person;

            Context.TodoItems.RemoveRange(Context.TodoItems);
            Context.TodoItems.AddRange(todoItems);
            Context.SaveChanges();

            var route = $"/api/v1/todoItems?page[size]={expectedEntitiesPerPage}&page[number]=1";

            // Act
            var response = await Client.GetAsync(route);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;

            var expectedTodoItems = new[] { todoItems[0], todoItems[1] };
            Assert.Equal(expectedTodoItems, deserializedBody, new IdComparer<TodoItem>());
        }


        [Fact]
        public async Task Pagination_FirstPage_DisplaysCorrectLinks()
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

            var route = $"/api/v1/todoItems";

            // Act
            var response = await Client.GetAsync(route);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var links = JsonConvert.DeserializeObject<Document>(body).Links;
            Assert.EndsWith("/api/v1/todoItems?page[size]=5&page[number]=1", links.Self);
            Assert.Null(links.First);
            Assert.Null(links.Prev);
            Assert.EndsWith("/api/v1/todoItems?page[size]=5&page[number]=2", links.Next);
            Assert.EndsWith("/api/v1/todoItems?page[size]=5&page[number]=4", links.Last);
        }

        [Fact]
        public async Task Pagination_SecondPage_DisplaysCorrectLinks()
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

            var route = $"/api/v1/todoItems?page[size]=5&page[number]=2";

            // Act
            var response = await Client.GetAsync(route);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var links = JsonConvert.DeserializeObject<Document>(body).Links;
            Assert.EndsWith("/api/v1/todoItems?page[size]=5&page[number]=2", links.Self);
            Assert.EndsWith("/api/v1/todoItems?page[size]=5&page[number]=1", links.First);
            Assert.EndsWith("/api/v1/todoItems?page[size]=5&page[number]=1", links.Prev);
            Assert.EndsWith("/api/v1/todoItems?page[size]=5&page[number]=3", links.Next);
            Assert.EndsWith("/api/v1/todoItems?page[size]=5&page[number]=4", links.Last);
        }

        [Fact]
        public async Task Can_Paginate_TodoItems_From_End()
        {
            // Arrange
            const int expectedEntitiesPerPage = 2;
            var totalCount = expectedEntitiesPerPage * 2;
            var person = new Person();
            var todoItems = _todoItemFaker.Generate(totalCount).ToList();
            foreach (var ti in todoItems)
                ti.Owner = person;

            Context.TodoItems.RemoveRange(Context.TodoItems);
            Context.TodoItems.AddRange(todoItems);
            Context.SaveChanges();
            var route = $"/api/v1/todoItems?page[size]={expectedEntitiesPerPage}&page[number]=-1";

            // Act
            var response = await Client.GetAsync(route);

            // Assert
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<TodoItemClient>(body).Data.Select(ti => ti.Id).ToArray();

            var expectedTodoItems = new[] { todoItems[totalCount - 2].Id, todoItems[totalCount - 1].Id };
            for (int i = 0; i < expectedEntitiesPerPage-1 ; i++)
                Assert.Contains(expectedTodoItems[i], deserializedBody);
        }

        private class IdComparer<T> : IEqualityComparer<T>
            where T : IIdentifiable
        {
            public bool Equals(T x, T y) => x?.StringId == y?.StringId;

            public int GetHashCode(T obj) => obj?.StringId?.GetHashCode() ?? 0;
        }
    }
}
