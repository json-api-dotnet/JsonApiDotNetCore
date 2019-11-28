using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Middleware
{
    public class CurrentRequestMiddlewareTests
    {
        [Fact]
        public async Task ParseUrlBase_UrlHasBaseIdSet_ShouldSetCurrentRequestWithSaidId()
        {
            // Arrange
            var id = "123";
            var configuration = GetConfiguration($"/users/{id}");
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
        public async Task ParseUrlRel_UrlHasRelationshipIdSet_ShouldHaveBaseIdAndRelationshipIdSet()
        {
            // Arrange
            var baseId = "5";
            var relId = "23";
            var configuration = GetConfiguration($"/users/{baseId}/relationships/books/{relId}", relType: typeof(TodoItem), relIdType: typeof(int));
            var currentRequest = configuration.CurrentRequest;

            // Act
            await RunMiddlewareTask(configuration);

            // Assert
            Assert.Equal(baseId, currentRequest.BaseId);
            Assert.Equal(relId, currentRequest.RelationshipId);
        }

        [Fact]
        public async Task ParseUrlBase_UrlHasNegativeBaseIdAndTypeIsInt_ShouldThrowJAException()
        {
            // Arrange
            var configuration = GetConfiguration("/users/-5/");

            // Act
            var task = RunMiddlewareTask(configuration);

            // Assert
            var exception = await Assert.ThrowsAsync<JsonApiException>(async () =>
            {
                await task;
            });
            Assert.Equal(500, exception.GetStatusCode());
            Assert.Contains("negative", exception.Message);
        }

        [Theory]
        [InlineData("12315K", typeof(int), true)]
        [InlineData("12315K", typeof(int), false)]
        [InlineData("5", typeof(Guid), true)]
        [InlineData("5", typeof(Guid), false)]
        public async Task ParseUrlBase_UrlHasIncorrectBaseIdSet_ShouldThrowException(string baseId, Type idType, bool addSlash)
        {
            // Arrange
            var url = addSlash ? $"/users/{baseId}/" : $"/users/{baseId}";
            var configuration = GetConfiguration(url, idType: idType);

            // Act
            var task = RunMiddlewareTask(configuration);

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
        private Task RunMiddlewareTask(InvokeConfiguration holder)
        {
            var controllerResourceMapping = holder.ControllerResourcemapping.Object;
            var context = holder.HttpContext;
            var options = holder.Options.Object;
            var currentRequest = holder.CurrentRequest;
            var resourceGraph = holder.ResourceGraph.Object;
            return holder.MiddleWare.Invoke(context, controllerResourceMapping, options, currentRequest, resourceGraph);
        }
        private InvokeConfiguration GetConfiguration(string path, string resourceName = "users", Type idType = null, Type relType = null, Type relIdType = null)
        {
            if((relType != null) != (relIdType != null))
            {
                throw new ArgumentException("Define both reltype and relidType or dont.");
            }
            if (path.First() != '/')
            {
                throw new ArgumentException("Path should start with a '/'");
            }
            idType ??= typeof(int);
            var middleware = new CurrentRequestMiddleware((context) =>
            {
                return Task.Run(() => Console.WriteLine("finished"));
            });
            var forcedNamespace = "api/v1";
            var mockMapping = new Mock<IControllerResourceMapping>();
            Mock<IJsonApiOptions> mockOptions = CreateMockOptions(forcedNamespace);
            var mockGraph = CreateMockResourceGraph(idType, resourceName, relIdType : relIdType);
            var currentRequest = new CurrentRequest();
            if (relType != null && relIdType != null)
            {
                currentRequest.RequestRelationship = new HasManyAttribute
                {
                    RightType = relType
                };
            }
            var context = CreateHttpContext(path, isRelationship: relType != null);
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

        private static DefaultHttpContext CreateHttpContext(string path, bool isRelationship = false)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = new PathString(path);
            context.Response.Body = new MemoryStream();
            var feature = new RouteValuesFeature();
            feature.RouteValues["controller"] = "fake!";
            feature.RouteValues["action"] = isRelationship ? "relationships" : "noRel";
            context.Features.Set<IRouteValuesFeature>(feature);
            return context;
        }

        private Mock<IResourceGraph> CreateMockResourceGraph(Type idType, string resourceName, Type relIdType = null)
        {
            var mockGraph = new Mock<IResourceGraph>();
            var resourceContext = new ResourceContext
            {
                ResourceName = resourceName,
                IdentityType = idType
            };
             var seq = mockGraph.SetupSequence(d => d.GetResourceContext(It.IsAny<Type>())).Returns(resourceContext);
            if (relIdType != null)
            {
                var relResourceContext = new ResourceContext
                {
                    ResourceName = "todoItems",
                    IdentityType = relIdType
                };
                seq.Returns(relResourceContext);
            }
            return mockGraph;
        }

    }
}
