using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace UnitTests.Internal
{
    public class QuerySet_Tests
    {
        private readonly Mock<IJsonApiContext> _jsonApiContextMock;
        private readonly Mock<IQueryCollection> _queryCollectionMock;

        public QuerySet_Tests()
        {
            _jsonApiContextMock = new Mock<IJsonApiContext>();
            _queryCollectionMock = new Mock<IQueryCollection>();
        }

        [Fact]
        public void Can_Build_Filters()
        {
            // arrange
            var query = new Dictionary<string, StringValues> {
                { "filter[key]", new StringValues("value") }
            };

            _queryCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(query.GetEnumerator());

            _jsonApiContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.None));

            // act -- ctor calls BuildQuerySet()
            var querySet = new QuerySet(
                _jsonApiContextMock.Object,
                _queryCollectionMock.Object);

            // assert
            Assert.Equal("value", querySet.Filters.Single(f => f.Key == "Key").Value);
        }

        [Fact]
        public void Filters_Properly_Parses_DateTime_With_Operation()
        {
            // arrange
            const string dt = "2017-08-15T22:43:47.0156350-05:00";
            var query = new Dictionary<string, StringValues> {
                { "filter[key]", new StringValues("le:" + dt) }
            };

            _queryCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(query.GetEnumerator());

            _jsonApiContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.None));

            // act -- ctor calls BuildQuerySet()
            var querySet = new QuerySet(
                _jsonApiContextMock.Object,
                _queryCollectionMock.Object);

            // assert
            Assert.Equal(dt, querySet.Filters.Single(f => f.Key == "Key").Value);
            Assert.Equal("le", querySet.Filters.Single(f => f.Key == "Key").Operation);
        }

        [Fact]
        public void Filters_Properly_Parses_DateTime_Without_Operation()
        {
            // arrange
            const string dt = "2017-08-15T22:43:47.0156350-05:00";
            var query = new Dictionary<string, StringValues> {
                { "filter[key]", new StringValues(dt) }
            };

            _queryCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(query.GetEnumerator());

            _jsonApiContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.None));

            // act -- ctor calls BuildQuerySet()
            var querySet = new QuerySet(
                _jsonApiContextMock.Object,
                _queryCollectionMock.Object);

            // assert
            Assert.Equal(dt, querySet.Filters.Single(f => f.Key == "Key").Value);
            Assert.Equal(string.Empty, querySet.Filters.Single(f => f.Key == "Key").Operation);
        }

        [Fact]
        public void Can_Disable_Filters()
        {
            // arrange
            var query = new Dictionary<string, StringValues> {
                { "filter[key]", new StringValues("value") }
            };

            _queryCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(query.GetEnumerator());

            _jsonApiContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.Filter));

            // act -- ctor calls BuildQuerySet()
            var querySet = new QuerySet(
                _jsonApiContextMock.Object,
                _queryCollectionMock.Object);

            // assert
            Assert.Empty(querySet.Filters);
        }

        [Fact]
        public void Can_Disable_Sort()
        {
            // arrange
            var query = new Dictionary<string, StringValues> {
                { "sort", new StringValues("-key") }
            };

            _queryCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(query.GetEnumerator());

            _jsonApiContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.Sort));

            // act -- ctor calls BuildQuerySet()
            var querySet = new QuerySet(
                _jsonApiContextMock.Object,
                _queryCollectionMock.Object);

            // assert
            Assert.Empty(querySet.SortParameters);
        }

        [Fact]
        public void Can_Disable_Include()
        {
            // arrange
            var query = new Dictionary<string, StringValues> {
                { "include", new StringValues("key") }
            };

            _queryCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(query.GetEnumerator());

            _jsonApiContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.Include));

            // act -- ctor calls BuildQuerySet()
            var querySet = new QuerySet(
                _jsonApiContextMock.Object,
                _queryCollectionMock.Object);

            // assert
            Assert.Empty(querySet.IncludedRelationships);
        }

        [Fact]
        public void Can_Disable_Page()
        {
            // arrange
            var query = new Dictionary<string, StringValues> {
                { "page[size]", new StringValues("1") }
            };

            _queryCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(query.GetEnumerator());

            _jsonApiContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.Page));

            // act -- ctor calls BuildQuerySet()
            var querySet = new QuerySet(
                _jsonApiContextMock.Object,
                _queryCollectionMock.Object);

            // assert
            Assert.Equal(0, querySet.PageQuery.PageSize);
        }

        [Fact]
        public void Can_Disable_Fields()
        {
            // arrange
            var query = new Dictionary<string, StringValues> {
                { "fields", new StringValues("key") }
            };

            _queryCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(query.GetEnumerator());

            _jsonApiContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.Fields));

            // act -- ctor calls BuildQuerySet()
            var querySet = new QuerySet(
                _jsonApiContextMock.Object,
                _queryCollectionMock.Object);

            // assert
            Assert.Empty(querySet.Fields);
        }
    }
}
