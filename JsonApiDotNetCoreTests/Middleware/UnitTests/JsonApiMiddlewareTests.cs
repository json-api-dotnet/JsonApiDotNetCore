using Xunit;
using Microsoft.AspNetCore.Http;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCoreTests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;

namespace JsonApiDotNetCoreTests.Middleware.UnitTests
{
    // see example explanation on xUnit.net website:
    // https://xunit.github.io/docs/getting-started-dotnet-core.html
    public class JsonApiMiddlewareTests
    {
        [Fact]
        public async void Invoke_CallsHandleJsonApiRequest_OnRouter()
        {
            // arrange
            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Path).Returns(new PathString(""));
            httpRequestMock.Setup(r => r.ContentType).Returns("application/vnd.api+json");

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Request).Returns(httpRequestMock.Object);

            var router = new TestRouter();
            var loggerMock = new Mock<ILogger<JsonApiMiddleware>>();
            var middleware = new JsonApiMiddleware(null, loggerMock.Object, router, null);

            // act
            await middleware.Invoke(httpContextMock.Object);

            // assert
            Assert.True(router.DidHandleRoute);
        }

        [Fact]
        public async void Invoke_SetsStatusCode_To415_ForInvalidContentType()
        {
            // arrange
            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Path).Returns(new PathString(""));
            httpRequestMock.Setup(r => r.ContentType).Returns("");

            var httpResponsMock = new Mock<HttpResponse>();
            httpResponsMock.SetupAllProperties();
            httpResponsMock.Setup(r => r.Body).Returns(new MemoryStream());

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Request).Returns(httpRequestMock.Object);
            httpContextMock.Setup(c => c.Response).Returns(httpResponsMock.Object);

            var requestDelegateMock = new Mock<RequestDelegate>();

            var router = new TestRouter();
            var loggerMock = new Mock<ILogger<JsonApiMiddleware>>();
            var middleware = new JsonApiMiddleware(requestDelegateMock.Object, loggerMock.Object, router, null);

            // act
            await middleware.Invoke(httpContextMock.Object);

            // assert
            Assert.Equal(415, httpResponsMock.Object.StatusCode);
        }
    }
}
