using System;
using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class EntityResourceService_Tests
    {
        private readonly Mock<IJsonApiContext> _jsonApiContextMock = new Mock<IJsonApiContext>();
        private readonly Mock<IEntityRepository<TodoItem>> _repositoryMock = new Mock<IEntityRepository<TodoItem>>();
        private readonly ILoggerFactory _loggerFactory = new Mock<ILoggerFactory>().Object;

        public EntityResourceService_Tests()
        {
            _jsonApiContextMock
                .Setup(m => m.ResourceGraph)
                .Returns(
                    new ResourceGraphBuilder()
                        .AddResource<TodoItem>("todo-items")
                        .Build()
                );
        }

        [Fact]
        public async Task GetRelationshipAsync_Passes_Public_ResourceName_To_Repository()
        {
            // arrange
            const int id = 1;
            const string relationshipName = "collection";

            _repositoryMock.Setup(m => m.GetAndIncludeAsync(id, relationshipName))
                .ReturnsAsync(new TodoItem());

            var repository = GetService();

            // act
            await repository.GetRelationshipAsync(id, relationshipName);

            // assert
            _repositoryMock.Verify(m => m.GetAndIncludeAsync(id, relationshipName), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipAsync_Returns_Relationship_Value()
        {
            // arrange
            const int id = 1;
            const string relationshipName = "collection";

            var todoItem = new TodoItem
            {
                Collection = new TodoItemCollection { Id = Guid.NewGuid() }
            };

            _repositoryMock.Setup(m => m.GetAndIncludeAsync(id, relationshipName))
                .ReturnsAsync(todoItem);

            var repository = GetService();

            // act
            var result = await repository.GetRelationshipAsync(id, relationshipName);

            // assert
            Assert.NotNull(result);
            var collection = Assert.IsType<TodoItemCollection>(result);
            Assert.Equal(todoItem.Collection.Id, collection.Id);
        }

        private EntityResourceService<TodoItem> GetService() =>
            new EntityResourceService<TodoItem>(_jsonApiContextMock.Object, _repositoryMock.Object, null, _loggerFactory);
    }
}
