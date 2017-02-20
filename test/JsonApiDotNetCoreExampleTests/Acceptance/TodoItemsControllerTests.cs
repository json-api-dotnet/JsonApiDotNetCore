using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using DotNetCoreDocs;
using DotNetCoreDocs.Models;
using DotNetCoreDocs.Writers;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Person = JsonApiDotNetCoreExample.Models.Person;
using Newtonsoft.Json;
using Xunit;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.Serialization;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    [Collection("WebHostCollection")]
    public class TodoItemControllerTests
    {
        private DocsFixture<Startup, JsonDocWriter> _fixture;
        private AppDbContext _context;
        private IJsonApiContext _jsonApiContext;
        private Faker<TodoItem> _todoItemFaker;

        public TodoItemControllerTests(DocsFixture<Startup, JsonDocWriter> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
            _jsonApiContext = fixture.GetService<IJsonApiContext>();
            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number());
        }

        [Fact]
        public async Task Can_Get_TodoItems()
        {
            // Arrange
            const int expectedEntitiesPerPage = 5;
            var person = new Person();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todo-items";

            var description = new RequestProperties("Get TodoItems");

            // Act
            var response = await _fixture.MakeRequest<TodoItem>(description, httpMethod, route);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = JsonApiDeSerializer.DeserializeList<TodoItem>(body, _jsonApiContext);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(deserializedBody);
            Assert.True(deserializedBody.Count <= expectedEntitiesPerPage);
        }

        [Fact]
        public async Task Can_Paginate_TodoItems()
        {
            // Arrange
            const int expectedEntitiesPerPage = 2;
            var person = new Person();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?page[size]={expectedEntitiesPerPage}";

            var description = new RequestProperties("Paginate TodoItems", new Dictionary<string, string> {
                { "?page[size]=", "Number of entities per page" }
            });

            // Act
            var response = await _fixture.MakeRequest<TodoItem>(description, httpMethod, route);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = JsonApiDeSerializer.DeserializeList<TodoItem>(body, _jsonApiContext);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(deserializedBody);
            Assert.True(deserializedBody.Count <= expectedEntitiesPerPage);
        }

        [Fact]
        public async Task Can_Filter_TodoItems()
        {
            // Arrange
            var person = new Person();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Ordinal = 999999;
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?filter[ordinal]={todoItem.Ordinal}";

            var description = new RequestProperties("Filter TodoItems By Attribute", new Dictionary<string, string> {
                { "?filter[...]=", "Filter on attribute" }
            });

            // Act
            var response = await _fixture.MakeRequest<TodoItem>(description, httpMethod, route);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = JsonApiDeSerializer.DeserializeList<TodoItem>(body, _jsonApiContext);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(deserializedBody);

            foreach (var todoItemResult in deserializedBody)
                Assert.Equal(todoItem.Ordinal, todoItemResult.Ordinal);
        }

        [Fact]
        public async Task Can_Sort_TodoItems_By_Ordinal_Ascending()
        {
            // Arrange
            _context.TodoItems.RemoveRange(_context.TodoItems);

            const int numberOfItems = 5;
            var person = new Person();

            for (var i = 1; i < numberOfItems; i++)
            {
                var todoItem = _todoItemFaker.Generate();
                todoItem.Ordinal = i;
                todoItem.Owner = person;
                _context.TodoItems.Add(todoItem);
            }
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?sort=ordinal";

            var description = new RequestProperties("Sort TodoItems Ascending", new Dictionary<string, string> {
                { "?sort=attr", "Sort on attribute" }
            });

            // Act
            var response = await _fixture.MakeRequest<TodoItem>(description, httpMethod, route);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = JsonApiDeSerializer.DeserializeList<TodoItem>(body, _jsonApiContext);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(deserializedBody);

            long priorOrdinal = 0;
            foreach (var todoItemResult in deserializedBody)
            {
                Assert.True(todoItemResult.Ordinal > priorOrdinal);
                priorOrdinal = todoItemResult.Ordinal;
            }                
        }

        [Fact]
        public async Task Can_Sort_TodoItems_By_Ordinal_Descending()
        {
            // Arrange
            _context.TodoItems.RemoveRange(_context.TodoItems);

            const int numberOfItems = 5;
            var person = new Person();

            for (var i = 1; i < numberOfItems; i++)
            {
                var todoItem = _todoItemFaker.Generate();
                todoItem.Ordinal = i;
                todoItem.Owner = person;
                _context.TodoItems.Add(todoItem);
            }
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?sort=-ordinal";

            var description = new RequestProperties("Sort TodoItems Descending", new Dictionary<string, string> {
                { "?sort=-attr", "Sort on attribute" }
            });

            // Act
            var response = await _fixture.MakeRequest<TodoItem>(description, httpMethod, route);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = JsonApiDeSerializer.DeserializeList<TodoItem>(body, _jsonApiContext);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(deserializedBody);

            long priorOrdinal = numberOfItems + 1;
            foreach (var todoItemResult in deserializedBody)
            {
                Assert.True(todoItemResult.Ordinal < priorOrdinal);
                priorOrdinal = todoItemResult.Ordinal;
            }                
        }

        [Fact]
        public async Task Can_Get_TodoItem_ById()
        {
            // Arrange
            var person = new Person();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items/{todoItem.Id}";

            var description = new RequestProperties("Get TodoItem By Id", new Dictionary<string, string> {
                { "/todo-items/{id}", "TodoItem Id" }
            });

            // Act
            var response = await _fixture.MakeRequest<TodoItem>(description, httpMethod, route);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = (TodoItem)JsonApiDeSerializer.Deserialize(body, _jsonApiContext);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(todoItem.Id, deserializedBody.Id);
            Assert.Equal(todoItem.Description, deserializedBody.Description);
            Assert.Equal(todoItem.Ordinal, deserializedBody.Ordinal);
        }

        [Fact]
        public async Task Can_Get_TodoItem_WithOwner()
        {
            // Arrange
            var person = new Person();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items/{todoItem.Id}?include=owner";

            var description = new RequestProperties("Get TodoItem By Id", new Dictionary<string, string> {
                { "/todo-items/{id}", "TodoItem Id" },
                { "?include={relationship}", "Included Relationship" }
            });

            // Act
            var response = await _fixture.MakeRequest<TodoItem>(description, httpMethod, route);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = (TodoItem)JsonApiDeSerializer.Deserialize(body, _jsonApiContext);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(person.Id, deserializedBody.OwnerId);
            Assert.Equal(todoItem.Id, deserializedBody.Id);
            Assert.Equal(todoItem.Description, deserializedBody.Description);
            Assert.Equal(todoItem.Ordinal, deserializedBody.Ordinal);
        }

        [Fact]
        public async Task Can_Post_TodoItem()
        {
            // Arrange
            var person = new Person();
            _context.People.Add(person);
            _context.SaveChanges();

            var todoItem = _todoItemFaker.Generate();
            var content = new
            {
                data = new
                {
                    type = "todo-items",
                    attributes = new
                    {
                        description = todoItem.Description,
                        ordinial = todoItem.Ordinal
                    },
                    relationships = new
                    {
                        owner = new
                        {
                            data = new
                            {
                                type = "people",
                                id = person.Id.ToString()
                            }
                        }
                    }
                }
            };

            var httpMethod = new HttpMethod("POST");
            var route = $"/api/v1/todo-items";

            var request = new HttpRequestMessage(httpMethod, route);
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            var description = new RequestProperties("Post TodoItem");

            // Act
            var response = await _fixture.MakeRequest<TodoItem>(description, request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = (TodoItem)JsonApiDeSerializer.Deserialize(body, _jsonApiContext);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(todoItem.Description, deserializedBody.Description);
        }
    }
}