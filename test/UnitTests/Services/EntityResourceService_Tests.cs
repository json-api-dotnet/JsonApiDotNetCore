using System;
using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class EntityResourceService_Tests
    {
        private readonly Mock<IEntityRepository<TodoItem>> _repositoryMock = new Mock<IEntityRepository<TodoItem>>();
        private readonly ILoggerFactory _loggerFactory = new Mock<ILoggerFactory>().Object;
        private readonly Mock<ICurrentRequest> _crMock;
        private readonly Mock<IPageQueryService> _pgsMock;
        private readonly Mock<ITargetedFields> _ufMock;
        private readonly IResourceGraph _resourceGraph;

        public EntityResourceService_Tests()
        {
            _crMock = new Mock<ICurrentRequest>();
            _pgsMock = new Mock<IPageQueryService>();
            _ufMock = new Mock<ITargetedFields>();
            _resourceGraph = new ResourceGraphBuilder()
                                .AddResource<TodoItem>()
                                .AddResource<TodoItemCollection, Guid>()
                                .Build();

        }

        [Fact]
        public async Task GetRelationshipAsync_Passes_Public_ResourceName_To_Repository()
        {
            // arrange
            const int id = 1;
            const string relationshipName = "collection";
            var relationship = new HasOneAttribute(relationshipName);

            _repositoryMock.Setup(m => m.GetAndIncludeAsync(id, relationship))
                .ReturnsAsync(new TodoItem());

            var service = GetService();

            // act
            await service.GetRelationshipAsync(id, relationshipName);

            // assert
            _repositoryMock.Verify(m => m.GetAndIncludeAsync(id, relationship), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipAsync_Returns_Relationship_Value()
        {
            // arrange
            const int id = 1;
            const string relationshipName = "collection";
            var relationship = new HasOneAttribute(relationshipName);

            var todoItem = new TodoItem
            {
                Collection = new TodoItemCollection { Id = Guid.NewGuid() }
            };

            _repositoryMock.Setup(m => m.GetAndIncludeAsync(id, relationship))
                .ReturnsAsync(todoItem);

            var repository = GetService();

            // act
            var result = await repository.GetRelationshipAsync(id, relationshipName);

            // assert
            Assert.NotNull(result);
            var collection = Assert.IsType<TodoItemCollection>(result);
            Assert.Equal(todoItem.Collection.Id, collection.Id);
        }

        private EntityResourceService<TodoItem> GetService()
        {
            return new EntityResourceService<TodoItem>(_repositoryMock.Object, new JsonApiOptions(), _ufMock.Object, _crMock.Object, null, null, _pgsMock.Object, _resourceGraph);
        }
    }
}
