using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Middleware
{
    public class CurrentRequestMiddlewareTests
    {
        [Fact]
        public async Task ParseUrl_UrlHasBaseIdSet_ShouldSetCurrentRequestWithSaidId()
        {
            // Arrange
            var middleware = new CurrentRequestMiddleware((context) =>
            {
                return Task.Run(() => Console.WriteLine("finished"));
            });
            var mockMapping = new Mock<IControllerResourceMapping>();
            var mockOptions = new Mock<IJsonApiOptions>();
            var mockGraph = new Mock<IResourceGraph>();
            var currentRequest = new CurrentRequest();
            var context = new DefaultHttpContext();
            var id = 1231;
            context.Request.Path = new PathString($"/api/v1/users/{id}");
            context.Response.Body = new MemoryStream();
            var feature = new RouteValuesFeature();
            feature.RouteValues["controller"] = "fake!";
            feature.RouteValues["action"] = "noRel";
            context.Features.Set<IRouteValuesFeature>(feature);
            var resourceContext = new ResourceContext();
            resourceContext.ResourceName = "users";
            mockGraph.Setup(d => d.GetResourceContext(It.IsAny<Type>())).Returns(resourceContext);

            // Act
            await middleware.Invoke(context, mockMapping.Object, mockOptions.Object, currentRequest, mockGraph.Object);

            // Assert
            Assert.Equal(id.ToString(), currentRequest.BaseId);
        }

        [Fact]
        public async Task ParseUrl_UrlHasNoBaseIdSet_ShouldHaveBaseIdSetToNull()
        {
            // Arrange
            var middleware = new CurrentRequestMiddleware((context) =>
            {
                return Task.Run(() => Console.WriteLine("finished"));
            });
            var mockMapping = new Mock<IControllerResourceMapping>();
            var mockOptions = new Mock<IJsonApiOptions>();
            var mockGraph = new Mock<IResourceGraph>();
            var currentRequest = new CurrentRequest();
            var context = new DefaultHttpContext();
            var id = 1231;
            context.Request.Path = new PathString($"/api/v1/users");
            context.Response.Body = new MemoryStream();
            var feature = new RouteValuesFeature();
            feature.RouteValues["controller"] = "fake!";
            feature.RouteValues["action"] = "noRel";
            context.Features.Set<IRouteValuesFeature>(feature);
            var resourceContext = new ResourceContext
            {
                ResourceName = "users"
            };
            mockGraph.Setup(d => d.GetResourceContext(It.IsAny<Type>())).Returns(resourceContext);

            // Act
            await middleware.Invoke(context, mockMapping.Object, mockOptions.Object, currentRequest, mockGraph.Object);

            // Assert
            Assert.Null(currentRequest.BaseId);
        }
        [Fact]
        public async Task ParseUrl_UrlHasRelationshipIdSet_ShouldHaveBaseIdAndRelationshipIdSet()
        {
            // Arrange
            var middleware = new CurrentRequestMiddleware((context) =>
            {
                return Task.Run(() => Console.WriteLine("finished"));
            });
            var mockMapping = new Mock<IControllerResourceMapping>();
            var mockOptions = new Mock<IJsonApiOptions>();
            var mockGraph = new Mock<IResourceGraph>();
            var currentRequest = new CurrentRequest();
            var context = new DefaultHttpContext();
            var id = 1231;
            var relId = 7654;
            context.Request.Path = new PathString($"/api/v1/users/");
            context.Response.Body = new MemoryStream();
            var feature = new RouteValuesFeature();
            feature.RouteValues["controller"] = "fake!";
            feature.RouteValues["action"] = "noRel";
            context.Features.Set<IRouteValuesFeature>(feature);
            var resourceContext = new ResourceContext
            {
                ResourceName = "users"
            };
            mockGraph.Setup(d => d.GetResourceContext(It.IsAny<Type>())).Returns(resourceContext);

            // Act
            await middleware.Invoke(context, mockMapping.Object, mockOptions.Object, currentRequest, mockGraph.Object);

            // Assert
            Assert.Equal(id.ToString(), currentRequest.BaseId);
            Assert.Equal(relId.ToString(), currentRequest.RelationshipId);
        }

        [Theory]
        [InlineData("12315K", typeof(int), true)]
        [InlineData("12315K", typeof(int), false)]
        [InlineData("-5", typeof(int), true)]
        [InlineData("-5", typeof(int), false)]
        [InlineData("5", typeof(Guid), true)]
        [InlineData("5", typeof(Guid), false)]
        public async Task ParseUrl_UrlHasIncorrectBaseIdSet_ShouldThrowException(string baseId, Type idType, bool addSlash)
        {
            // Arrange
            var url = addSlash ? $"/users/{baseId}/" : $"/users/{baseId}";
            var configuration = Setup(url, idType: idType);

            // Act
            var task = PrepareTask(configuration);

            // Assert
            var exception = await Assert.ThrowsAsync<JsonApiException>(async () =>
            {
                await task;
            });
            Assert.Equal(500, exception.GetStatusCode());
            Assert.Contains(baseId, exception.Message);
        }

        class InvokeConfiguration
        {
            public CurrentRequestMiddleware MiddleWare;
            public HttpContext HttpContext;
            public Mock<IControllerResourceMapping> ControllerResourcemapping;
            public Mock<IJsonApiOptions> Options;
            public CurrentRequest CurrentRequest;
            public Mock<IResourceGraph> ResourceGraph;
        }
        private Task PrepareTask(InvokeConfiguration holder)
        {
            var controllerResourceMapping = holder.ControllerResourcemapping.Object;
            var context = holder.HttpContext;
            var options = holder.Options.Object;
            var currentRequest = holder.CurrentRequest;
            var resourceGraph = holder.ResourceGraph.Object;
            return holder.MiddleWare.Invoke(context, controllerResourceMapping, options, currentRequest, resourceGraph);
        }
        private InvokeConfiguration Setup(string path, Type idType = null)
        {
            idType ??= typeof(int);
            var middleware = new CurrentRequestMiddleware((context) =>
            {
                return Task.Run(() => Console.WriteLine("finished"));
            });
            var forcedNamespace = "api/v1";
            var mockMapping = new Mock<IControllerResourceMapping>();
            Mock<IJsonApiOptions> mockOptions = CreateMockOptions(forcedNamespace);
            var mockGraph = CreateMockResourceGraph();
            var currentRequest = new CurrentRequest();
            var context = CreateHttpContext(path);
            return new InvokeConfiguration
            {
                MiddleWare = middleware,
                ControllerResourcemapping = mockMapping,
                Options = mockOptions,
                CurrentRequest = currentRequest,
                HttpContext = context,
                ResourceGraph = mockGraph
            };
        }

        private static Mock<IJsonApiOptions> CreateMockOptions(string forcedNamespace)
        {
            var mockOptions = new Mock<IJsonApiOptions>();
            mockOptions.Setup(o => o.Namespace).Returns(forcedNamespace);
            return mockOptions;
        }

        private static DefaultHttpContext CreateHttpContext(string path)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = new PathString(path);
            context.Response.Body = new MemoryStream();
            var feature = new RouteValuesFeature();
            feature.RouteValues["controller"] = "fake!";
            feature.RouteValues["action"] = "noRel";
            context.Features.Set<IRouteValuesFeature>(feature);
            return context;
        }

        private Mock<IResourceGraph> CreateMockResourceGraph()
        {
            var mockGraph = new Mock<IResourceGraph>();
            var resourceContext = new ResourceContext
            {
                ResourceName = "users"
            };
            mockGraph.Setup(d => d.GetResourceContext(It.IsAny<Type>())).Returns(resourceContext);
            return mockGraph;
        }

    }
}
