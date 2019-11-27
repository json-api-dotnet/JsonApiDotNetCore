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

            mockGraph.Setup(d => d.GetResourceContext(It.IsAny<Type>())).Returns(resourceContext);
            // Act
            await middleware.Invoke(context, mockMapping.Object, mockOptions.Object, currentRequest, mockGraph.Object);

            // Assert
            Assert.Equal(id.ToString(), currentRequest.BaseId);
        }
    }
}
