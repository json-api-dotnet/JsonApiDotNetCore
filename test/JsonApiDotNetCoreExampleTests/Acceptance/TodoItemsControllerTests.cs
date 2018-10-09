using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    [Collection("WebHostCollection")]
    public class TodoItemControllerTests
    {
        private TestFixture<TestStartup> _fixture;
        private AppDbContext _context;
        private IJsonApiContext _jsonApiContext;
        private Faker<TodoItem> _todoItemFaker;

        public TodoItemControllerTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
            _jsonApiContext = fixture.GetService<IJsonApiContext>();
            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());
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
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetService<IJsonApiDeSerializer>().DeserializeList<TodoItem>(body);

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
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetService<IJsonApiDeSerializer>().DeserializeList<TodoItem>(body);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(deserializedBody);

            foreach (var todoItemResult in deserializedBody)
                Assert.Equal(todoItem.Ordinal, todoItemResult.Ordinal);
        }

        [Fact]
        public async Task Can_Filter_TodoItems_Using_IsNotNull_Operator()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.UpdatedDate = new DateTime();

            var otherTodoItem = _todoItemFaker.Generate();
            otherTodoItem.UpdatedDate = null;

            _context.TodoItems.AddRange(new[] { todoItem, otherTodoItem });
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?filter[updated-date]=isnotnull:";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var todoItems = _fixture.GetService<IJsonApiDeSerializer>().DeserializeList<TodoItem>(body);

            // Assert
            Assert.NotEmpty(todoItems);
            Assert.All(todoItems, t => Assert.NotNull(t.UpdatedDate));
        }

        [Fact]
        public async Task Can_Filter_TodoItems_Using_IsNull_Operator()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.UpdatedDate = null;

            var otherTodoItem = _todoItemFaker.Generate();
            otherTodoItem.UpdatedDate = new DateTime();

            _context.TodoItems.AddRange(new[] { todoItem, otherTodoItem });
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?filter[updated-date]=isnull:";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var todoItems = _fixture.GetService<IJsonApiDeSerializer>().DeserializeList<TodoItem>(body);

            // Assert
            Assert.NotEmpty(todoItems);
            Assert.All(todoItems, t => Assert.Null(t.UpdatedDate));
        }

        [Fact]
        public async Task Can_Filter_TodoItems_Using_Like_Operator()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.Ordinal = 999999;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();
            var substring = todoItem.Description.Substring(1, todoItem.Description.Length - 2);

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?filter[description]=like:{substring}";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetService<IJsonApiDeSerializer>().DeserializeList<TodoItem>(body);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(deserializedBody);

            foreach (var todoItemResult in deserializedBody)
                Assert.Contains(substring, todoItem.Description);
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
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetService<IJsonApiDeSerializer>().DeserializeList<TodoItem>(body);

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
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetService<IJsonApiDeSerializer>().DeserializeList<TodoItem>(body);

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
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = (TodoItem)_fixture.GetService<IJsonApiDeSerializer>().Deserialize(body);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(todoItem.Id, deserializedBody.Id);
            Assert.Equal(todoItem.Description, deserializedBody.Description);
            Assert.Equal(todoItem.Ordinal, deserializedBody.Ordinal);
            Assert.Equal(todoItem.CreatedDate.ToString("G"), deserializedBody.CreatedDate.ToString("G"));
            Assert.Null(deserializedBody.AchievedDate);
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
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var deserializedBody = (TodoItem)_fixture.GetService<IJsonApiDeSerializer>().Deserialize(body);
            Assert.Equal(person.Id, deserializedBody.OwnerId);
            Assert.Equal(todoItem.Id, deserializedBody.Id);
            Assert.Equal(todoItem.Description, deserializedBody.Description);
            Assert.Equal(todoItem.Ordinal, deserializedBody.Ordinal);
            Assert.Equal(todoItem.CreatedDate.ToString("G"), deserializedBody.CreatedDate.ToString("G"));
            Assert.Null(deserializedBody.AchievedDate);
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
                    attributes = new Dictionary<string, object>()
                    {
                        { "description", todoItem.Description },
                        { "ordinal", todoItem.Ordinal },
                        { "created-date", todoItem.CreatedDate }
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

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = (TodoItem)_fixture.GetService<IJsonApiDeSerializer>().Deserialize(body);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(todoItem.Description, deserializedBody.Description);
            Assert.Equal(todoItem.CreatedDate.ToString("G"), deserializedBody.CreatedDate.ToString("G"));
            Assert.Null(deserializedBody.AchievedDate);
        }


        [Fact]
        public async Task Can_Post_TodoItem_With_Different_Owner_And_Assignee()
        {
            // Arrange
            var person1 = new Person();
            var person2 = new Person();
            _context.People.Add(person1);
            _context.People.Add(person2);
            _context.SaveChanges();

            var todoItem = _todoItemFaker.Generate();
            var content = new
            {
                data = new
                {
                    type = "todo-items",
                    attributes = new Dictionary<string, object>()
                    {
                        { "description", todoItem.Description },
                        { "ordinal", todoItem.Ordinal },
                        { "created-date", todoItem.CreatedDate }
                    },
                    relationships = new
                    {
                        owner = new
                        {
                            data = new
                            {
                                type = "people",
                                id = person1.Id.ToString()
                            }
                        },
                        assignee = new
                        {
                            data = new
                            {
                                type = "people",
                                id = person2.Id.ToString()
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

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert -- response
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(body);
            var resultId = int.Parse(document.Data.Id);

            // Assert -- database
            var todoItemResult = await _context.TodoItems.SingleAsync(t => t.Id == resultId);

            Assert.Equal(person1.Id, todoItemResult.OwnerId);
            Assert.Equal(person2.Id, todoItemResult.AssigneeId);
        }

        [Fact]
        public async Task Can_Patch_TodoItem()
        {
            // Arrange
            var person = new Person();
            _context.People.Add(person);
            _context.SaveChanges();

            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var newTodoItem = _todoItemFaker.Generate();

            var content = new
            {
                data = new
                {
                    type = "todo-items",
                    attributes = new Dictionary<string, object>()
                    {
                        { "description", newTodoItem.Description },
                        { "ordinal", newTodoItem.Ordinal },
                        { "created-date", newTodoItem.CreatedDate }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todo-items/{todoItem.Id}";

            var request = new HttpRequestMessage(httpMethod, route);
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = (TodoItem)_fixture.GetService<IJsonApiDeSerializer>().Deserialize(body);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(newTodoItem.Description, deserializedBody.Description);
            Assert.Equal(newTodoItem.Ordinal, deserializedBody.Ordinal);
            Assert.Equal(newTodoItem.CreatedDate.ToString("G"), deserializedBody.CreatedDate.ToString("G"));
            Assert.Null(deserializedBody.AchievedDate);
        }

        [Fact]
        public async Task Can_Patch_TodoItemWithNullable()
        {
            // Arrange
            var person = new Person();
            _context.People.Add(person);
            _context.SaveChanges();

            var todoItem = _todoItemFaker.Generate();
            todoItem.AchievedDate = System.DateTime.Now;
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var newTodoItem = _todoItemFaker.Generate();
            newTodoItem.AchievedDate = System.DateTime.Now.AddDays(2);

            var content = new
            {
                data = new
                {
                    type = "todo-items",
                    attributes = new Dictionary<string, object>()
                    {
                        { "description", newTodoItem.Description },
                        { "ordinal", newTodoItem.Ordinal },
                        { "created-date", newTodoItem.CreatedDate },
                        { "achieved-date", newTodoItem.AchievedDate }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todo-items/{todoItem.Id}";

            var request = new HttpRequestMessage(httpMethod, route);
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = (TodoItem)_fixture.GetService<IJsonApiDeSerializer>().Deserialize(body);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(newTodoItem.Description, deserializedBody.Description);
            Assert.Equal(newTodoItem.Ordinal, deserializedBody.Ordinal);
            Assert.Equal(newTodoItem.CreatedDate.ToString("G"), deserializedBody.CreatedDate.ToString("G"));
            Assert.Equal(newTodoItem.AchievedDate.GetValueOrDefault().ToString("G"), deserializedBody.AchievedDate.GetValueOrDefault().ToString("G"));
        }

        [Fact]
        public async Task Can_Patch_TodoItemWithNullValue()
        {
            // Arrange
            var person = new Person();
            _context.People.Add(person);
            _context.SaveChanges();

            var todoItem = _todoItemFaker.Generate();
            todoItem.AchievedDate = System.DateTime.Now;
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var newTodoItem = _todoItemFaker.Generate();

            var content = new
            {
                data = new
                {
                    type = "todo-items",
                    attributes = new Dictionary<string, object>()
                    {
                        { "description", newTodoItem.Description },
                        { "ordinal", newTodoItem.Ordinal },
                        { "created-date", newTodoItem.CreatedDate },
                        { "achieved-date", null }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todo-items/{todoItem.Id}";

            var request = new HttpRequestMessage(httpMethod, route);
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = (TodoItem)_fixture.GetService<IJsonApiDeSerializer>().Deserialize(body);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(newTodoItem.Description, deserializedBody.Description);
            Assert.Equal(newTodoItem.Ordinal, deserializedBody.Ordinal);
            Assert.Equal(newTodoItem.CreatedDate.ToString("G"), deserializedBody.CreatedDate.ToString("G"));
            Assert.Null(deserializedBody.AchievedDate);
        }

        [Fact]
        public async Task Can_Delete_TodoItem()
        {
            // Arrange
            var person = new Person();
            _context.People.Add(person);
            _context.SaveChanges();

            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var httpMethod = new HttpMethod("DELETE");
            var route = $"/api/v1/todo-items/{todoItem.Id}";

            var request = new HttpRequestMessage(httpMethod, route);
            request.Content = new StringContent(string.Empty);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Null(_context.TodoItems.FirstOrDefault(t => t.Id == todoItem.Id));
        }
    }
}
