using Xunit;
using Microsoft.AspNetCore.Http;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCoreTests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;

namespace JsonApiDotNetCoreTests.Middleware.UnitTests
{
    public class JsonApiMiddlewareTests
    {
        [Fact]
        public async void Invoke_CallsHandleJsonApiRequest_OnRouter()
        {
            // arrange
            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Path).Returns(new PathString(""));
            httpRequestMock.Setup(r => r.ContentType).Returns("application/vnd.api+json");
            httpRequestMock.Setup(r => r.ContentLength).Returns(0);
            var headers = new HeaderDictionary();
            headers.Add("Accept","application/vnd.api+json");
            httpRequestMock.Setup(r => r.Headers).Returns(headers);

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
        public async void Invoke_SetsStatusCode_To415_ForInvalidAcceptType()
        {
            // arrange
            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Path).Returns(new PathString(""));
            httpRequestMock.Setup(r => r.ContentType).Returns("application/vnd.api+json");
            httpRequestMock.Setup(r => r.ContentLength).Returns(0);
            var headers = new HeaderDictionary();
            headers.Add("Accept","");
            httpRequestMock.Setup(r => r.Headers).Returns(headers);

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

        [Fact]
        public async void Invoke_SetsStatusCode_To415_ForInvalidContentType()
        {
            // arrange
            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Path).Returns(new PathString(""));
            httpRequestMock.Setup(r => r.ContentType).Returns("");
            httpRequestMock.Setup(r => r.ContentLength).Returns(1);
            var headers = new HeaderDictionary();
            headers.Add("Accept","application/vnd.api+json");
            httpRequestMock.Setup(r => r.Headers).Returns(headers);

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
