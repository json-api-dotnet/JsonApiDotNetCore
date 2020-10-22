using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    [Collection("WebHostCollection")]
    public sealed class TodoItemControllerTests
    {
        private readonly TestFixture<TestStartup> _fixture;
        private readonly AppDbContext _context;
        private readonly Faker<TodoItem> _todoItemFaker;
        private readonly Faker<Person> _personFaker;

        public TodoItemControllerTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetRequiredService<AppDbContext>();
            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());

            _personFaker = new Faker<Person>()
                .RuleFor(t => t.FirstName, f => f.Name.FirstName())
                .RuleFor(t => t.LastName, f => f.Name.LastName())
                .RuleFor(t => t.Age, f => f.Random.Int(1, 99));
        }

        [Fact]
        public async Task Can_Get_TodoItems_Paginate_Check()
        {
            // Arrange
            await _context.ClearTableAsync<TodoItem>();
            await _context.SaveChangesAsync();
            var expectedResourcesPerPage = _fixture.GetRequiredService<IJsonApiOptions>().DefaultPageSize.Value;
            var person = new Person();
            var todoItems = _todoItemFaker.Generate(expectedResourcesPerPage + 1);

            foreach (var todoItem in todoItems)
            {
                todoItem.Owner = person;
                _context.TodoItems.Add(todoItem);
                await _context.SaveChangesAsync();

            }

            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeMany<TodoItem>(body).Data;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(deserializedBody);
            Assert.True(deserializedBody.Count <= expectedResourcesPerPage, $"There are more items on the page than the default page size. {deserializedBody.Count} > {expectedResourcesPerPage}");
        }

        [Fact]
        public async Task Can_Get_TodoItem_ById()
        {
            // Arrange
            var person = new Person();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todoItems/{todoItem.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeSingle<TodoItem>(body).Data;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
            await _context.SaveChangesAsync();

            var serializer = _fixture.GetSerializer<TodoItem>(e => new { e.Description, e.OffsetDate, e.Ordinal, e.CreatedDate }, e => new { e.Owner });

            var todoItem = _todoItemFaker.Generate();
            var nowOffset = new DateTimeOffset();
            todoItem.OffsetDate = nowOffset;

            var httpMethod = new HttpMethod("POST");
            var route = "/api/v1/todoItems";

            var request = new HttpRequestMessage(httpMethod, route)
            {
                Content = new StringContent(serializer.Serialize(todoItem))
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeSingle<TodoItemClient>(body).Data;
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(todoItem.Description, deserializedBody.Description);
            Assert.Equal(todoItem.CreatedDate.ToString("G"), deserializedBody.CreatedDate.ToString("G"));
            Assert.Equal(nowOffset, deserializedBody.OffsetDate);
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
            await _context.SaveChangesAsync();

            var todoItem = _todoItemFaker.Generate();
            var content = new
            {
                data = new
                {
                    type = "todoItems",
                    attributes = new Dictionary<string, object>
                    {
                        { "description", todoItem.Description },
                        { "ordinal", todoItem.Ordinal },
                        { "createdDate", todoItem.CreatedDate }
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
            var route = "/api/v1/todoItems";

            var request = new HttpRequestMessage(httpMethod, route)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content))
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert -- response
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(body);
            var resultId = int.Parse(document.SingleData.Id);

            // Assert -- database
            var todoItemResult = await _context.TodoItems.SingleAsync(t => t.Id == resultId);

            Assert.Equal(person1.Id, todoItemResult.Owner.Id);
            Assert.Equal(person2.Id, todoItemResult.AssigneeId);
        }

        [Fact]
        public async Task Can_Patch_TodoItem()
        {
            // Arrange
            var person = new Person();
            _context.People.Add(person);
            await _context.SaveChangesAsync();

            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var newTodoItem = _todoItemFaker.Generate();

            var content = new
            {
                data = new
                {
                    id = todoItem.Id,
                    type = "todoItems",
                    attributes = new Dictionary<string, object>
                    {
                        { "description", newTodoItem.Description },
                        { "ordinal", newTodoItem.Ordinal },
                        { "alwaysChangingValue", "ignored" },
                        { "createdDate", newTodoItem.CreatedDate }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoItems/{todoItem.Id}";

            var request = new HttpRequestMessage(httpMethod, route)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content))
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeSingle<TodoItem>(body).Data;

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
            await _context.SaveChangesAsync();

            var todoItem = _todoItemFaker.Generate();
            todoItem.AchievedDate = new DateTime(2002, 2,2);
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var newTodoItem = _todoItemFaker.Generate();
            newTodoItem.AchievedDate = new DateTime(2002, 2,4);

            var content = new
            {
                data = new
                {
                    id = todoItem.Id,
                    type = "todoItems",
                    attributes = new Dictionary<string, object>
                    {
                        { "description", newTodoItem.Description },
                        { "ordinal", newTodoItem.Ordinal },
                        { "createdDate", newTodoItem.CreatedDate },
                        { "achievedDate", newTodoItem.AchievedDate }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoItems/{todoItem.Id}";

            var request = new HttpRequestMessage(httpMethod, route)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content))
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeSingle<TodoItem>(body).Data;

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
            await _context.SaveChangesAsync();

            var todoItem = _todoItemFaker.Generate();
            todoItem.AchievedDate = new DateTime(2002, 2,2);
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var newTodoItem = _todoItemFaker.Generate();

            var content = new
            {
                data = new
                {
                    id = todoItem.Id,
                    type = "todoItems",
                    attributes = new Dictionary<string, object>
                    {
                        { "description", newTodoItem.Description },
                        { "ordinal", newTodoItem.Ordinal },
                        { "createdDate", newTodoItem.CreatedDate },
                        { "achievedDate", null }
                    }
                }
            };

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoItems/{todoItem.Id}";

            var request = new HttpRequestMessage(httpMethod, route)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content))
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeSingle<TodoItem>(body).Data;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(newTodoItem.Description, deserializedBody.Description);
            Assert.Equal(newTodoItem.Ordinal, deserializedBody.Ordinal);
            Assert.Equal(newTodoItem.CreatedDate.ToString("G"), deserializedBody.CreatedDate.ToString("G"));
            Assert.Null(deserializedBody.AchievedDate);
        }
    }
}
