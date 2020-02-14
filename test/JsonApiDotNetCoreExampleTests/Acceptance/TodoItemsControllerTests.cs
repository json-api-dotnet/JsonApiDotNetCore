using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
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
    public class TodoItemControllerTests
    {
        private TestFixture<Startup> _fixture;
        private AppDbContext _context;
        private Faker<TodoItem> _todoItemFaker;
        private Faker<Person> _personFaker;

        public TodoItemControllerTests(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
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
            _context.TodoItems.RemoveRange(_context.TodoItems.ToList());
            _context.SaveChanges();
            int expectedEntitiesPerPage = _fixture.GetService<IJsonApiOptions>().DefaultPageSize;
            var person = new Person();
            var todoItems = _todoItemFaker.Generate(expectedEntitiesPerPage + 1);

            foreach (var todoItem in todoItems)
            {
                todoItem.Owner = person;
                _context.TodoItems.Add(todoItem);
                _context.SaveChanges();

            }

            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(deserializedBody);
            Assert.True(deserializedBody.Count <= expectedEntitiesPerPage, $"There are more items on the page than the default page size. {deserializedBody.Count} > {expectedEntitiesPerPage}");
        }

        [Fact]
        public async Task Can_Filter_By_Resource_Id()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todoItems?filter[id]={todoItem.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(deserializedBody);
            Assert.Contains(deserializedBody, (i) => i.Id == todoItem.Id);
        }

        [Fact]
        public async Task Can_Filter_By_Relationship_Id()
        {
            // Arrange
            var person = new Person();
            var todoItems = _todoItemFaker.Generate(3).ToList();
            _context.TodoItems.AddRange(todoItems);
            todoItems[0].Owner = person;
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todoItems?filter[owner.id]={person.Id}";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(deserializedBody);
            Assert.Contains(deserializedBody, (i) => i.Id == todoItems[0].Id);
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
            var route = $"/api/v1/todoItems?filter[ordinal]={todoItem.Ordinal}";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;

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

            _context.TodoItems.AddRange(todoItem, otherTodoItem);
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems?filter[updatedDate]=isnotnull:";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var todoItems = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;

            // Assert
            Assert.NotEmpty(todoItems);
            Assert.All(todoItems, t => Assert.NotNull(t.UpdatedDate));
        }

        [Fact]
        public async Task Can_Filter_TodoItems_ByParent_Using_IsNotNull_Operator()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.Assignee = new Person();

            var otherTodoItem = _todoItemFaker.Generate();
            otherTodoItem.Assignee = null;

            _context.RemoveRange(_context.TodoItems);
            _context.TodoItems.AddRange(todoItem, otherTodoItem);
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems?filter[assignee.id]=isnotnull:";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var list = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;

            // Assert
            Assert.Equal(todoItem.Id, list.Single().Id);
        }

        [Fact]
        public async Task Can_Filter_TodoItems_Using_IsNull_Operator()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.UpdatedDate = null;

            var otherTodoItem = _todoItemFaker.Generate();
            otherTodoItem.UpdatedDate = new DateTime();

            _context.TodoItems.AddRange(todoItem, otherTodoItem);
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems?filter[updatedDate]=isnull:";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var todoItems = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;

            // Assert
            Assert.NotEmpty(todoItems);
            Assert.All(todoItems, t => Assert.Null(t.UpdatedDate));
        }

        [Fact]
        public async Task Can_Filter_TodoItems_ByParent_Using_IsNull_Operator()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.Assignee = null;

            var otherTodoItem = _todoItemFaker.Generate();
            otherTodoItem.Assignee = new Person();

            _context.TodoItems.AddRange(todoItem, otherTodoItem);
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems?filter[assignee.id]=isnull:";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var todoItems = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;

            // Assert
            Assert.NotEmpty(todoItems);
            Assert.All(todoItems, t => Assert.Null(t.Assignee));
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
            var route = $"/api/v1/todoItems?filter[description]=like:{substring}";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(deserializedBody);

            foreach (var todoItemResult in deserializedBody)
                Assert.Contains(substring, todoItemResult.Description);
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
            var route = "/api/v1/todoItems?sort=ordinal";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;

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
        public async Task Can_Sort_TodoItems_By_Nested_Attribute_Ascending()
        {
            // Arrange
            _context.TodoItems.RemoveRange(_context.TodoItems);

            const int numberOfItems = 10;

            for (var i = 1; i <= numberOfItems; i++)
            {
                var todoItem = _todoItemFaker.Generate();
                todoItem.Ordinal = i;
                todoItem.Owner = _personFaker.Generate();
                _context.TodoItems.Add(todoItem);
            }
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todoItems?page[size]={numberOfItems}&include=owner&sort=owner.age";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;
            Assert.NotEmpty(deserializedBody);

            long lastAge = 0;
            foreach (var todoItemResult in deserializedBody)
            {
                Assert.True(todoItemResult.Owner.Age >= lastAge);
                lastAge = todoItemResult.Owner.Age;
            }
        }

        [Fact]
        public async Task Can_Sort_TodoItems_By_Nested_Attribute_Descending()
        {
            // Arrange
            _context.TodoItems.RemoveRange(_context.TodoItems);

            const int numberOfItems = 10;

            for (var i = 1; i <= numberOfItems; i++)
            {
                var todoItem = _todoItemFaker.Generate();
                todoItem.Ordinal = i;
                todoItem.Owner = _personFaker.Generate();
                _context.TodoItems.Add(todoItem);
            }
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todoItems?page[size]={numberOfItems}&include=owner&sort=-owner.age";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;
            Assert.NotEmpty(deserializedBody);

            int maxAge = deserializedBody.Max(i => i.Owner.Age) + 1;
            foreach (var todoItemResult in deserializedBody)
            {
                Assert.True(todoItemResult.Owner.Age <= maxAge);
                maxAge = todoItemResult.Owner.Age;
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
            var route = "/api/v1/todoItems?sort=-ordinal";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeList<TodoItem>(body).Data;

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
        public async Task Can_Get_TodoItem_WithOwner()
        {
            // Arrange
            var person = new Person();
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todoItems/{todoItem.Id}?include=owner";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var deserializedBody = _fixture.GetDeserializer().DeserializeSingle<TodoItem>(body).Data;

            Assert.Equal(person.Id, deserializedBody.Owner.Id);
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
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetDeserializer().DeserializeSingle<TodoItemClient>(body).Data;
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(todoItem.Description, deserializedBody.Description);
            Assert.Equal(todoItem.CreatedDate.ToString("G"), deserializedBody.CreatedDate.ToString("G"));
            Assert.Equal(nowOffset.ToString("yyyy-MM-ddTHH:mm:ssK"), deserializedBody.OffsetDate?.ToString("yyyy-MM-ddTHH:mm:ssK"));
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
                    type = "todoItems",
                    attributes = new Dictionary<string, object>()
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
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert -- response
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(body);
            var resultId = int.Parse(document.SingleData.Id);

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
                    id = todoItem.Id,
                    type = "todoItems",
                    attributes = new Dictionary<string, object>()
                    {
                        { "description", newTodoItem.Description },
                        { "ordinal", newTodoItem.Ordinal },
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
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

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
            _context.SaveChanges();

            var todoItem = _todoItemFaker.Generate();
            todoItem.AchievedDate = DateTime.Now;
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var newTodoItem = _todoItemFaker.Generate();
            newTodoItem.AchievedDate = DateTime.Now.AddDays(2);

            var content = new
            {
                data = new
                {
                    id = todoItem.Id,
                    type = "todoItems",
                    attributes = new Dictionary<string, object>()
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
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

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
            _context.SaveChanges();

            var todoItem = _todoItemFaker.Generate();
            todoItem.AchievedDate = DateTime.Now;
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            var newTodoItem = _todoItemFaker.Generate();

            var content = new
            {
                data = new
                {
                    id = todoItem.Id,
                    type = "todoItems",
                    attributes = new Dictionary<string, object>()
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
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

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
            var route = $"/api/v1/todoItems/{todoItem.Id}";

            var request = new HttpRequestMessage(httpMethod, route) {Content = new StringContent(string.Empty)};
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Null(_context.TodoItems.FirstOrDefault(t => t.Id == todoItem.Id));
        }
    }
}
