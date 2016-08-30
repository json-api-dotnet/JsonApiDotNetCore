using System;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Routing;
using JsonApiDotNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;

namespace JsonApiDotNetCoreTests.Routing.UnitTests
{
    public class RouterTests
    {
        [Fact]
        public void HandleJsonApiRoute_ReturnsFalse_IfTheRouteCannotBeBuilt()
        {
          //--> arrange
          var httpContextMock = new Mock<HttpContext>();
          httpContextMock.SetupAllProperties();

          // since this is an empty stub, IRouteBuilder.BuildFromRequest() will always return null
          var routeBuilderMock = new Mock<IRouteBuilder>();

          var router = new Router(null, routeBuilderMock.Object, null);

          //--> act
          var result = router.HandleJsonApiRoute(httpContextMock.Object, null);

          //--> assert
          Assert.False(result);
        }

        [Fact]
        public void HandleJsonApiRoute_CallsGetMethod_ForGetRequest()
        {
          //--> arrange
          var httpContextMock = new Mock<HttpContext>();
          httpContextMock.SetupAllProperties();
          var httpResponseMock = new Mock<HttpResponse>();
          httpResponseMock.Setup(r => r.Body).Returns(new MemoryStream());
          httpContextMock.Setup(c => c.Response).Returns(httpResponseMock.Object);

          var route = new Route(null, "GET", null, null);

          var routeBuilderMock = new Mock<IRouteBuilder>();
          routeBuilderMock.Setup(rb => rb.BuildFromRequest(null)).Returns(route);

          var serviceProviderMock = new Mock<IServiceProvider>();

          var controllerMock = new Mock<IJsonApiController>();
          controllerMock.Setup(c => c.Get()).Returns(new OkObjectResult(null));
          var controllerBuilder = new Mock<IControllerBuilder>();
          controllerBuilder.Setup(cb => cb.BuildController(It.IsAny<JsonApiContext>())).Returns(controllerMock.Object);

          var router = new Router(new JsonApiModelConfiguration(), routeBuilderMock.Object, controllerBuilder.Object);

          //--> act
          var result = router.HandleJsonApiRoute(httpContextMock.Object, serviceProviderMock.Object);

          //--> assert
          Assert.True(result);
          controllerMock.Verify(c => c.Get());
        }

        [Fact]
        public void HandleJsonApiRoute_CallsGetIdMethod_ForGetIdRequest()
        {
          //--> arrange
          const string resourceId = "1";
          var httpContextMock = new Mock<HttpContext>();
          httpContextMock.SetupAllProperties();
          var httpResponseMock = new Mock<HttpResponse>();
          httpResponseMock.Setup(r => r.Body).Returns(new MemoryStream());
          httpContextMock.Setup(c => c.Response).Returns(httpResponseMock.Object);

          var route = new Route(null, "GET", resourceId, null);

          var routeBuilderMock = new Mock<IRouteBuilder>();
          routeBuilderMock.Setup(rb => rb.BuildFromRequest(null)).Returns(route);

          var serviceProviderMock = new Mock<IServiceProvider>();

          var controllerMock = new Mock<IJsonApiController>();
          controllerMock.Setup(c => c.Get(resourceId)).Returns(new OkObjectResult(null));
          var controllerBuilder = new Mock<IControllerBuilder>();
          controllerBuilder.Setup(cb => cb.BuildController(It.IsAny<JsonApiContext>())).Returns(controllerMock.Object);

          var router = new Router(new JsonApiModelConfiguration(), routeBuilderMock.Object, controllerBuilder.Object);

          //--> act
          var result = router.HandleJsonApiRoute(httpContextMock.Object, serviceProviderMock.Object);

          //--> assert
          Assert.True(result);
          controllerMock.Verify(c => c.Get(resourceId));
        }
    }
}
