using System.Collections.Generic;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
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
            querySet.Filters=filters;

            _jsonApiContext
                .Setup(m => m.QuerySet)
                .Returns(querySet);

            var queryComposer = new QueryComposer();
            // act
            var filterString = queryComposer.Compose(_jsonApiContext.Object);
            // assert
            Assert.Equal("&filter[attribute]=eq:value", filterString);
        }

        [Fact]
        public void Can_ComposeLessThan_FilterStringForUrl()
        {
            // arrange
            var filter = new FilterQuery("attribute", "value", "le");
            var querySet = new QuerySet();
            List<FilterQuery> filters = new List<FilterQuery>();
            filters.Add(filter);
            querySet.Filters = filters;

            _jsonApiContext
                .Setup(m => m.QuerySet)
                .Returns(querySet);

            var queryComposer = new QueryComposer();
            // act
            var filterString = queryComposer.Compose(_jsonApiContext.Object);
            // assert
            Assert.Equal("&filter[attribute]=le:value", filterString);
        }

        [Fact]
        public void NoFilter_Compose_EmptyStringReturned()
        {
            // arrange
            var querySet = new QuerySet();

            _jsonApiContext
                .Setup(m => m.QuerySet)
                .Returns(querySet);

            var queryComposer = new QueryComposer();
            // act
            var filterString = queryComposer.Compose(_jsonApiContext.Object);
            // assert
            Assert.Equal("", filterString); 
        }
    }
}
