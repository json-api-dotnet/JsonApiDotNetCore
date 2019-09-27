using System.Collections.Generic;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Services;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class QueryComposerTests
    {
        private readonly Mock<IJsonApiContext> _jsonApiContext;

        public QueryComposerTests()
        {
            _jsonApiContext = new Mock<IJsonApiContext>();
        }

        [Fact]
        public void Can_ComposeEqual_FilterStringForUrl()
        {
            // arrange
            var filter = new FilterQuery("attribute", "value", "eq");
            var querySet = new QuerySet();
            List<FilterQuery> filters = new List<FilterQuery>();
            filters.Add(filter);
            querySet.Filters = filters;

            var rmMock = new Mock<ICurrentRequest>();
            rmMock
                .Setup(m => m.QuerySet)
                .Returns(querySet);

            var queryComposer = new QueryComposer();
            // act
            var filterString = queryComposer.Compose(rmMock.Object);
            // assert
            Assert.Equal("&filter[attribute]=eq:value", filterString);
        }

        [Fact]
        public void Can_ComposeLessThan_FilterStringForUrl()
        {
            // arrange
            var filter = new FilterQuery("attribute", "value", "le");
            var filter2 = new FilterQuery("attribute2", "value2", "");
            var querySet = new QuerySet();
            List<FilterQuery> filters = new List<FilterQuery>();
            filters.Add(filter);
            filters.Add(filter2);
            querySet.Filters = filters;
            var rmMock = new Mock<ICurrentRequest>();
            rmMock
                .Setup(m => m.QuerySet)
                .Returns(querySet);


            var queryComposer = new QueryComposer();
            // act
            var filterString = queryComposer.Compose(rmMock.Object);
            // assert
            Assert.Equal("&filter[attribute]=le:value&filter[attribute2]=value2", filterString);
        }

        [Fact]
        public void NoFilter_Compose_EmptyStringReturned()
        {
            // arrange
            var querySet = new QuerySet();

            var rmMock = new Mock<ICurrentRequest>();
            rmMock
                .Setup(m => m.QuerySet)
                .Returns(querySet);

            var queryComposer = new QueryComposer();
            // Act

            var filterString = queryComposer.Compose(rmMock.Object);
            // assert
            Assert.Equal("", filterString);
        }
    }
}
