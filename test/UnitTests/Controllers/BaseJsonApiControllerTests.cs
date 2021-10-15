#nullable disable

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace UnitTests.Controllers
{
    public sealed class BaseJsonApiControllerTests
    {
        [Fact]
        public async Task GetAsync_Calls_Service()
        {
            // Arrange
            var serviceMock = new Mock<IGetAllService<Resource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new ResourceController(options, resourceGraph, NullLoggerFactory.Instance, serviceMock.Object);

            // Act
            await controller.GetAsync(CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.GetAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetAsync_Throws_405_If_No_Service()
        {
            // Arrange
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new ResourceController(options, resourceGraph, NullLoggerFactory.Instance, null);

            // Act
            Func<Task> asyncAction = () => controller.GetAsync(CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<RequestMethodNotAllowedException>(asyncAction);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Errors[0].StatusCode);
            Assert.Equal(HttpMethod.Get, exception.Method);
        }

        [Fact]
        public async Task GetAsyncById_Calls_Service()
        {
            // Arrange
            const int id = 0;
            var serviceMock = new Mock<IGetByIdService<Resource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new ResourceController(options, resourceGraph, NullLoggerFactory.Instance, getById: serviceMock.Object);

            // Act
            await controller.GetAsync(id, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.GetAsync(id, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetAsyncById_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new ResourceController(options, resourceGraph, NullLoggerFactory.Instance);

            // Act
            Func<Task> asyncAction = () => controller.GetAsync(id, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<RequestMethodNotAllowedException>(asyncAction);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Errors[0].StatusCode);
            Assert.Equal(HttpMethod.Get, exception.Method);
        }

        [Fact]
        public async Task GetRelationshipsAsync_Calls_Service()
        {
            // Arrange
            const int id = 0;
            const string relationshipName = "articles";
            var serviceMock = new Mock<IGetRelationshipService<Resource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new ResourceController(options, resourceGraph, NullLoggerFactory.Instance, getRelationship: serviceMock.Object);

            // Act
            await controller.GetRelationshipAsync(id, relationshipName, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.GetRelationshipAsync(id, relationshipName, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipsAsync_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new ResourceController(options, resourceGraph, NullLoggerFactory.Instance);

            // Act
            Func<Task> asyncAction = () => controller.GetRelationshipAsync(id, "articles", CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<RequestMethodNotAllowedException>(asyncAction);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Errors[0].StatusCode);
            Assert.Equal(HttpMethod.Get, exception.Method);
        }

        [Fact]
        public async Task GetRelationshipAsync_Calls_Service()
        {
            // Arrange
            const int id = 0;
            const string relationshipName = "articles";
            var serviceMock = new Mock<IGetSecondaryService<Resource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new ResourceController(options, resourceGraph, NullLoggerFactory.Instance, getSecondary: serviceMock.Object);

            // Act
            await controller.GetSecondaryAsync(id, relationshipName, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.GetSecondaryAsync(id, relationshipName, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipAsync_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new ResourceController(options, resourceGraph, NullLoggerFactory.Instance);

            // Act
            Func<Task> asyncAction = () => controller.GetSecondaryAsync(id, "articles", CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<RequestMethodNotAllowedException>(asyncAction);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Errors[0].StatusCode);
            Assert.Equal(HttpMethod.Get, exception.Method);
        }

        [Fact]
        public async Task PatchAsync_Calls_Service()
        {
            // Arrange
            const int id = 0;
            var resource = new Resource();
            var serviceMock = new Mock<IUpdateService<Resource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new ResourceController(options, resourceGraph, NullLoggerFactory.Instance, update: serviceMock.Object);

            // Act
            await controller.PatchAsync(id, resource, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.UpdateAsync(id, It.IsAny<Resource>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task PatchAsync_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var resource = new Resource();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new ResourceController(options, resourceGraph, NullLoggerFactory.Instance);

            // Act
            Func<Task> asyncAction = () => controller.PatchAsync(id, resource, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<RequestMethodNotAllowedException>(asyncAction);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Errors[0].StatusCode);
            Assert.Equal(HttpMethod.Patch, exception.Method);
        }

        [Fact]
        public async Task PostAsync_Calls_Service()
        {
            // Arrange
            var resource = new Resource();
            var serviceMock = new Mock<ICreateService<Resource, int>>();
            serviceMock.Setup(service => service.CreateAsync(It.IsAny<Resource>(), It.IsAny<CancellationToken>())).ReturnsAsync(resource);
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new ResourceController(options, resourceGraph, NullLoggerFactory.Instance, create: serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            await controller.PostAsync(resource, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.CreateAsync(It.IsAny<Resource>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task PatchRelationshipsAsync_Calls_Service()
        {
            // Arrange
            const int id = 0;
            const string relationshipName = "articles";
            var serviceMock = new Mock<ISetRelationshipService<Resource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new ResourceController(options, resourceGraph, NullLoggerFactory.Instance, setRelationship: serviceMock.Object);

            // Act
            await controller.PatchRelationshipAsync(id, relationshipName, null, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.SetRelationshipAsync(id, relationshipName, null, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task PatchRelationshipsAsync_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new ResourceController(options, resourceGraph, NullLoggerFactory.Instance);

            // Act
            Func<Task> asyncAction = () => controller.PatchRelationshipAsync(id, "articles", null, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<RequestMethodNotAllowedException>(asyncAction);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Errors[0].StatusCode);
            Assert.Equal(HttpMethod.Patch, exception.Method);
        }

        [Fact]
        public async Task DeleteAsync_Calls_Service()
        {
            // Arrange
            const int id = 0;
            var serviceMock = new Mock<IDeleteService<Resource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new ResourceController(options, resourceGraph, NullLoggerFactory.Instance, delete: serviceMock.Object);

            // Act
            await controller.DeleteAsync(id, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.DeleteAsync(id, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new ResourceController(options, resourceGraph, NullLoggerFactory.Instance);

            // Act
            Func<Task> asyncAction = () => controller.DeleteAsync(id, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<RequestMethodNotAllowedException>(asyncAction);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Errors[0].StatusCode);
            Assert.Equal(HttpMethod.Delete, exception.Method);
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public sealed class Resource : Identifiable<int>
        {
            [Attr]
            public string TestAttribute { get; set; }
        }

        private sealed class ResourceController : BaseJsonApiController<Resource, int>
        {
            public ResourceController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
                IResourceService<Resource, int> resourceService)
                : base(options, resourceGraph, loggerFactory, resourceService)
            {
            }

            public ResourceController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
                IGetAllService<Resource, int> getAll = null, IGetByIdService<Resource, int> getById = null,
                IGetSecondaryService<Resource, int> getSecondary = null, IGetRelationshipService<Resource, int> getRelationship = null,
                ICreateService<Resource, int> create = null, IAddToRelationshipService<Resource, int> addToRelationship = null,
                IUpdateService<Resource, int> update = null, ISetRelationshipService<Resource, int> setRelationship = null,
                IDeleteService<Resource, int> delete = null, IRemoveFromRelationshipService<Resource, int> removeFromRelationship = null)
                : base(options, resourceGraph, loggerFactory, getAll, getById, getSecondary, getRelationship, create, addToRelationship, update,
                    setRelationship, delete, removeFromRelationship)
            {
            }
        }
    }
}
