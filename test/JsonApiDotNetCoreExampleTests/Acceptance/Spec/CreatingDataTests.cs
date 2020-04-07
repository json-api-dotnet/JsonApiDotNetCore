using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    public sealed class CreatingDataTests : FunctionalTestCollection<StandardApplicationFactory>
    {
        private readonly Faker<TodoItem> _todoItemFaker;
        private readonly Faker<Person> _personFaker;

        public CreatingDataTests(StandardApplicationFactory factory) : base(factory)
        {
            _todoItemFaker = new Faker<TodoItem>()
                    .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                    .RuleFor(t => t.Ordinal, f => f.Random.Number())
                    .RuleFor(t => t.CreatedDate, f => f.Date.Past());
            _personFaker = new Faker<Person>()
                    .RuleFor(t => t.FirstName, f => f.Name.FirstName())
                    .RuleFor(t => t.LastName, f => f.Name.LastName());
        }

        [Fact]
        public async Task CreateResource_ModelWithEntityFrameworkInheritance_IsCreated()
        {
            // Arrange
            var serializer = GetSerializer<SuperUser>(e => new { e.SecurityLevel, e.Username, e.Password });
            var superUser = new SuperUser { SecurityLevel = 1337, Username = "Super", Password = "User" };

            // Act
            var (body, response) = await Post("/api/v1/superUsers", serializer.Serialize(superUser));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            var createdSuperUser = _deserializer.DeserializeSingle<SuperUser>(body).Data;
            var first = _dbContext.SuperUsers.FirstOrDefault(e => e.Id.Equals(createdSuperUser.Id));
            Assert.NotNull(first);
        }

        [Fact]
        public async Task CreateResource_GuidResource_IsCreated()
        {
            // Arrange
            var serializer = GetSerializer<TodoItemCollection>(e => new { }, e => new { e.Owner });
            var owner = new Person();
            _dbContext.People.Add(owner);
            _dbContext.SaveChanges();
            var todoItemCollection = new TodoItemCollection { Owner = owner };

            // Act
            var (_, response) = await Post("/api/v1/todoCollections", serializer.Serialize(todoItemCollection));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
        }

        [Fact]
        public async Task ClientGeneratedId_IntegerIdAndNotEnabled_IsForbidden()
        {
            // Arrange
            var serializer = GetSerializer<TodoItem>(e => new { e.Description, e.Ordinal, e.CreatedDate });
            var todoItem = _todoItemFaker.Generate();
            const int clientDefinedId = 9999;
            todoItem.Id = clientDefinedId;

            // Act
            var (body, response) = await Post("/api/v1/todoItems", serializer.Serialize(todoItem));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Forbidden, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Forbidden, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Specifying the resource id in POST requests is not allowed.", errorDocument.Errors[0].Title);
            Assert.Null(errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task CreateWithRelationship_HasMany_IsCreated()
        {
            // Arrange
            var serializer = GetSerializer<TodoItemCollection>(e => new { }, e => new { e.TodoItems });
            var todoItem = _todoItemFaker.Generate();
            _dbContext.TodoItems.Add(todoItem);
            _dbContext.SaveChanges();
            var todoCollection = new TodoItemCollection { TodoItems = new List<TodoItem> { todoItem } };

            // Act
            var (body, response) = await Post("/api/v1/todoCollections", serializer.Serialize(todoCollection));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            var responseItem = _deserializer.DeserializeSingle<TodoItemCollectionClient>(body).Data;
            var contextCollection = GetDbContext().TodoItemCollections.AsNoTracking()
                .Include(c => c.Owner)
                .Include(c => c.TodoItems)
                .SingleOrDefault(c => c.Id == responseItem.Id);

            Assert.NotEmpty(contextCollection.TodoItems);
            Assert.Equal(todoItem.Id, contextCollection.TodoItems.First().Id);
        }

        [Fact]
        public async Task CreateWithRelationship_HasManyAndInclude_IsCreatedAndIncludes()
        {
            // Arrange
            var serializer = GetSerializer<TodoItemCollection>(e => new { }, e => new { e.TodoItems, e.Owner });
            var owner = new Person();
            var todoItem = new TodoItem { Owner = owner, Description = "Description" };
            _dbContext.People.Add(owner);
            _dbContext.TodoItems.Add(todoItem);
            _dbContext.SaveChanges();
            var todoCollection = new TodoItemCollection { Owner = owner, TodoItems = new List<TodoItem> { todoItem } };

            // Act
            var (body, response) = await Post("/api/v1/todoCollections?include=todoItems", serializer.Serialize(todoCollection));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            var responseItem = _deserializer.DeserializeSingle<TodoItemCollectionClient>(body).Data;
            Assert.NotNull(responseItem);
            Assert.NotEmpty(responseItem.TodoItems);
            Assert.Equal(todoItem.Description, responseItem.TodoItems.Single().Description);
        }

        [Fact]
        public async Task CreateWithRelationship_HasOne_IsCreated()
        {
            // Arrange
            var serializer = GetSerializer<TodoItem>(attributes: ti => new { }, relationships: ti => new { ti.Owner });
            var todoItem = new TodoItem();
            var owner = new Person();
            _dbContext.People.Add(owner);
            _dbContext.SaveChanges();
            todoItem.Owner = owner;

            // Act
            var (body, response) = await Post("/api/v1/todoItems", serializer.Serialize(todoItem));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            var responseItem = _deserializer.DeserializeSingle<TodoItemClient>(body).Data;
            var todoItemResult = GetDbContext().TodoItems.AsNoTracking()
                .Include(c => c.Owner)
                .SingleOrDefault(c => c.Id == responseItem.Id);
            Assert.Equal(owner.Id, todoItemResult.OwnerId);
        }

        [Fact]
        public async Task CreateWithRelationship_HasOneAndInclude_IsCreatedAndIncludes()
        {
            // Arrange
            var serializer = GetSerializer<TodoItem>(attributes: ti => new { }, relationships: ti => new { ti.Owner });
            var todoItem = new TodoItem();
            var owner = new Person { FirstName = "Alice" };
            _dbContext.People.Add(owner);
            _dbContext.SaveChanges();
            todoItem.Owner = owner;

            // Act
            var (body, response) = await Post("/api/v1/todoItems?include=owner", serializer.Serialize(todoItem));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            var responseItem = _deserializer.DeserializeSingle<TodoItemClient>(body).Data;
            Assert.NotNull(responseItem);
            Assert.NotNull(responseItem.Owner);
            Assert.Equal(owner.FirstName, responseItem.Owner.FirstName);
        }

        [Fact]
        public async Task CreateWithRelationship_HasOneFromIndependentSide_IsCreated()
        {
            // Arrange
            var serializer = GetSerializer<PersonRole>(pr => new { }, pr => new { pr.Person });
            var person = new Person();
            _dbContext.People.Add(person);
            _dbContext.SaveChanges();
            var personRole = new PersonRole { Person = person };

            // Act
            var (body, response) = await Post("/api/v1/personRoles", serializer.Serialize(personRole));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            var responseItem = _deserializer.DeserializeSingle<PersonRole>(body).Data;
            var personRoleResult = _dbContext.PersonRoles.AsNoTracking()
                .Include(c => c.Person)
                .SingleOrDefault(c => c.Id == responseItem.Id);
            Assert.NotEqual(0, responseItem.Id);
            Assert.Equal(person.Id, personRoleResult.Person.Id);
        }

        [Fact]
        public async Task CreateResource_SimpleResource_HeaderLocationsAreCorrect()
        {
            // Arrange
            var serializer = GetSerializer<TodoItem>(ti => new { ti.CreatedDate, ti.Description, ti.Ordinal });
            var todoItem = _todoItemFaker.Generate();

            // Act
            var (body, response) = await Post("/api/v1/todoItems", serializer.Serialize(todoItem));
            var responseItem = _deserializer.DeserializeSingle<TodoItemClient>(body).Data;

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            Assert.Equal($"/api/v1/todoItems/{responseItem.Id}", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task CreateResource_EntityTypeMismatch_IsConflict()
        {
            // Arrange
            string content = JsonConvert.SerializeObject(new
            {
                data = new
                {
                    type = "people"
                }
            });

            // Act
            var (body, response) = await Post("/api/v1/todoItems", content);

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Conflict, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Conflict, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Resource type mismatch between request body and endpoint URL.", errorDocument.Errors[0].Title);
            Assert.Equal("Expected resource of type 'todoItems' in POST request body at endpoint '/api/v1/todoItems', instead of 'people'.", errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task CreateResource_UnknownEntityType_Fails()
        {
            // Arrange
            string content = JsonConvert.SerializeObject(new
            {
                data = new
                {
                    type = "something"
                }
            });

            // Act
            var (body, response) = await Post("/api/v1/todoItems", content);

            // Assert
            AssertEqualStatusCode(HttpStatusCode.UnprocessableEntity, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Failed to deserialize request body: Payload includes unknown resource type.", errorDocument.Errors[0].Title);
            Assert.StartsWith("The resource 'something' is not registered on the resource graph.", errorDocument.Errors[0].Detail);
            Assert.Contains("Request body: <<", errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task CreateRelationship_ToOneWithImplicitRemove_IsCreated()
        {
            // Arrange
            var serializer = GetSerializer<Person>(e => new { e.FirstName }, e => new { e.Passport });
            var passport = new Passport();
            var currentPerson = _personFaker.Generate();
            currentPerson.Passport = passport;
            _dbContext.People.Add(currentPerson);
            _dbContext.SaveChanges();
            var newPerson = _personFaker.Generate();
            newPerson.Passport = passport;

            // Act
            var (body, response) = await Post("/api/v1/people", serializer.Serialize(newPerson));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            var responseItem = _deserializer.DeserializeSingle<Person>(body).Data;
            var newPersonDb = _dbContext.People.AsNoTracking().Where(p => p.Id == responseItem.Id).Include(e => e.Passport).Single();
            Assert.NotNull(newPersonDb.Passport);
            Assert.Equal(passport.Id, newPersonDb.Passport.Id);
        }

        [Fact]
        public async Task CreateRelationship_ToManyWithImplicitRemove_IsCreated()
        {
            // Arrange
            var serializer = GetSerializer<Person>(e => new { e.FirstName }, e => new { e.TodoItems });
            var currentPerson = _personFaker.Generate();
            var todoItems = _todoItemFaker.Generate(3).ToList();
            currentPerson.TodoItems = todoItems;
            _dbContext.Add(currentPerson);
            _dbContext.SaveChanges();
            var firstTd = currentPerson.TodoItems[0];
            var secondTd = currentPerson.TodoItems[1];
            var thirdTd = currentPerson.TodoItems[2];

            var newPerson = _personFaker.Generate();
            newPerson.TodoItems = new List<TodoItem> { firstTd, secondTd };

            // Act
            var (body, response) = await Post("/api/v1/people", serializer.Serialize(newPerson));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            var responseItem = _deserializer.DeserializeSingle<Person>(body).Data;
            var newPersonDb = _dbContext.People.AsNoTracking().Where(p => p.Id == responseItem.Id).Include(e => e.TodoItems).Single();
            var oldPersonDb = _dbContext.People.AsNoTracking().Where(p => p.Id == currentPerson.Id).Include(e => e.TodoItems).Single();
            Assert.Equal(2, newPersonDb.TodoItems.Count);
            Assert.Single(oldPersonDb.TodoItems);
            Assert.NotNull(newPersonDb.TodoItems.SingleOrDefault(ti => ti.Id == firstTd.Id));
            Assert.NotNull(newPersonDb.TodoItems.SingleOrDefault(ti => ti.Id == secondTd.Id));
            Assert.NotNull(oldPersonDb.TodoItems.SingleOrDefault(ti => ti.Id == thirdTd.Id));
        }
    }


    public sealed class CreatingDataWithClientEnabledIdTests : FunctionalTestCollection<ClientEnabledIdsApplicationFactory>
    {
        private readonly Faker<TodoItem> _todoItemFaker;

        public CreatingDataWithClientEnabledIdTests(ClientEnabledIdsApplicationFactory factory) : base(factory)
        {
            _todoItemFaker = new Faker<TodoItem>()
                    .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                    .RuleFor(t => t.Ordinal, f => f.Random.Number())
                    .RuleFor(t => t.CreatedDate, f => f.Date.Past());
        }

        [Fact]
        public async Task ClientGeneratedId_IntegerIdAndEnabled_IsCreated()
        {
            // Arrange
            var serializer = GetSerializer<TodoItem>(e => new { e.Description, e.Ordinal, e.CreatedDate });
            var todoItem = _todoItemFaker.Generate();
            const int clientDefinedId = 9999;
            todoItem.Id = clientDefinedId;

            // Act
            var (body, response) = await Post("/api/v1/todoItems", serializer.Serialize(todoItem));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            var responseItem = _deserializer.DeserializeSingle<TodoItemClient>(body).Data;
            Assert.Equal(clientDefinedId, responseItem.Id);
        }

        [Fact]
        public async Task ClientGeneratedId_GuidIdAndEnabled_IsCreated()
        {
            // Arrange
            var serializer = GetSerializer<TodoItemCollection>(e => new { }, e => new { e.Owner });
            var owner = new Person();
            _dbContext.People.Add(owner);
            await _dbContext.SaveChangesAsync();
            var clientDefinedId = Guid.NewGuid();
            var todoItemCollection = new TodoItemCollection { Owner = owner, OwnerId = owner.Id, Id = clientDefinedId };

            // Act
            var (body, response) = await Post("/api/v1/todoCollections", serializer.Serialize(todoItemCollection));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            var responseItem = _deserializer.DeserializeSingle<TodoItemCollectionClient>(body).Data;
            Assert.Equal(clientDefinedId, responseItem.Id);
        }
    }
}
