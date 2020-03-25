using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Middleware
{
    public sealed class CurrentRequestMiddlewareTests
    {
        [Fact]
        public async Task ParseUrlBase_ObfuscatedIdClass_ShouldSetIdCorrectly()
        {
            // Arrange
            var id = "ABC123ABC";
            var configuration = GetConfiguration($"/obfuscatedIdModel/{id}", action: "GetAsync", id: id);
            var currentRequest = configuration.CurrentRequest;

            // Act
            await RunMiddlewareTask(configuration);

            // Assert
            Assert.Equal(id, currentRequest.BaseId);

        }
        [Fact]
        public async Task ParseUrlBase_UrlHasBaseIdSet_ShouldSetCurrentRequestWithSaidId()
        {
            // Arrange
            var id = "123";
            var configuration = GetConfiguration($"/users/{id}", id: id);
            var currentRequest = configuration.CurrentRequest;

            // Act
            await RunMiddlewareTask(configuration);

            // Assert
            Assert.Equal(id, currentRequest.BaseId);
        }

        [Fact]
        public async Task ParseUrlBase_UrlHasNoBaseIdSet_ShouldHaveBaseIdSetToNull()
        {
            // Arrange
            var configuration = GetConfiguration("/users");
            var currentRequest = configuration.CurrentRequest;

            // Act
            await RunMiddlewareTask(configuration);

            // Assert
            Assert.Null(currentRequest.BaseId);
        }

        [Fact]
        public async Task ParseUrlBase_UrlHasNegativeBaseIdAndTypeIsInt_ShouldNotThrowJAException()
        {
            // Arrange
            var configuration = GetConfiguration("/users/-5/");

            // Act / Assert
            await RunMiddlewareTask(configuration);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("", true)]
        public async Task ParseUrlBase_UrlHasIncorrectBaseIdSet_ShouldThrowException(string baseId, bool addSlash)
        {
            // Arrange
            var url = addSlash ? $"/users/{baseId}/" : $"/users/{baseId}";
            var configuration = GetConfiguration(url, id: baseId);

            // Act
            var task = RunMiddlewareTask(configuration);

            // Assert
            var exception = await Assert.ThrowsAsync<JsonApiException>(async () =>
            {
                await task;
            });
            Assert.Equal(400, exception.GetStatusCode());
            Assert.Contains(baseId, exception.Message);
        }

        private sealed class InvokeConfiguration
        {
            public CurrentRequestMiddleware MiddleWare;
            public HttpContext HttpContext;
            public Mock<IControllerResourceMapping> ControllerResourceMapping;
            public Mock<IJsonApiOptions> Options;
            public CurrentRequest CurrentRequest;
            public Mock<IResourceGraph> ResourceGraph;
        }
        private Task RunMiddlewareTask(InvokeConfiguration holder)
        {
            var controllerResourceMapping = holder.ControllerResourceMapping.Object;
            var context = holder.HttpContext;
            var options = holder.Options.Object;
            var currentRequest = holder.CurrentRequest;
            var resourceGraph = holder.ResourceGraph.Object;
            return holder.MiddleWare.Invoke(context, controllerResourceMapping, options, currentRequest, resourceGraph);
        }
        private InvokeConfiguration GetConfiguration(string path, string resourceName = "users", string action = "", string id =null, Type relType = null)
        {
            if (path.First() != '/')
            {
                throw new ArgumentException("Path should start with a '/'");
            }
            var middleware = new CurrentRequestMiddleware(httpContext =>
            {
                return Task.Run(() => Console.WriteLine("finished"));
            });
            var forcedNamespace = "api/v1";
            var mockMapping = new Mock<IControllerResourceMapping>();
            Mock<IJsonApiOptions> mockOptions = CreateMockOptions(forcedNamespace);
            var mockGraph = CreateMockResourceGraph(resourceName, includeRelationship: relType != null);
            var currentRequest = new CurrentRequest();
            if (relType != null)
            {
                currentRequest.RequestRelationship = new HasManyAttribute
                {
                    RightType = relType
                };
            }
            var context = CreateHttpContext(path, isRelationship: relType != null, action: action, id: id);
            return new InvokeConfiguration
            {
                MiddleWare = middleware,
                ControllerResourceMapping = mockMapping,
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

        private static DefaultHttpContext CreateHttpContext(string path, bool isRelationship = false, string action = "", string id =null)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = new PathString(path);
            context.Response.Body = new MemoryStream();
            var feature = new RouteValuesFeature
            {
                RouteValues = {["controller"] = "fake!", ["action"] = isRelationship ? "GetRelationship" : action}
            };
            if(id != null)
            {
                feature.RouteValues["id"] = id;
            }
            context.Features.Set<IRouteValuesFeature>(feature);
            return context;
        }

        private Mock<IResourceGraph> CreateMockResourceGraph( string resourceName, bool includeRelationship = false)
        {
            var mockGraph = new Mock<IResourceGraph>();
            var resourceContext = new ResourceContext
            {
                ResourceName = resourceName,
                IdentityType = typeof(string)
            };
             var seq = mockGraph.SetupSequence(d => d.GetResourceContext(It.IsAny<Type>())).Returns(resourceContext);
            if (includeRelationship)
            {
                var relResourceContext = new ResourceContext
                {
                    ResourceName = "todoItems",
                    IdentityType = typeof(string)
                };
                seq.Returns(relResourceContext);
            }
            return mockGraph;
        }

    }
}
