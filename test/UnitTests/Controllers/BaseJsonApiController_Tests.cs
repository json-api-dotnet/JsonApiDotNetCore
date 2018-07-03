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

namespace UnitTests
{
    public class BaseJsonApiController_Tests
    {
        public class Resource : Identifiable { }
        private Mock<IJsonApiContext> _jsonApiContextMock = new Mock<IJsonApiContext>();

        [Fact]
        public async Task GetAsync_Calls_Service()
        {
            // arrange
            var serviceMock = new Mock<IGetAllService<Resource>>();
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, getAll: serviceMock.Object);

            // act
            await controller.GetAsync();

            // assert
            serviceMock.Verify(m => m.GetAsync(), Times.Once);
            VerifyApplyContext();
        }

        [Fact]
        public async Task GetAsync_Throws_405_If_No_Service()
        {
            // arrange
            var serviceMock = new Mock<IGetAllService<Resource>>();
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, null);

            // act
            var exception = await Assert.ThrowsAsync<JsonApiException>(() => controller.GetAsync());

            // assert
            Assert.Equal(405, exception.GetStatusCode());
        }

        [Fact]
        public async Task GetAsyncById_Calls_Service()
        {
            // arrange
            const int id = 0;
            var serviceMock = new Mock<IGetByIdService<Resource>>();
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, getById: serviceMock.Object);

            // act
            await controller.GetAsync(id);

            // assert
            serviceMock.Verify(m => m.GetAsync(id), Times.Once);
            VerifyApplyContext();
        }

        [Fact]
        public async Task GetAsyncById_Throws_405_If_No_Service()
        {
            // arrange
            const int id = 0;
            var serviceMock = new Mock<IGetByIdService<Resource>>();
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, getById: null);

            // act
            var exception = await Assert.ThrowsAsync<JsonApiException>(() => controller.GetAsync(id));

            // assert
            Assert.Equal(405, exception.GetStatusCode());
        }

        [Fact]
        public async Task GetRelationshipsAsync_Calls_Service()
        {
            // arrange
            const int id = 0;
            var serviceMock = new Mock<IGetRelationshipsService<Resource>>();
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, getRelationships: serviceMock.Object);

            // act
            await controller.GetRelationshipsAsync(id, string.Empty);

            // assert
            serviceMock.Verify(m => m.GetRelationshipsAsync(id, string.Empty), Times.Once);
            VerifyApplyContext();
        }

        [Fact]
        public async Task GetRelationshipsAsync_Throws_405_If_No_Service()
        {
            // arrange
            const int id = 0;
            var serviceMock = new Mock<IGetRelationshipsService<Resource>>();
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, getRelationships: null);

            // act
            var exception = await Assert.ThrowsAsync<JsonApiException>(() => controller.GetRelationshipsAsync(id, string.Empty));

            // assert
            Assert.Equal(405, exception.GetStatusCode());
        }

        [Fact]
        public async Task GetRelationshipAsync_Calls_Service()
        {
            // arrange
            const int id = 0;
            var serviceMock = new Mock<IGetRelationshipService<Resource>>();
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, getRelationship: serviceMock.Object);

            // act
            await controller.GetRelationshipAsync(id, string.Empty);

            // assert
            serviceMock.Verify(m => m.GetRelationshipAsync(id, string.Empty), Times.Once);
            VerifyApplyContext();
        }

        [Fact]
        public async Task GetRelationshipAsync_Throws_405_If_No_Service()
        {
            // arrange
            const int id = 0;
            var serviceMock = new Mock<IGetRelationshipService<Resource>>();
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, getRelationship: null);

            // act
            var exception = await Assert.ThrowsAsync<JsonApiException>(() => controller.GetRelationshipAsync(id, string.Empty));

            // assert
            Assert.Equal(405, exception.GetStatusCode());
        }

        [Fact]
        public async Task PatchAsync_Calls_Service()
        {
            // arrange
            const int id = 0;
            var resource = new Resource();
            var serviceMock = new Mock<IUpdateService<Resource>>();
            _jsonApiContextMock.Setup(a => a.ApplyContext<Resource>(It.IsAny<BaseJsonApiController<Resource>>())).Returns(_jsonApiContextMock.Object);
            _jsonApiContextMock.SetupGet(a => a.Options).Returns(new JsonApiOptions());
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, update: serviceMock.Object);

            // act
            await controller.PatchAsync(id, resource);

            // assert
            serviceMock.Verify(m => m.UpdateAsync(id, It.IsAny<Resource>()), Times.Once);
            VerifyApplyContext();
        }

        [Fact]
        public async Task PatchAsync_ModelStateInvalid_ValidateModelStateDisbled()
        {
            // arrange
            const int id = 0;
            var resource = new Resource();
            var serviceMock = new Mock<IUpdateService<Resource>>();
            _jsonApiContextMock.Setup(a => a.ApplyContext<Resource>(It.IsAny<BaseJsonApiController<Resource>>())).Returns(_jsonApiContextMock.Object);
            _jsonApiContextMock.SetupGet(a => a.Options).Returns(new JsonApiOptions { ValidateModelState = false });
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, update: serviceMock.Object);

            // act
            var response = await controller.PatchAsync(id, resource);

            // assert
            serviceMock.Verify(m => m.UpdateAsync(id, It.IsAny<Resource>()), Times.Once);
            VerifyApplyContext();
            Assert.IsNotType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task PatchAsync_ModelStateInvalid_ValidateModelStateEnabled()
        {
            // arrange
            const int id = 0;
            var resource = new Resource();
            var serviceMock = new Mock<IUpdateService<Resource>>();
            _jsonApiContextMock.Setup(a => a.ApplyContext<Resource>(It.IsAny<BaseJsonApiController<Resource>>())).Returns(_jsonApiContextMock.Object);
            _jsonApiContextMock.SetupGet(a => a.Options).Returns(new JsonApiOptions{ValidateModelState = true});
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, update: serviceMock.Object);
            controller.ModelState.AddModelError("Id", "Failed Validation");

            // act
            var response = await controller.PatchAsync(id, resource);

            // assert
            serviceMock.Verify(m => m.UpdateAsync(id, It.IsAny<Resource>()), Times.Never);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.IsType<ErrorCollection>(((BadRequestObjectResult) response).Value);
        }

        [Fact]
        public async Task PatchAsync_Throws_405_If_No_Service()
        {
            // arrange
            const int id = 0;
            var serviceMock = new Mock<IUpdateService<Resource>>();
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, update: null);

            // act
            var exception = await Assert.ThrowsAsync<JsonApiException>(() => controller.PatchAsync(id, It.IsAny<Resource>()));

            // assert
            Assert.Equal(405, exception.GetStatusCode());
        }

        [Fact]
        public async Task PostAsync_Calls_Service()
        {
            // arrange
            var resource = new Resource();
            var serviceMock = new Mock<ICreateService<Resource>>();
            _jsonApiContextMock.Setup(a => a.ApplyContext<Resource>(It.IsAny<BaseJsonApiController<Resource>>())).Returns(_jsonApiContextMock.Object);
            _jsonApiContextMock.SetupGet(a => a.Options).Returns(new JsonApiOptions());
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, create: serviceMock.Object);
            serviceMock.Setup(m => m.CreateAsync(It.IsAny<Resource>())).ReturnsAsync(resource);
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext {HttpContext = new DefaultHttpContext()};

            // act
            await controller.PostAsync(resource);

            // assert
            serviceMock.Verify(m => m.CreateAsync(It.IsAny<Resource>()), Times.Once);
            VerifyApplyContext();
        }

        [Fact]
        public async Task PostAsync_ModelStateInvalid_ValidateModelStateDisabled()
        {
            // arrange
            var resource = new Resource();
            var serviceMock = new Mock<ICreateService<Resource>>();
            _jsonApiContextMock.Setup(a => a.ApplyContext<Resource>(It.IsAny<BaseJsonApiController<Resource>>())).Returns(_jsonApiContextMock.Object);
            _jsonApiContextMock.SetupGet(a => a.Options).Returns(new JsonApiOptions { ValidateModelState = false });
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, create: serviceMock.Object);
            serviceMock.Setup(m => m.CreateAsync(It.IsAny<Resource>())).ReturnsAsync(resource);
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = new DefaultHttpContext() };

            // act
            var response = await controller.PostAsync(resource);

            // assert
            serviceMock.Verify(m => m.CreateAsync(It.IsAny<Resource>()), Times.Once);
            VerifyApplyContext();
            Assert.IsNotType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task PostAsync_ModelStateInvalid_ValidateModelStateEnabled()
        {
            // arrange
            var resource = new Resource();
            var serviceMock = new Mock<ICreateService<Resource>>();
            _jsonApiContextMock.Setup(a => a.ApplyContext<Resource>(It.IsAny<BaseJsonApiController<Resource>>())).Returns(_jsonApiContextMock.Object);
            _jsonApiContextMock.SetupGet(a => a.Options).Returns(new JsonApiOptions { ValidateModelState = true });
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, create: serviceMock.Object);
            controller.ModelState.AddModelError("Id", "Failed Validation");

            // act
            var response = await controller.PostAsync(resource);

            // assert
            serviceMock.Verify(m => m.CreateAsync(It.IsAny<Resource>()), Times.Never);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.IsType<ErrorCollection>(((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public async Task PatchRelationshipsAsync_Calls_Service()
        {
            // arrange
            const int id = 0;
            var resource = new Resource();
            var serviceMock = new Mock<IUpdateRelationshipService<Resource>>();
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, updateRelationships: serviceMock.Object);

            // act
            await controller.PatchRelationshipsAsync(id, string.Empty, null);

            // assert
            serviceMock.Verify(m => m.UpdateRelationshipsAsync(id, string.Empty, null), Times.Once);
            VerifyApplyContext();
        }

        [Fact]
        public async Task PatchRelationshipsAsync_Throws_405_If_No_Service()
        {
            // arrange
            const int id = 0;
            var serviceMock = new Mock<IUpdateRelationshipService<Resource>>();
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, updateRelationships: null);

            // act
            var exception = await Assert.ThrowsAsync<JsonApiException>(() => controller.PatchRelationshipsAsync(id, string.Empty, null));

            // assert
            Assert.Equal(405, exception.GetStatusCode());
        }

        [Fact]
        public async Task DeleteAsync_Calls_Service()
        {
            // arrange
            const int id = 0;
            var resource = new Resource();
            var serviceMock = new Mock<IDeleteService<Resource>>();
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, delete: serviceMock.Object);

            // act
            await controller.DeleteAsync(id);

            // assert
            serviceMock.Verify(m => m.DeleteAsync(id), Times.Once);
            VerifyApplyContext();
        }

        [Fact]
        public async Task DeleteAsync_Throws_405_If_No_Service()
        {
            // arrange
            const int id = 0;
            var serviceMock = new Mock<IUpdateRelationshipService<Resource>>();
            var controller = new BaseJsonApiController<Resource>(_jsonApiContextMock.Object, delete: null);

            // act
            var exception = await Assert.ThrowsAsync<JsonApiException>(() => controller.DeleteAsync(id));

            // assert
            Assert.Equal(405, exception.GetStatusCode());
        }

        private void VerifyApplyContext()
         => _jsonApiContextMock.Verify(m => m.ApplyContext<Resource>(It.IsAny<BaseJsonApiController<Resource>>()), Times.Once);
    }
}
