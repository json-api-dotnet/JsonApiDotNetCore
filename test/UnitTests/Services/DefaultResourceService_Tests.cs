using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
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
                .Add<TodoItem>()
                .Add<TodoItemCollection, Guid>()
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
            var resourceDefinitionAccessor = new Mock<IResourceDefinitionAccessor>().Object;
            var resourceRepositoryAccessor = new Mock<IResourceRepositoryAccessor>().Object;
            var paginationContext = new PaginationContext();
            var targetedFields = new Mock<ITargetedFields>().Object;
            var resourceContextProvider = new Mock<IResourceContextProvider>().Object;
            var resourceHookExecutor = new NeverResourceHookExecutorFacade();
            var composer = new QueryLayerComposer(new List<IQueryConstraintProvider>(), _resourceGraph, resourceDefinitionAccessor, options, paginationContext);
            var dataStoreUpdateFailureInspector = new DataStoreUpdateFailureInspector(resourceContextProvider, targetedFields, composer, resourceRepositoryAccessor);

            var request = new JsonApiRequest
            {
                PrimaryResource = _resourceGraph.GetResourceContext<TodoItem>(),
                SecondaryResource = _resourceGraph.GetResourceContext<TodoItemCollection>(),
                Relationship = _resourceGraph.GetRelationships(typeof(TodoItem))
                    .Single(x => x.PublicName == "collection")
            };

            return new JsonApiResourceService<TodoItem>(_repositoryMock.Object, composer, paginationContext, options,
                NullLoggerFactory.Instance, request, changeTracker, resourceFactory, dataStoreUpdateFailureInspector,
                resourceHookExecutor);
        }
    }
}
