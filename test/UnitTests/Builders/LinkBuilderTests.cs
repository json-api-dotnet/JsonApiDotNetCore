using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Managers.Contracts;
using Moq;
using Xunit;

namespace UnitTests
{
    public class LinkBuilderTests
    {
        private readonly Mock<IRequestManager> _requestManagerMock = new Mock<IRequestManager>();
        private const string _host = "http://www.example.com";


        public LinkBuilderTests()
        {
            _requestManagerMock.Setup(m => m.BasePath).Returns(_host);
            _requestManagerMock.Setup(m => m.GetContextEntity()).Returns(new ContextEntity { EntityName = "articles" });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetPageLink_GivenRelativeConfiguration_ReturnsExpectedPath(bool isRelative)
        {
            //arrange
            var options = new JsonApiOptions { RelativeLinks = isRelative };
            var linkBuilder = new LinkBuilder(options, _requestManagerMock.Object);
            var pageSize = 10;
            var pageOffset = 20;
            var expectedLink = $"/articles?page[size]={pageSize}&page[number]={pageOffset}";

            // act
            var link = linkBuilder.GetPageLink(pageOffset, pageSize);

            // assert
            if (isRelative)
            {
                Assert.Equal(expectedLink, link);
            } else
            {
                Assert.Equal(_host + expectedLink, link);
            }
        }

        /// todo: write tests for remaining linkBuilder methods
    }
}
