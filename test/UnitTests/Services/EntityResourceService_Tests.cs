using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Mock<IPageService> _pgsMock;
        private readonly Mock<ITargetedFields> _ufMock;
        private readonly IContextEntityProvider _resourceGraph;

        public EntityResourceService_Tests()
        {
            _crMock = new Mock<ICurrentRequest>();
            _pgsMock = new Mock<IPageService>();
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

            var todoItem = new TodoItem();
            var query = new List<TodoItem> { todoItem }.AsQueryable();

            _repositoryMock.Setup(m => m.Get(id)).Returns(query);
            _repositoryMock.Setup(m => m.Include(query, relationship)).Returns(query);
            _repositoryMock.Setup(m => m.FirstOrDefaultAsync(query)).ReturnsAsync(todoItem);

            var service = GetService();

            // act
            await service.GetRelationshipAsync(id, relationshipName);

            // assert
            _repositoryMock.Verify(m => m.Get(id), Times.Once);
            _repositoryMock.Verify(m => m.Include(query, relationship), Times.Once);
            _repositoryMock.Verify(m => m.FirstOrDefaultAsync(query), Times.Once);
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

            var query = new List<TodoItem> { todoItem }.AsQueryable();

            _repositoryMock.Setup(m => m.Get(id)).Returns(query);
            _repositoryMock.Setup(m => m.Include(query, relationship)).Returns(query);
            _repositoryMock.Setup(m => m.FirstOrDefaultAsync(query)).ReturnsAsync(todoItem);

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
            return new EntityResourceService<TodoItem>(null, null, _repositoryMock.Object, new JsonApiOptions(), null, null, _pgsMock.Object, _resourceGraph);
        }
    }
}
