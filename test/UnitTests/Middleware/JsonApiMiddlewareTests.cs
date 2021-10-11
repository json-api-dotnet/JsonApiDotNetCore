#nullable disable

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace UnitTests.Middleware
{
    public sealed class JsonApiMiddlewareTests
    {
        [Fact]
        public async Task ParseUrlBase_ObfuscatedIdClass_ShouldSetIdCorrectly()
        {
            // Arrange
            const string id = "ABC123ABC";
            InvokeConfiguration configuration = GetConfiguration($"/obfuscatedIdModel/{id}", action: "GetAsync", id: id);
            JsonApiRequest request = configuration.Request;

            // Act
            await RunMiddlewareTask(configuration);

            // Assert
            Assert.Equal(id, request.PrimaryId);
        }

        [Fact]
        public async Task ParseUrlBase_UrlHasPrimaryIdSet_ShouldSetupRequestWithSameId()
        {
            // Arrange
            const string id = "123";
            InvokeConfiguration configuration = GetConfiguration($"/users/{id}", id: id);
            JsonApiRequest request = configuration.Request;

            // Act
            await RunMiddlewareTask(configuration);

            // Assert
            Assert.Equal(id, request.PrimaryId);
        }

        [Fact]
        public async Task ParseUrlBase_UrlHasNoPrimaryIdSet_ShouldHaveBaseIdSetToNull()
        {
            // Arrange
            InvokeConfiguration configuration = GetConfiguration("/users");
            JsonApiRequest request = configuration.Request;

            // Act
            await RunMiddlewareTask(configuration);

            // Assert
            Assert.Null(request.PrimaryId);
        }

        [Fact]
        public async Task ParseUrlBase_UrlHasNegativePrimaryIdAndTypeIsInt_ShouldNotThrowJAException()
        {
            // Arrange
            InvokeConfiguration configuration = GetConfiguration("/users/-5/");

            // Act
            Func<Task> asyncAction = async () => await RunMiddlewareTask(configuration);

            // Assert
            await asyncAction();
        }

        private Task RunMiddlewareTask(InvokeConfiguration holder)
        {
            IControllerResourceMapping controllerResourceMapping = holder.ControllerResourceMapping.Object;
            HttpContext httpContext = holder.HttpContext;
            IJsonApiOptions options = holder.Options;
            JsonApiRequest request = holder.Request;

            return holder.MiddleWare.InvokeAsync(httpContext, controllerResourceMapping, options, request, NullLogger<JsonApiMiddleware>.Instance);
        }

        private InvokeConfiguration GetConfiguration(string path, string resourceName = "users", string action = "", string id = null, Type relType = null)
        {
            if (path.First() != '/')
            {
                throw new ArgumentException("Path should start with a '/'");
            }

            var middleware = new JsonApiMiddleware(_ =>
            {
                return Task.Run(() => Console.WriteLine("finished"));
            }, new HttpContextAccessor());

            const string forcedNamespace = "api/v1";
            var mockMapping = new Mock<IControllerResourceMapping>();
            var resourceType = new ResourceType(resourceName, typeof(object), typeof(string));
            mockMapping.Setup(mapping => mapping.GetResourceTypeForController(It.IsAny<Type>())).Returns(resourceType);

            IJsonApiOptions options = CreateOptions(forcedNamespace);
            var request = new JsonApiRequest();

            if (relType != null)
            {
                request.Relationship = new HasManyAttribute();
            }

            DefaultHttpContext httpContext = CreateHttpContext(path, relType != null, action, id);

            return new InvokeConfiguration
            {
                MiddleWare = middleware,
                ControllerResourceMapping = mockMapping,
                Options = options,
                Request = request,
                HttpContext = httpContext
            };
        }

        private static IJsonApiOptions CreateOptions(string forcedNamespace)
        {
            var options = new JsonApiOptions
            {
                Namespace = forcedNamespace
            };

            return options;
        }

        private static DefaultHttpContext CreateHttpContext(string path, bool isRelationship = false, string action = "", string id = null)
        {
            var httpContext = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString(path)
                },
                Response =
                {
                    Body = new MemoryStream()
                }
            };

            var feature = new RouteValuesFeature
            {
                RouteValues =
                {
                    ["controller"] = "fake!",
                    ["action"] = isRelationship ? "GetRelationship" : action
                }
            };

            if (id != null)
            {
                feature.RouteValues["id"] = id;
            }

            httpContext.Features.Set<IRouteValuesFeature>(feature);

            var controllerActionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = (TypeInfo)typeof(object)
            };

            httpContext.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(controllerActionDescriptor), null));
            return httpContext;
        }

        private sealed class InvokeConfiguration
        {
            public JsonApiMiddleware MiddleWare { get; init; }
            public HttpContext HttpContext { get; init; }
            public Mock<IControllerResourceMapping> ControllerResourceMapping { get; init; }
            public IJsonApiOptions Options { get; init; }
            public JsonApiRequest Request { get; init; }
        }
    }
}
