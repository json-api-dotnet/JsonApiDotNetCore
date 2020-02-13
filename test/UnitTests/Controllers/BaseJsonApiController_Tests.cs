using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Moq;
using Xunit;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UnitTests
{
    public class BaseJsonApiController_Tests
    {
        public class Resource : Identifiable
        {
            [Attr] public string TestAttribute { get; set; }
        }

        public class ResourceController : BaseJsonApiController<Resource>
        {
            public ResourceController(
                IJsonApiOptions jsonApiOptions,
                ILoggerFactory loggerFactory,
                IResourceService<Resource, int> resourceService)
                : base(jsonApiOptions, loggerFactory, resourceService)
            { }

            public ResourceController(
                IJsonApiOptions jsonApiOptions,
                ILoggerFactory loggerFactory,
                IGetAllService<Resource, int> getAll = null,
                IGetByIdService<Resource, int> getById = null,
                IGetRelationshipService<Resource, int> getRelationship = null,
                IGetRelationshipsService<Resource, int> getRelationships = null,
                ICreateService<Resource, int> create = null,
                IUpdateService<Resource, int> update = null,
                IUpdateRelationshipService<Resource, int> updateRelationships = null,
                IDeleteService<Resource, int> delete = null)
                : base(jsonApiOptions, loggerFactory, getAll, getById, getRelationship, getRelationships, create,
                    update, updateRelationships, delete)
            { }
        }

        [Fact]
        public async Task GetAsync_Calls_Service()
        {
            // Arrange
            var serviceMock = new Mock<IGetAllService<Resource>>();
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, null, getAll: serviceMock.Object);

            // Act
            await controller.GetAsync();

            // Assert
            serviceMock.Verify(m => m.GetAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAsync_Throws_405_If_No_Service()
        {
            // Arrange
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, null, null);

            // Act
            var exception = await Assert.ThrowsAsync<JsonApiException>(() => controller.GetAsync());

            // Assert
            Assert.Equal(405, exception.GetStatusCode());
        }

        [Fact]
        public async Task GetAsyncById_Calls_Service()
        {
            // Arrange
            const int id = 0;
            var serviceMock = new Mock<IGetByIdService<Resource>>();
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, null, getById: serviceMock.Object);

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
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, null, getById: null);

            // Act
            var exception = await Assert.ThrowsAsync<JsonApiException>(() => controller.GetAsync(id));

            // Assert
            Assert.Equal(405, exception.GetStatusCode());
        }

        [Fact]
        public async Task GetRelationshipsAsync_Calls_Service()
        {
            // Arrange
            const int id = 0;
            var serviceMock = new Mock<IGetRelationshipsService<Resource>>();
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, null, getRelationships: serviceMock.Object);

            // Act
            await controller.GetRelationshipsAsync(id, string.Empty);

            // Assert
            serviceMock.Verify(m => m.GetRelationshipsAsync(id, string.Empty), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipsAsync_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, null, getRelationships: null);

            // Act
            var exception = await Assert.ThrowsAsync<JsonApiException>(() => controller.GetRelationshipsAsync(id, string.Empty));

            // Assert
            Assert.Equal(405, exception.GetStatusCode());
        }

        [Fact]
        public async Task GetRelationshipAsync_Calls_Service()
        {
            // Arrange
            const int id = 0;
            var serviceMock = new Mock<IGetRelationshipService<Resource>>();
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, null, getRelationship: serviceMock.Object);

            // Act
            await controller.GetRelationshipAsync(id, string.Empty);

            // Assert
            serviceMock.Verify(m => m.GetRelationshipAsync(id, string.Empty), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipAsync_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, null, getRelationship: null);

            // Act
            var exception = await Assert.ThrowsAsync<JsonApiException>(() => controller.GetRelationshipAsync(id, string.Empty));

            // Assert
            Assert.Equal(405, exception.GetStatusCode());
        }

        [Fact]
        public async Task PatchAsync_Calls_Service()
        {
            // Arrange
            const int id = 0;
            var resource = new Resource();
            var serviceMock = new Mock<IUpdateService<Resource>>();

            var controller = new ResourceController(new JsonApiOptions(), null, update: serviceMock.Object);

            // Act
            await controller.PatchAsync(id, resource);

            // Assert
            serviceMock.Verify(m => m.UpdateAsync(id, It.IsAny<Resource>()), Times.Once);
        }

        [Fact]
        public async Task PatchAsync_ModelStateInvalid_ValidateModelStateDisabled()
        {
            // Arrange
            const int id = 0;
            var resource = new Resource();
            var serviceMock = new Mock<IUpdateService<Resource>>();
            var controller = new ResourceController(new JsonApiOptions(), null, update: serviceMock.Object);

            // Act
            var response = await controller.PatchAsync(id, resource);

            // Assert
            serviceMock.Verify(m => m.UpdateAsync(id, It.IsAny<Resource>()), Times.Once);
            Assert.IsNotType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task PatchAsync_ModelStateInvalid_ValidateModelStateEnabled()
        {
            // Arrange
            const int id = 0;
            var resource = new Resource();
            var serviceMock = new Mock<IUpdateService<Resource>>();

            var controller = new ResourceController(new JsonApiOptions { ValidateModelState = true }, null, update: serviceMock.Object);
            controller.ModelState.AddModelError("TestAttribute", "Failed Validation");

            // Act
            var response = await controller.PatchAsync(id, resource);

            // Assert
            serviceMock.Verify(m => m.UpdateAsync(id, It.IsAny<Resource>()), Times.Never);
            Assert.IsType<UnprocessableEntityObjectResult>(response);
            Assert.IsType<ErrorCollection>(((UnprocessableEntityObjectResult)response).Value);
        }

        [Fact]
        public async Task PatchAsync_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, null, update: null);

            // Act
            var exception = await Assert.ThrowsAsync<JsonApiException>(() => controller.PatchAsync(id, It.IsAny<Resource>()));

            // Assert
            Assert.Equal(405, exception.GetStatusCode());
        }

        [Fact]
        public async Task PostAsync_Calls_Service()
        {
            // Arrange
            var resource = new Resource();
            var serviceMock = new Mock<ICreateService<Resource>>();

            var controller = new ResourceController(new JsonApiOptions(), null, create: serviceMock.Object);
            serviceMock.Setup(m => m.CreateAsync(It.IsAny<Resource>())).ReturnsAsync(resource);
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act
            await controller.PostAsync(resource);

            // Assert
            serviceMock.Verify(m => m.CreateAsync(It.IsAny<Resource>()), Times.Once);
        }

        [Fact]
        public async Task PostAsync_ModelStateInvalid_ValidateModelStateDisabled()
        {
            // Arrange
            var resource = new Resource();
            var serviceMock = new Mock<ICreateService<Resource>>();
            var controller = new ResourceController(new JsonApiOptions { ValidateModelState = false }, null, create: serviceMock.Object);
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = new DefaultHttpContext() };
            serviceMock.Setup(m => m.CreateAsync(It.IsAny<Resource>())).ReturnsAsync(resource);

            // Act
            var response = await controller.PostAsync(resource);

            // Assert
            serviceMock.Verify(m => m.CreateAsync(It.IsAny<Resource>()), Times.Once);
            Assert.IsNotType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task PostAsync_ModelStateInvalid_ValidateModelStateEnabled()
        {
            // Arrange
            var resource = new Resource();
            var serviceMock = new Mock<ICreateService<Resource>>();
            var controller = new ResourceController(new JsonApiOptions { ValidateModelState = true }, null, create: serviceMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            controller.ModelState.AddModelError("TestAttribute", "Failed Validation");
            serviceMock.Setup(m => m.CreateAsync(It.IsAny<Resource>())).ReturnsAsync(resource);

            // Act
            var response = await controller.PostAsync(resource);

            // Assert
            serviceMock.Verify(m => m.CreateAsync(It.IsAny<Resource>()), Times.Never);
            Assert.IsType<UnprocessableEntityObjectResult>(response);
            Assert.IsType<ErrorCollection>(((UnprocessableEntityObjectResult)response).Value);
        }

        [Fact]
        public async Task PatchRelationshipsAsync_Calls_Service()
        {
            // Arrange
            const int id = 0;
            var serviceMock = new Mock<IUpdateRelationshipService<Resource>>();
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, null, updateRelationships: serviceMock.Object);

            // Act
            await controller.PatchRelationshipsAsync(id, string.Empty, null);

            // Assert
            serviceMock.Verify(m => m.UpdateRelationshipsAsync(id, string.Empty, null), Times.Once);
        }

        [Fact]
        public async Task PatchRelationshipsAsync_Throws_405_If_No_Service()
        {
            // Arrange
            const int id = 0;
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, null, updateRelationships: null);

            // Act
            var exception = await Assert.ThrowsAsync<JsonApiException>(() => controller.PatchRelationshipsAsync(id, string.Empty, null));

            // Assert
            Assert.Equal(405, exception.GetStatusCode());
        }

        [Fact]
        public async Task DeleteAsync_Calls_Service()
        {
            // Arrange
            const int id = 0;
            var serviceMock = new Mock<IDeleteService<Resource>>();
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, null, delete: serviceMock.Object);

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
            var controller = new ResourceController(new Mock<IJsonApiOptions>().Object, null,
                delete: null);

            // Act
            var exception = await Assert.ThrowsAsync<JsonApiException>(() => controller.DeleteAsync(id));

            // Assert
            Assert.Equal(405, exception.GetStatusCode());
        }
    }
}
