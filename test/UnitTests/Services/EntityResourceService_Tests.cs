using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class EntityResourceService_Tests
    {
        private readonly Mock<IResourceRepository<TodoItem>> _repositoryMock = new Mock<IResourceRepository<TodoItem>>();
        private readonly IResourceGraph _resourceGraph;

        public EntityResourceService_Tests()
        {
            _resourceGraph = new ResourceGraphBuilder()
                                .AddResource<TodoItem>()
                                .AddResource<TodoItemCollection, Guid>()
                                .Build();

        }

        [Fact]
        public async Task GetRelationshipAsync_Passes_Public_ResourceName_To_Repository()
        {
            // Arrange
            const int id = 1;
            const string relationshipName = "collection";
            var relationship = new RelationshipAttribute[] { new HasOneAttribute(relationshipName) };

            var todoItem = new TodoItem();
            var query = new List<TodoItem> { todoItem }.AsQueryable();

            _repositoryMock.Setup(m => m.Get(id)).Returns(query);
            _repositoryMock.Setup(m => m.Include(query, relationship)).Returns(query);
            _repositoryMock.Setup(m => m.FirstOrDefaultAsync(query)).ReturnsAsync(todoItem);

            var service = GetService();

            // Act
            await service.GetRelationshipAsync(id, relationshipName);

            // Assert
            _repositoryMock.Verify(m => m.Get(id), Times.Once);
            _repositoryMock.Verify(m => m.Include(query, relationship), Times.Once);
            _repositoryMock.Verify(m => m.FirstOrDefaultAsync(query), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipAsync_Returns_Relationship_Value()
        {
            // Arrange
            const int id = 1;
            const string relationshipName = "collection";
            var relationship = new RelationshipAttribute[] { new HasOneAttribute(relationshipName) };

            var todoItem = new TodoItem
            {
                Collection = new TodoItemCollection { Id = Guid.NewGuid() }
            };

            var query = new List<TodoItem> { todoItem }.AsQueryable();

            _repositoryMock.Setup(m => m.Get(id)).Returns(query);
            _repositoryMock.Setup(m => m.Include(query, relationship)).Returns(query);
            _repositoryMock.Setup(m => m.FirstOrDefaultAsync(query)).ReturnsAsync(todoItem);

            var repository = GetService();

            // Act
            var result = await repository.GetRelationshipAsync(id, relationshipName);

            // Assert
            Assert.NotNull(result);
            var collection = Assert.IsType<TodoItemCollection>(result);
            Assert.Equal(todoItem.Collection.Id, collection.Id);
        }

        private DefaultResourceService<TodoItem> GetService()
        {
            var includeService = new Mock<IIncludeService>();
            includeService.Setup(m => m.Get()).Returns(new List<List<RelationshipAttribute>>());
            return new DefaultResourceService<TodoItem>(new List<IQueryParameterService>() { includeService.Object }, new JsonApiOptions(), _repositoryMock.Object, _resourceGraph);
        }
    }
}
