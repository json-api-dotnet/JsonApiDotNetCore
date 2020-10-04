using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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

namespace UnitTests
{
    public sealed class BaseJsonApiController_Tests
    {
        public sealed class Resource : Identifiable
        {
            [Attr] public string TestAttribute { get; set; }
        }

        public sealed class ResourceController : BaseJsonApiController<Resource>
        {
            public ResourceController(
                IJsonApiOptions options,
                ILoggerFactory loggerFactory,
                IResourceService<Resource, int> resourceService)
                : base(options, loggerFactory, resourceService)
            { }

            public ResourceController(
                IJsonApiOptions options,
                ILoggerFactory loggerFactory,
                IGetAllService<Resource, int> getAll = null,
                IGetByIdService<Resource, int> getById = null,
                IGetSecondaryService<Resource, int> getSecondary = null,
                IGetRelationshipService<Resource, int> getRelationship = null,
                ICreateService<Resource, int> create = null,
                IUpdateService<Resource, int> update = null,
                IUpdateRelationshipService<Resource, int> updateRelationship = null,
                IDeleteService<Resource, int> delete = null)
                : base(options, loggerFactory, getAll, getById, getSecondary, getRelationship, create,
                    update, updateRelationship, delete)
            { }
        }

        [Fact]
        public async Task GetAsync_Calls_Service()
        {
            // Arrange
            var serviceMock = new Mock<IGetAllService<Resource>>();
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, NullLoggerFactory.Instance, getAll: serviceMock.Object);

            // Act
            await controller.GetAsync();

            // Assert
            serviceMock.Verify(m => m.GetAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAsync_Throws_405_If_No_Service()
        {
            // Arrange
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, NullLoggerFactory.Instance, null);

            // Act
            var exception = await Assert.ThrowsAsync<RequestMethodNotAllowedException>(() => controller.GetAsync());

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Error.StatusCode);
            Assert.Equal(HttpMethod.Get, exception.Method);
        }

        [Fact]
        public async Task GetAsyncById_Calls_Service()
        {
            // Arrange
            const int id = 0;
            var serviceMock = new Mock<IGetByIdService<Resource>>();
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, NullLoggerFactory.Instance, getById: serviceMock.Object);

            // Act
            await controller.GetAsync(id);

            // Assert
            serviceMock.Verify(m => m.GetAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetAsyncById_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, NullLoggerFactory.Instance);

            // Act
            var exception = await Assert.ThrowsAsync<RequestMethodNotAllowedException>(() => controller.GetAsync(id));

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Error.StatusCode);
            Assert.Equal(HttpMethod.Get, exception.Method);
        }

        [Fact]
        public async Task GetRelationshipsAsync_Calls_Service()
        {
            // Arrange
            const int id = 0;
            var serviceMock = new Mock<IGetRelationshipService<Resource>>();
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, NullLoggerFactory.Instance, getRelationship: serviceMock.Object);

            // Act
            await controller.GetRelationshipAsync(id, string.Empty);

            // Assert
            serviceMock.Verify(m => m.GetRelationshipAsync(id, string.Empty), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipsAsync_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, NullLoggerFactory.Instance);

            // Act
            var exception = await Assert.ThrowsAsync<RequestMethodNotAllowedException>(() => controller.GetRelationshipAsync(id, string.Empty));

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Error.StatusCode);
            Assert.Equal(HttpMethod.Get, exception.Method);
        }

        [Fact]
        public async Task GetRelationshipAsync_Calls_Service()
        {
            // Arrange
            const int id = 0;
            var serviceMock = new Mock<IGetSecondaryService<Resource>>();
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, NullLoggerFactory.Instance, getSecondary: serviceMock.Object);

            // Act
            await controller.GetSecondaryAsync(id, string.Empty);

            // Assert
            serviceMock.Verify(m => m.GetSecondaryAsync(id, string.Empty), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipAsync_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, NullLoggerFactory.Instance);

            // Act
            var exception = await Assert.ThrowsAsync<RequestMethodNotAllowedException>(() => controller.GetSecondaryAsync(id, string.Empty));

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Error.StatusCode);
            Assert.Equal(HttpMethod.Get, exception.Method);
        }

        [Fact]
        public async Task PatchAsync_Calls_Service()
        {
            // Arrange
            const int id = 0;
            var resource = new Resource();
            var serviceMock = new Mock<IUpdateService<Resource>>();

            var controller = new ResourceController(new JsonApiOptions(), NullLoggerFactory.Instance, update: serviceMock.Object);

            // Act
            await controller.PatchAsync(id, resource);

            // Assert
            serviceMock.Verify(m => m.UpdateAsync(id, It.IsAny<Resource>()), Times.Once);
        }

        [Fact]
        public async Task PatchAsync_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, NullLoggerFactory.Instance);

            // Act
            var exception = await Assert.ThrowsAsync<RequestMethodNotAllowedException>(() => controller.PatchAsync(id, It.IsAny<Resource>()));

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Error.StatusCode);
            Assert.Equal(HttpMethod.Patch, exception.Method);
        }

        [Fact]
        public async Task PostAsync_Calls_Service()
        {
            // Arrange
            var resource = new Resource();
            var serviceMock = new Mock<ICreateService<Resource>>();

            var controller = new ResourceController(new JsonApiOptions(), NullLoggerFactory.Instance, create: serviceMock.Object);
            serviceMock.Setup(m => m.CreateAsync(It.IsAny<Resource>())).ReturnsAsync(resource);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act
            await controller.PostAsync(resource);

            // Assert
            serviceMock.Verify(m => m.CreateAsync(It.IsAny<Resource>()), Times.Once);
        }

        [Fact]
        public async Task PatchRelationshipsAsync_Calls_Service()
        {
            // Arrange
            const int id = 0;
            var serviceMock = new Mock<IUpdateRelationshipService<Resource>>();
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, NullLoggerFactory.Instance, updateRelationship: serviceMock.Object);

            // Act
            await controller.PatchRelationshipAsync(id, string.Empty, null);

            // Assert
            serviceMock.Verify(m => m.UpdateRelationshipAsync(id, string.Empty, null), Times.Once);
        }

        [Fact]
        public async Task PatchRelationshipsAsync_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, NullLoggerFactory.Instance);

            // Act
            var exception = await Assert.ThrowsAsync<RequestMethodNotAllowedException>(() => controller.PatchRelationshipAsync(id, string.Empty, null));

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Error.StatusCode);
            Assert.Equal(HttpMethod.Patch, exception.Method);
        }

        [Fact]
        public async Task DeleteAsync_Calls_Service()
        {
            // Arrange
            const int id = 0;
            var serviceMock = new Mock<IDeleteService<Resource>>();
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, NullLoggerFactory.Instance, delete: serviceMock.Object);

            // Act
            await controller.DeleteAsync(id);

            // Assert
            serviceMock.Verify(m => m.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, NullLoggerFactory.Instance);

            // Act
            var exception = await Assert.ThrowsAsync<RequestMethodNotAllowedException>(() => controller.DeleteAsync(id));

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Error.StatusCode);
            Assert.Equal(HttpMethod.Delete, exception.Method);
        }
    }
}
