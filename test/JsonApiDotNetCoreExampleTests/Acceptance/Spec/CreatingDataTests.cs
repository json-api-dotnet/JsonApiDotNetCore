using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Serialization.Client;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Models;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{

    [Collection("WebHostCollection")]
    public class CreatingDataTests : EndToEndTest
    {
        private readonly Faker<TodoItem> _todoItemFaker;
        private readonly Faker<Person> _personFaker;

        public CreatingDataTests(TestFixture<Startup> fixture) : base(fixture)
        {
            _fixture = fixture;
            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());
            _personFaker = new Faker<Person>()
                    .RuleFor(t => t.FirstName, f => f.Name.FirstName())
                    .RuleFor(t => t.LastName, f => f.Name.LastName());

        }

        [Fact]
        public async Task CreateResource_GuidResource_IsCreated()
        {
            // Arrange
            var dbContext = PrepareTest<Startup>();
            var serializer = GetSerializer<TodoItemCollection>(e => new { }, e => new { e.Owner });

            var owner = new Person();
            dbContext.People.Add(owner);
            dbContext.SaveChanges();
            var todoItemCollection = new TodoItemCollection { Owner = owner };

            // Act
            var (body, response) = await Post("/api/v1/todo-collections", serializer.Serialize(todoItemCollection));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
        }

        [Fact]
        public async Task ClientGeneratedId_IntegerIdAndNotEnabled_IsForbidden()
        {
            // Arrange
            var dbContext = PrepareTest<Startup>();
            var serializer = GetSerializer<TodoItem>(e => new { e.Description, e.Ordinal, e.CreatedDate });

            var todoItem = _todoItemFaker.Generate();
            const int clientDefinedId = 9999;
            todoItem.Id = clientDefinedId;

            // Act
            var (body, response) = await Post("/api/v1/todo-items", serializer.Serialize(todoItem));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Forbidden, response);
        }


        [Fact]
        public async Task ClientGeneratedId_IntegerIdAndEnabled_IsCreated()
        {
            // Arrange
            var dbContext = PrepareTest<ClientGeneratedIdsStartup>();
            var serializer = GetSerializer<TodoItem>(e => new { e.Description, e.Ordinal, e.CreatedDate });

            var todoItem = _todoItemFaker.Generate();
            const int clientDefinedId = 9999;
            todoItem.Id = clientDefinedId;

            // Act
            var (body, response) = await Post("/api/v1/todo-items", serializer.Serialize(todoItem));
            var responseItem = _deserializer.DeserializeSingle<TodoItemClient>(body).Data;

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            Assert.Equal(clientDefinedId, responseItem.Id);
        }

        [Fact]
        public async Task ClientGeneratedId_GuidIdAndEnabled_IsCreated()
        {
            // Arrange
            var dbContext = PrepareTest<ClientGeneratedIdsStartup>();
            var serializer = GetSerializer<TodoItemCollection>(e => new { }, e => new { e.Owner });

            var owner = new Person();
            dbContext.People.Add(owner);
            await dbContext.SaveChangesAsync();
            var clientDefinedId = Guid.NewGuid();
            var todoItemCollection = new TodoItemCollection { Owner = owner, OwnerId = owner.Id, Id = clientDefinedId };

            // Act
            var (body, response) = await Post("/api/v1/todo-collections", serializer.Serialize(todoItemCollection));
            var responseItem = _deserializer.DeserializeSingle<TodoItemCollectionClient>(body).Data;

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            Assert.Equal(clientDefinedId, responseItem.Id);
        }

        [Fact]
        public async Task CreateWithRelationship_HasMany_IsCreated()
        {
            // Arrange
            var dbContext = PrepareTest<Startup>();
            var serializer = GetSerializer<TodoItemCollection>(e => new { }, e => new { e.TodoItems });

            var todoItem = _todoItemFaker.Generate();
            dbContext.TodoItems.Add(todoItem);
            dbContext.SaveChanges();
            var todoCollection = new TodoItemCollection { TodoItems = new List<TodoItem> { todoItem } };

            // Act
            var (body, response) = await Post("/api/v1/todo-collections", serializer.Serialize(todoCollection));
            var responseItem = _deserializer.DeserializeSingle<TodoItemCollectionClient>(body).Data;

            // Assert
            var contextCollection = GetDbContext().TodoItemCollections.AsNoTracking()
                .Include(c => c.Owner)
                .Include(c => c.TodoItems)
                .SingleOrDefault(c => c.Id == responseItem.Id);

            AssertEqualStatusCode(HttpStatusCode.Created, response);
            Assert.NotEmpty(contextCollection.TodoItems);
            Assert.Equal(todoItem.Id, contextCollection.TodoItems.First().Id);
        }

        [Fact]
        public async Task CreateWithRelationship_HasManyAndInclude_IsCreatedAndIncludes()
        {
            // Arrange
            var dbContext = PrepareTest<Startup>();
            var serializer = GetSerializer<TodoItemCollection>(e => new { }, e => new { e.TodoItems, e.Owner });

            var owner = new Person();
            var todoItem = new TodoItem
            {
                Owner = owner,
                Description = "Description"
            };
            dbContext.People.Add(owner);
            dbContext.TodoItems.Add(todoItem);
            dbContext.SaveChanges();
            var todoCollection = new TodoItemCollection { Owner = owner, TodoItems = new List<TodoItem> { todoItem } };

            // Act
            var (body, response) = await Post("/api/v1/todo-collections?include=todo-items", serializer.Serialize(todoCollection));
            var responseItem = _deserializer.DeserializeSingle<TodoItemCollectionClient>(body).Data;

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);

            Assert.NotNull(responseItem);
            Assert.NotEmpty(responseItem.TodoItems);
            Assert.Equal(todoItem.Description, responseItem.TodoItems.Single().Description);
        }

        [Fact]
        public async Task CreateWithRelationship_HasOne_IsCreated()
        {
            // Arrange
            var dbContext = PrepareTest<Startup>();
            var serializer = GetSerializer<TodoItem>(attributes: ti => new { }, relationships: ti => new { ti.Owner });

            var todoItem = new TodoItem();
            var owner = new Person();
            dbContext.People.Add(owner);
            await dbContext.SaveChangesAsync();
            todoItem.Owner = owner;

            // Act
            var (body, response) = await Post("/api/v1/todo-items", serializer.Serialize(todoItem));
            var responseItem = _deserializer.DeserializeSingle<TodoItemClient>(body).Data;

            // Assert
            var todoItemResult = GetDbContext().TodoItems.AsNoTracking()
                .Include(c => c.Owner)
                .SingleOrDefault(c => c.Id == responseItem.Id);
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            Assert.Equal(owner.Id, todoItemResult.OwnerId);
        }

        [Fact]
        public async Task CreateWithRelationship_HasOneAndInclude_IsCreatedAndIncludes()
        {
            // Arrange
            var dbContext = PrepareTest<Startup>();
            var serializer = GetSerializer<TodoItem>(attributes: ti => new { }, relationships: ti => new { ti.Owner });

            var todoItem = new TodoItem();
            var owner = new Person { FirstName = "Alice" };
            dbContext.People.Add(owner);
            dbContext.SaveChanges();
            todoItem.Owner = owner;

            // Act
            var (body, response) = await Post("/api/v1/todo-items?include=owner", serializer.Serialize(todoItem));
            var responseItem = _deserializer.DeserializeSingle<TodoItemClient>(body).Data;

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            Assert.NotNull(responseItem);
            Assert.NotNull(responseItem.Owner);
            Assert.Equal(owner.FirstName, responseItem.Owner.FirstName);
        }

        [Fact]
        public async Task CreateWithRelationship_HasOneFromIndependentSide_IsCreated()
        {
            // Arrange
            var dbContext = PrepareTest<Startup>();
            var serializer = GetSerializer<PersonRole>(pr => new { }, pr => new { pr.Person });

            var person = new Person();
            dbContext.People.Add(person);
            dbContext.SaveChanges();
            var personRole = new PersonRole { Person = person };

            // Act
            var (body, response) = await Post("/api/v1/person-roles", serializer.Serialize(personRole));
            var responseItem = _deserializer.DeserializeSingle<PersonRole>(body).Data;

            // Assert
            var personRoleResult = dbContext.PersonRoles.AsNoTracking()
                .Include(c => c.Person)
                .SingleOrDefault(c => c.Id == responseItem.Id);
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            Assert.NotEqual(0, responseItem.Id);
            Assert.Equal(person.Id, personRoleResult.Person.Id);
        }

        [Fact]
        public async Task CreateResource_SimpleResource_HeaderLocationsAreCorrect()
        {
            // Arrange
            var dbContext = PrepareTest<Startup>();
            var serializer = GetSerializer<TodoItem>(ti => new { ti.CreatedDate, ti.Description, ti.Ordinal });

            var todoItem = _todoItemFaker.Generate();

            // Act
            var (body, response) = await Post("/api/v1/todo-items", serializer.Serialize(todoItem));
            var responseItem = _deserializer.DeserializeSingle<TodoItemClient>(body).Data;

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            Assert.Equal($"/api/v1/todo-items/{responseItem.Id}", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task CreateResource_EntityTypeMismatch_IsConflict()
        {
            // Arrange
            var dbContext = PrepareTest<Startup>();
            var serializer = GetSerializer<TodoItemCollection>(e => new { }, e => new { e.Owner });
            var resourceGraph = new ResourceGraphBuilder().AddResource<TodoItemClient>("todo-items").AddResource<Person>().AddResource<TodoItemCollectionClient, Guid>().Build();
            var _deserializer = new ResponseDeserializer(resourceGraph);

            var content = serializer.Serialize(_todoItemFaker.Generate()).Replace("todo-items", "people");

            // Act
            var (body, response) = await Post("/api/v1/todo-items", content);

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Conflict, response);
        }

        [Fact]
        public async Task CreateRelationship_ToOneWithImplicitRemove_IsCreated()
        {
            // Arrange
            var dbContext = PrepareTest<Startup>();
            var serializer = GetSerializer<Person>(e => new { e.FirstName }, e => new { e.Passport });

            var passport = new Passport();
            var currentPerson = _personFaker.Generate();
            currentPerson.Passport = passport;
            dbContext.People.Add(currentPerson);
            dbContext.SaveChanges();
            var newPerson = _personFaker.Generate();
            newPerson.Passport = passport;

            // Act
            var (body, response) = await Post("/api/v1/people", serializer.Serialize(newPerson));
            var responseItem = _deserializer.DeserializeSingle<Person>(body).Data;

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            var newPersonDb = dbContext.People.AsNoTracking().Where(p => p.Id == responseItem.Id).Include(e => e.Passport).Single();
            Assert.NotNull(newPersonDb.Passport);
            Assert.Equal(passport.Id, newPersonDb.Passport.Id);
        }

        [Fact]
        public async Task CreateRelationship_ToManyWithImplicitRemove_IsCreated()
        {
            // Arrange
            var dbContext = PrepareTest<Startup>();
            var serializer = GetSerializer<Person>(e => new { e.FirstName }, e => new { e.TodoItems });

            var context = _fixture.GetService<AppDbContext>();
            var currentPerson = _personFaker.Generate();
            var todoItems = _todoItemFaker.Generate(3).ToList();
            currentPerson.TodoItems = todoItems;
            dbContext.Add(currentPerson);
            dbContext.SaveChanges();
            var firstTd = currentPerson.TodoItems[0];
            var secondTd = currentPerson.TodoItems[1];
            var thirdTd = currentPerson.TodoItems[2];

            var newPerson = _personFaker.Generate();
            newPerson.TodoItems = new List<TodoItem> { firstTd, secondTd };

            // Act
            var (body, response) = await Post("/api/v1/people", serializer.Serialize(newPerson));
            var responseItem = _deserializer.DeserializeSingle<Person>(body).Data;

            // Assert
            var newPersonDb = dbContext.People.AsNoTracking().Where(p => p.Id == responseItem.Id).Include(e => e.TodoItems).Single();
            var oldPersonDb = dbContext.People.AsNoTracking().Where(p => p.Id == currentPerson.Id).Include(e => e.TodoItems).Single();
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            Assert.Equal(2, newPersonDb.TodoItems.Count);
            Assert.Single(oldPersonDb.TodoItems);
            Assert.NotNull(newPersonDb.TodoItems.SingleOrDefault(ti => ti.Id == firstTd.Id));
            Assert.NotNull(newPersonDb.TodoItems.SingleOrDefault(ti => ti.Id == secondTd.Id));
            Assert.NotNull(oldPersonDb.TodoItems.SingleOrDefault(ti => ti.Id == thirdTd.Id));
        }
    }
}
