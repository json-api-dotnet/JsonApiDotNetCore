using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public sealed class UpdatingDataTests : EndToEndTest
    {
        private readonly AppDbContext _context;
        private readonly Faker<TodoItem> _todoItemFaker;
        private readonly Faker<Person> _personFaker;

        public UpdatingDataTests(TestFixture<TestStartup> fixture) : base(fixture)
        { 
            _context = fixture.GetService<AppDbContext>();

            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());
            _personFaker = new Faker<Person>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName());
        }

        [Fact]
        public async Task PatchResource_ModelWithEntityFrameworkInheritance_IsPatched()
        {
            // Arrange
            var dbContext = PrepareTest<TestStartup>();

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);

            var clock = server.Host.Services.GetRequiredService<ISystemClock>();

            var serializer = TestFixture<TestStartup>.GetSerializer<SuperUser>(server.Host.Services, e => new { e.SecurityLevel, e.UserName, e.Password });
            var superUser = new SuperUser(_context) { SecurityLevel = 1337, UserName = "Super", Password = "User", LastPasswordChange = clock.UtcNow.LocalDateTime.AddMinutes(-15) };
            dbContext.Set<SuperUser>().Add(superUser);
            await dbContext.SaveChangesAsync();

            var su = new SuperUser(_context) { Id = superUser.Id, SecurityLevel = 2674, UserName = "Power", Password = "secret" };
            var content = serializer.Serialize(su);

            // Act
            var (body, response) = await Patch($"/api/v1/superUsers/{su.Id}", content);

            // Assert
            AssertEqualStatusCode(HttpStatusCode.OK, response);
            var updated = _deserializer.DeserializeSingle<SuperUser>(body).Data;
            Assert.Equal(su.SecurityLevel, updated.SecurityLevel);
            Assert.Equal(su.UserName, updated.UserName);
            Assert.Null(updated.Password);
        }

        [Fact]
        public async Task Response422IfUpdatingNotSettableAttribute()
        {
            // Arrange
            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();

            var loggerFactory = new FakeLoggerFactory();
            builder.ConfigureLogging(options =>
            {
                options.AddProvider(loggerFactory);
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddFilter((category, level) => level == LogLevel.Trace && 
                    (category == typeof(JsonApiReader).FullName || category == typeof(JsonApiWriter).FullName));
            });

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var todoItem = _todoItemFaker.Generate();
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var serializer = TestFixture<TestStartup>.GetSerializer<TodoItem>(server.Host.Services, ti => new { ti.CalculatedValue });
            var content = serializer.Serialize(todoItem);
            var request = PrepareRequest("PATCH", $"/api/v1/todoItems/{todoItem.Id}", content);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(document.Errors);

            var error = document.Errors.Single();
            Assert.Equal(HttpStatusCode.UnprocessableEntity, error.StatusCode);
            Assert.Equal("Failed to deserialize request body.", error.Title);
            Assert.StartsWith("Property 'TodoItem.CalculatedValue' is read-only. - Request body: <<", error.Detail);

            Assert.NotEmpty(loggerFactory.Logger.Messages);
            Assert.Contains(loggerFactory.Logger.Messages,
                x => x.Text.StartsWith("Received request at ") && x.Text.Contains("with body:"));
            Assert.Contains(loggerFactory.Logger.Messages,
                x => x.Text.StartsWith("Sending 422 response for request at ") && x.Text.Contains("Failed to deserialize request body."));
        }

        [Fact]
        public async Task Respond_404_If_ResourceDoesNotExist()
        {
            // Arrange
            await _context.ClearTableAsync<TodoItem>();
            await _context.SaveChangesAsync();

            var todoItem = _todoItemFaker.Generate();
            todoItem.Id = 100;
            todoItem.CreatedDate = new DateTime(2002, 2,2);
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var serializer = TestFixture<TestStartup>.GetSerializer<TodoItem>(server.Host.Services, ti => new { ti.Description, ti.Ordinal, ti.CreatedDate });
            var content = serializer.Serialize(todoItem);
            var request = PrepareRequest("PATCH", $"/api/v1/todoItems/{todoItem.Id}", content);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.NotFound, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The requested resource does not exist.", errorDocument.Errors[0].Title);
            Assert.Equal("Resource of type 'todoItems' with id '100' does not exist.", errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Respond_422_If_IdNotInAttributeList()
        {
            // Arrange
            var maxPersonId = _context.TodoItems.ToList().LastOrDefault()?.Id ?? 0;
            var todoItem = _todoItemFaker.Generate();
            todoItem.CreatedDate = new DateTime(2002, 2,2);
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var serializer = TestFixture<TestStartup>.GetSerializer<TodoItem>(server.Host.Services, ti => new {ti.Description, ti.Ordinal, ti.CreatedDate});
            var content = serializer.Serialize(todoItem);
            var request = PrepareRequest("PATCH", $"/api/v1/todoItems/{maxPersonId}", content);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(document.Errors);

            var error = document.Errors.Single();
            Assert.Equal(HttpStatusCode.UnprocessableEntity, error.StatusCode);
            Assert.Equal("Failed to deserialize request body: Payload must include id attribute.", error.Title);
            Assert.StartsWith("Request body: <<", error.Detail);
        }

        [Fact]
        public async Task Respond_409_If_IdInUrlIsDifferentFromIdInRequestBody()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.CreatedDate = new DateTime(2002, 2,2);

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var wrongTodoItemId = todoItem.Id + 1;

            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var serializer = TestFixture<TestStartup>.GetSerializer<TodoItem>(server.Host.Services, ti => new {ti.Description, ti.Ordinal, ti.CreatedDate});
            var content = serializer.Serialize(todoItem);
            var request = PrepareRequest("PATCH", $"/api/v1/todoItems/{wrongTodoItemId}", content);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(document.Errors);

            var error = document.Errors.Single();
            Assert.Equal(HttpStatusCode.Conflict, error.StatusCode);
            Assert.Equal("Resource id mismatch between request body and endpoint URL.", error.Title);
            Assert.Equal($"Expected resource id '{wrongTodoItemId}' in PATCH request body at endpoint 'http://localhost/api/v1/todoItems/{wrongTodoItemId}', instead of '{todoItem.Id}'.", error.Detail);
        }

        [Fact]
        public async Task Respond_422_If_Broken_JSON_Payload()
        {
            // Arrange
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var content = "{ \"data\" {";
            var request = PrepareRequest("POST", "/api/v1/todoItems", content);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(document.Errors);

            var error = document.Errors.Single();
            Assert.Equal(HttpStatusCode.UnprocessableEntity, error.StatusCode);
            Assert.Equal("Failed to deserialize request body.", error.Title);
            Assert.StartsWith("Invalid character after parsing", error.Detail);
        }

        [Fact]
        public async Task Can_Patch_Resource()
        {
            // Arrange
            await _context.ClearTableAsync<TodoItemCollection>();
            await _context.ClearTableAsync<TodoItem>();
            await _context.ClearTableAsync<Person>();
            await _context.SaveChangesAsync();

            var todoItem = _todoItemFaker.Generate();
            var person = _personFaker.Generate();
            todoItem.Owner = person;
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var newTodoItem = _todoItemFaker.Generate();
            newTodoItem.Id = todoItem.Id;
            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var serializer = TestFixture<TestStartup>.GetSerializer<TodoItem>(server.Host.Services, p => new { p.Description, p.Ordinal });

            var request = PrepareRequest("PATCH", $"/api/v1/todoItems/{todoItem.Id}", serializer.Serialize(newTodoItem));

            // Act
            var response = await client.SendAsync(request);

            // Assert -- response
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(body);
            Assert.NotNull(document);
            Assert.NotNull(document.Data);
            Assert.NotNull(document.SingleData.Attributes);
            Assert.Equal(newTodoItem.Description, document.SingleData.Attributes["description"]);
            Assert.Equal(newTodoItem.Ordinal, (long)document.SingleData.Attributes["ordinal"]);
            Assert.True(document.SingleData.Relationships.ContainsKey("owner"));
            Assert.Null(document.SingleData.Relationships["owner"].SingleData);

            // Assert -- database
            var updatedTodoItem = _context.TodoItems.AsNoTracking()
                .Include(t => t.Owner)
                .SingleOrDefault(t => t.Id == todoItem.Id);
            Assert.Equal(person.Id, todoItem.OwnerId);
            Assert.Equal(newTodoItem.Description, updatedTodoItem.Description);
            Assert.Equal(newTodoItem.Ordinal, updatedTodoItem.Ordinal);
        }

        [Fact]
        public async Task Patch_Resource_With_HasMany_Does_Not_Include_Relationships()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.Owner = _personFaker.Generate();
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var newPerson = _personFaker.Generate();
            newPerson.Id = todoItem.Owner.Id;
            var builder = WebHost.CreateDefaultBuilder().UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var serializer = TestFixture<TestStartup>.GetSerializer<Person>(server.Host.Services, p => new { p.LastName, p.FirstName });

            var request = PrepareRequest("PATCH", $"/api/v1/people/{todoItem.Owner.Id}", serializer.Serialize(newPerson));

            // Act
            var response = await client.SendAsync(request);

            // Assert -- response
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(body);
            Assert.NotNull(document);
            Assert.NotNull(document.Data);
            Assert.NotNull(document.SingleData.Attributes);
            Assert.Equal(newPerson.LastName, document.SingleData.Attributes["lastName"]);
            Assert.Equal(newPerson.FirstName, document.SingleData.Attributes["firstName"]);
            Assert.True(document.SingleData.Relationships.ContainsKey("todoItems"));
            Assert.Null(document.SingleData.Relationships["todoItems"].Data);
        }

        [Fact]
        public async Task Can_Patch_Resource_And_HasOne_Relationships()
        {
            // Arrange
            var todoItem = _todoItemFaker.Generate();
            todoItem.CreatedDate = new DateTime(2002, 2,2);
            var person = _personFaker.Generate();
            _context.TodoItems.Add(todoItem);
            _context.People.Add(person);
            await _context.SaveChangesAsync();
            todoItem.Owner = person;

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var serializer = _fixture.GetSerializer<TodoItem>(ti => new { ti.Description, ti.Ordinal, ti.CreatedDate }, ti => new { ti.Owner });
            var content = serializer.Serialize(todoItem);
            var request = PrepareRequest("PATCH", $"/api/v1/todoItems/{todoItem.Id}", content);

            // Act
            var response = await client.SendAsync(request);
            var updatedTodoItem = _context.TodoItems.AsNoTracking()
                .Include(t => t.Owner)
                .SingleOrDefault(t => t.Id == todoItem.Id);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(person.Id, updatedTodoItem.OwnerId);
        }

        private HttpRequestMessage PrepareRequest(string method, string route, string content)
        {
            var httpMethod = new HttpMethod(method);
            var request = new HttpRequestMessage(httpMethod, route) {Content = new StringContent(content)};

            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);
            return request;
        }
    }
}
