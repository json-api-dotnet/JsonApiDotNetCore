using System.Collections.Generic;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using DotNetCoreDocs;
using DotNetCoreDocs.Models;
using DotNetCoreDocs.Writers;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    public class PagingTests : TestFixture<Startup>
    {
        private readonly Faker<TodoItem> _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());

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

            var route = $"/api/v1/todo-items?page[size]={expectedEntitiesPerPage}";

            // Act
            var response = await Client.GetAsync(route);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = GetService<IJsonApiDeSerializer>().DeserializeList<TodoItem>(body);

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
            var todoItems = _todoItemFaker.Generate(totalCount);

            foreach (var todoItem in todoItems)
                todoItem.Owner = person;

            Context.TodoItems.RemoveRange(Context.TodoItems);
            Context.TodoItems.AddRange(todoItems);
            Context.SaveChanges();

            var route = $"/api/v1/todo-items?page[size]={expectedEntitiesPerPage}&page[number]=1";

            // Act
            var response = await Client.GetAsync(route);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = GetService<IJsonApiDeSerializer>().DeserializeList<TodoItem>(body);

            Assert.NotEmpty(deserializedBody);
            Assert.Equal(expectedEntitiesPerPage, deserializedBody.Count);

            var expectedTodoItems = Context.TodoItems.Take(2);
            foreach (var todoItem in expectedTodoItems)
                Assert.NotNull(deserializedBody.SingleOrDefault(t => t.Id == todoItem.Id));
        }

        [Fact]
        public async Task Can_Paginate_TodoItems_From_End()
        {
            // Arrange
            const int expectedEntitiesPerPage = 2;
            var totalCount = expectedEntitiesPerPage * 2;
            var person = new Person();
            var todoItems = _todoItemFaker.Generate(totalCount);

            foreach (var todoItem in todoItems)
                todoItem.Owner = person;

            Context.TodoItems.RemoveRange(Context.TodoItems);
            Context.TodoItems.AddRange(todoItems);
            Context.SaveChanges();

            var route = $"/api/v1/todo-items?page[size]={expectedEntitiesPerPage}&page[number]=-1";

            // Act
            var response = await Client.GetAsync(route);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = GetService<IJsonApiDeSerializer>().DeserializeList<TodoItem>(body);

            Assert.NotEmpty(deserializedBody);
            Assert.Equal(expectedEntitiesPerPage, deserializedBody.Count);

            var expectedTodoItems = Context.TodoItems
                .OrderByDescending(t => t.Id)
                .Take(2)
                .ToList()
                .OrderBy(t => t.Id)
                .ToList();

            for (int i = 0; i < expectedEntitiesPerPage; i++)
                Assert.Equal(expectedTodoItems[i].Id, deserializedBody[i].Id);
        }
    }
}