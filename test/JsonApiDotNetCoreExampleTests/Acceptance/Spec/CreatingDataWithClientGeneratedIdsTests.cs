using System;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Models;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    public sealed class CreatingDataWithClientGeneratedIdsTests : FunctionalTestCollection<ClientGeneratedIdsApplicationFactory>
    {
        private readonly Faker<TodoItem> _todoItemFaker;

        public CreatingDataWithClientGeneratedIdsTests(ClientGeneratedIdsApplicationFactory factory) : base(factory)
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
