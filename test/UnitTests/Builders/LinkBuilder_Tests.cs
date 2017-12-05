using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace UnitTests
{
    public class LinkBuilder_Tests
    {
        [Theory]
        [InlineData("http", "localhost", "/api/v1/articles", false, "http://localhost/api/v1")]
        [InlineData("https", "localhost", "/api/v1/articles", false, "https://localhost/api/v1")]
        [InlineData("http", "example.com", "/api/v1/articles", false, "http://example.com/api/v1")]
        [InlineData("https", "example.com", "/api/v1/articles", false, "https://example.com/api/v1")]
        [InlineData("https", "example.com", "/articles", false, "https://example.com")]
        [InlineData("https", "example.com", "/articles", true, "")]
        [InlineData("https", "example.com", "/api/v1/articles", true, "/api/v1")]
        public void GetBasePath_Returns_Path_Before_Resource(string scheme,
            string host, string path, bool isRelative, string expectedPath)
        {
            // arrange
            const string resource = "articles";
            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.Setup(m => m.Options).Returns(new JsonApiOptions
            {
                RelativeLinks = isRelative
            });

            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(m => m.Scheme).Returns(scheme);
            requestMock.Setup(m => m.Host).Returns(new HostString(host));
            requestMock.Setup(m => m.Path).Returns(new PathString(path));

            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(m => m.Request).Returns(requestMock.Object);

            var linkBuilder = new LinkBuilder(jsonApiContextMock.Object);

            // act
            var basePath = linkBuilder.GetBasePath(contextMock.Object, resource);

            // assert
            Assert.Equal(expectedPath, basePath);
        }
    }
}
