using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Queries;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.RequestServices;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public sealed class JsonApiResourceServiceTests
    {
        private readonly Mock<IResourceRepository<TodoItem>> _repositoryMock = new Mock<IResourceRepository<TodoItem>>();
        private readonly IResourceGraph _resourceGraph;

        public JsonApiResourceServiceTests()
        {
            _resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
                .AddResource<TodoItem>()
                .AddResource<TodoItemCollection, Guid>()
                .Build();
        }

        [Fact]
        public async Task GetRelationshipAsync_Passes_Public_ResourceName_To_Repository()
        {
            // Arrange
            var todoItem = new TodoItem();

            _repositoryMock.Setup(m => m.GetAsync(It.IsAny<QueryLayer>())).ReturnsAsync(new[] {todoItem});
            var service = GetService();

            // Act
            await service.GetSecondaryAsync(1, "collection");

            // Assert
            _repositoryMock.Verify(m => m.GetAsync(It.IsAny<QueryLayer>()), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipAsync_Returns_Relationship_Value()
        {
            // Arrange
            var todoItem = new TodoItem
            {
                Id = 1,
                Collection = new TodoItemCollection { Id = Guid.NewGuid() }
            };

            _repositoryMock.Setup(m => m.GetAsync(It.IsAny<QueryLayer>())).ReturnsAsync(new[] {todoItem});
            var service = GetService();

            // Act
            var result = await service.GetSecondaryAsync(1, "collection");

            // Assert
            Assert.NotNull(result);
            var collection = Assert.IsType<TodoItemCollection>(result);
            Assert.Equal(todoItem.Collection.Id, collection.Id);
        }

        private JsonApiResourceService<TodoItem> GetService()
        {
            var options = new JsonApiOptions();
            var changeTracker = new ResourceChangeTracker<TodoItem>(options, _resourceGraph, new TargetedFields());
            var serviceProvider = new ServiceContainer();
            var resourceFactory = new ResourceFactory(serviceProvider);
            var resourceDefinitionProvider = new ResourceDefinitionProvider(_resourceGraph, new TestScopedServiceProvider(serviceProvider));
            var paginationContext = new PaginationContext();
            var composer = new QueryLayerComposer(new List<IQueryConstraintProvider>(), _resourceGraph, resourceDefinitionProvider, options, paginationContext);
            var currentRequest = new CurrentRequest
            {
                PrimaryResource = _resourceGraph.GetResourceContext<TodoItem>(),
                SecondaryResource = _resourceGraph.GetResourceContext<TodoItemCollection>(),
                Relationship = _resourceGraph.GetRelationships(typeof(TodoItem))
                    .Single(x => x.PublicName == "collection")
            };

            return new JsonApiResourceService<TodoItem>(_repositoryMock.Object, composer, paginationContext, options,
                NullLoggerFactory.Instance, currentRequest, changeTracker, resourceFactory, null);
        }
    }
}
