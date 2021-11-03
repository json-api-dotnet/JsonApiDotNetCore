using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Specialized;
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
        public async Task GetAsync_calls_service()
        {
            // Arrange
            var serviceMock = new Mock<IGetAllService<AccessResource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance, serviceMock.Object);

            // Act
            await controller.GetAsync(CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.GetAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetAsync_throws_405_if_service_unavailable()
        {
            // Arrange
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance, null!);

            // Act
            Func<Task> action = () => controller.GetAsync(CancellationToken.None);

            // Assert
            ExceptionAssertions<RequestMethodNotAllowedException> assertion = await action.Should().ThrowExactlyAsync<RequestMethodNotAllowedException>();
            RequestMethodNotAllowedException exception = assertion.Subject.Single();

            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
            exception.Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public async Task GetAsyncById_calls_service()
        {
            // Arrange
            const int id = 0;

            var serviceMock = new Mock<IGetByIdService<AccessResource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance, getById: serviceMock.Object);

            // Act
            await controller.GetAsync(id, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.GetAsync(id, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetAsyncById_throws_405_if_service_unavailable()
        {
            // Arrange
            const int id = 0;

            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance);

            // Act
            Func<Task> action = () => controller.GetAsync(id, CancellationToken.None);

            // Assert
            ExceptionAssertions<RequestMethodNotAllowedException> assertion = await action.Should().ThrowExactlyAsync<RequestMethodNotAllowedException>();
            RequestMethodNotAllowedException exception = assertion.Subject.Single();

            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
            exception.Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public async Task GetSecondaryAsync_calls_service()
        {
            // Arrange
            const int id = 0;
            const string relationshipName = "articles";

            var serviceMock = new Mock<IGetSecondaryService<AccessResource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance, getSecondary: serviceMock.Object);

            // Act
            await controller.GetSecondaryAsync(id, relationshipName, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.GetSecondaryAsync(id, relationshipName, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetSecondaryAsync_throws_405_if_service_unavailable()
        {
            // Arrange
            const int id = 0;
            const string relationshipName = "articles";

            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance);

            // Act
            Func<Task> action = () => controller.GetSecondaryAsync(id, relationshipName, CancellationToken.None);

            // Assert
            ExceptionAssertions<RequestMethodNotAllowedException> assertion = await action.Should().ThrowExactlyAsync<RequestMethodNotAllowedException>();
            RequestMethodNotAllowedException exception = assertion.Subject.Single();

            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
            exception.Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public async Task GetRelationshipAsync_calls_service()
        {
            // Arrange
            const int id = 0;
            const string relationshipName = "articles";

            var serviceMock = new Mock<IGetRelationshipService<AccessResource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance, getRelationship: serviceMock.Object);

            // Act
            await controller.GetRelationshipAsync(id, relationshipName, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.GetRelationshipAsync(id, relationshipName, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipAsync_throws_405_if_service_unavailable()
        {
            // Arrange
            const int id = 0;
            const string relationshipName = "articles";

            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance);

            // Act
            Func<Task> action = () => controller.GetRelationshipAsync(id, relationshipName, CancellationToken.None);

            // Assert
            ExceptionAssertions<RequestMethodNotAllowedException> assertion = await action.Should().ThrowExactlyAsync<RequestMethodNotAllowedException>();
            RequestMethodNotAllowedException exception = assertion.Subject.Single();

            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
            exception.Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public async Task PostAsync_calls_service()
        {
            // Arrange
            var resource = new AccessResource();

            var serviceMock = new Mock<ICreateService<AccessResource, int>>();
            serviceMock.Setup(service => service.CreateAsync(It.IsAny<AccessResource>(), It.IsAny<CancellationToken>())).ReturnsAsync(resource);
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();

            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance, create: serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            await controller.PostAsync(resource, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.CreateAsync(It.IsAny<AccessResource>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task PostAsync_throws_405_if_service_unavailable()
        {
            // Arrange
            var resource = new AccessResource();

            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance);

            // Act
            Func<Task> action = () => controller.PostAsync(resource, CancellationToken.None);

            // Assert
            ExceptionAssertions<RequestMethodNotAllowedException> assertion = await action.Should().ThrowExactlyAsync<RequestMethodNotAllowedException>();
            RequestMethodNotAllowedException exception = assertion.Subject.Single();

            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
            exception.Method.Should().Be(HttpMethod.Post);
        }

        [Fact]
        public async Task PostRelationshipAsync_calls_service()
        {
            // Arrange
            const int id = 0;
            const string relationshipName = "articles";
            var rightResourceIds = new HashSet<IIdentifiable>();

            var serviceMock = new Mock<IAddToRelationshipService<AccessResource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance, addToRelationship: serviceMock.Object);

            // Act
            await controller.PostRelationshipAsync(id, relationshipName, rightResourceIds, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.AddToToManyRelationshipAsync(id, relationshipName, rightResourceIds, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task PostRelationshipAsync_throws_405_if_service_unavailable()
        {
            // Arrange
            const int id = 0;
            const string relationshipName = "articles";
            var rightResourceIds = new HashSet<IIdentifiable>();

            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance);

            // Act
            Func<Task> action = () => controller.PostRelationshipAsync(id, relationshipName, rightResourceIds, CancellationToken.None);

            // Assert
            ExceptionAssertions<RequestMethodNotAllowedException> assertion = await action.Should().ThrowExactlyAsync<RequestMethodNotAllowedException>();
            RequestMethodNotAllowedException exception = assertion.Subject.Single();

            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
            exception.Method.Should().Be(HttpMethod.Post);
        }

        [Fact]
        public async Task PatchAsync_calls_service()
        {
            // Arrange
            const int id = 0;
            var resource = new AccessResource();

            var serviceMock = new Mock<IUpdateService<AccessResource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance, update: serviceMock.Object);

            // Act
            await controller.PatchAsync(id, resource, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.UpdateAsync(id, It.IsAny<AccessResource>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task PatchAsync_throws_405_if_service_unavailable()
        {
            // Arrange
            const int id = 0;
            var resource = new AccessResource();

            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance);

            // Act
            Func<Task> action = () => controller.PatchAsync(id, resource, CancellationToken.None);

            // Assert
            ExceptionAssertions<RequestMethodNotAllowedException> assertion = await action.Should().ThrowExactlyAsync<RequestMethodNotAllowedException>();
            RequestMethodNotAllowedException exception = assertion.Subject.Single();

            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
            exception.Method.Should().Be(HttpMethod.Patch);
        }

        [Fact]
        public async Task PatchRelationshipAsync_calls_service()
        {
            // Arrange
            const int id = 0;
            const string relationshipName = "articles";

            var serviceMock = new Mock<ISetRelationshipService<AccessResource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance, setRelationship: serviceMock.Object);

            // Act
            await controller.PatchRelationshipAsync(id, relationshipName, null, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.SetRelationshipAsync(id, relationshipName, null, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task PatchRelationshipAsync_throws_405_if_service_unavailable()
        {
            // Arrange
            const int id = 0;
            const string relationshipName = "articles";

            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance);

            // Act
            Func<Task> action = () => controller.PatchRelationshipAsync(id, relationshipName, null, CancellationToken.None);

            // Assert
            ExceptionAssertions<RequestMethodNotAllowedException> assertion = await action.Should().ThrowExactlyAsync<RequestMethodNotAllowedException>();
            RequestMethodNotAllowedException exception = assertion.Subject.Single();

            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
            exception.Method.Should().Be(HttpMethod.Patch);
        }

        [Fact]
        public async Task DeleteAsync_calls_service()
        {
            // Arrange
            const int id = 0;

            var serviceMock = new Mock<IDeleteService<AccessResource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance, delete: serviceMock.Object);

            // Act
            await controller.DeleteAsync(id, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.DeleteAsync(id, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_throws_405_if_service_unavailable()
        {
            // Arrange
            const int id = 0;

            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance);

            // Act
            Func<Task> action = () => controller.DeleteAsync(id, CancellationToken.None);

            // Assert
            ExceptionAssertions<RequestMethodNotAllowedException> assertion = await action.Should().ThrowExactlyAsync<RequestMethodNotAllowedException>();
            RequestMethodNotAllowedException exception = assertion.Subject.Single();

            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
            exception.Method.Should().Be(HttpMethod.Delete);
        }

        [Fact]
        public async Task DeleteRelationshipAsync_calls_service()
        {
            // Arrange
            const int id = 0;
            const string relationshipName = "articles";
            var rightResourceIds = new HashSet<IIdentifiable>();

            var serviceMock = new Mock<IRemoveFromRelationshipService<AccessResource, int>>();
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance, removeFromRelationship: serviceMock.Object);

            // Act
            await controller.DeleteRelationshipAsync(id, relationshipName, rightResourceIds, CancellationToken.None);

            // Assert
            serviceMock.Verify(service => service.RemoveFromToManyRelationshipAsync(id, relationshipName, rightResourceIds, CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task DeleteRelationshipAsync_throws_405_if_service_unavailable()
        {
            // Arrange
            const int id = 0;
            const string relationshipName = "articles";
            var rightResourceIds = new HashSet<IIdentifiable>();

            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Build();
            var controller = new AccessResourceController(options, resourceGraph, NullLoggerFactory.Instance);

            // Act
            Func<Task> action = () => controller.DeleteRelationshipAsync(id, relationshipName, rightResourceIds, CancellationToken.None);

            // Assert
            ExceptionAssertions<RequestMethodNotAllowedException> assertion = await action.Should().ThrowExactlyAsync<RequestMethodNotAllowedException>();
            RequestMethodNotAllowedException exception = assertion.Subject.Single();

            exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
            exception.Method.Should().Be(HttpMethod.Delete);
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public sealed class AccessResource : Identifiable<int>
        {
            [Attr]
            public string? TestAttribute { get; set; }
        }

        private sealed class AccessResourceController : BaseJsonApiController<AccessResource, int>
        {
            public AccessResourceController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
                IResourceService<AccessResource, int> resourceService)
                : base(options, resourceGraph, loggerFactory, resourceService)
            {
            }

            public AccessResourceController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
                IGetAllService<AccessResource, int>? getAll = null, IGetByIdService<AccessResource, int>? getById = null,
                IGetSecondaryService<AccessResource, int>? getSecondary = null, IGetRelationshipService<AccessResource, int>? getRelationship = null,
                ICreateService<AccessResource, int>? create = null, IAddToRelationshipService<AccessResource, int>? addToRelationship = null,
                IUpdateService<AccessResource, int>? update = null, ISetRelationshipService<AccessResource, int>? setRelationship = null,
                IDeleteService<AccessResource, int>? delete = null, IRemoveFromRelationshipService<AccessResource, int>? removeFromRelationship = null)
                : base(options, resourceGraph, loggerFactory, getAll, getById, getSecondary, getRelationship, create, addToRelationship, update,
                    setRelationship, delete, removeFromRelationship)
            {
            }
        }
    }
}
