using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class QueryParser_Tests
    {
        private readonly Mock<IControllerContext> _controllerContextMock;
        private readonly Mock<IQueryCollection> _queryCollectionMock;

        public QueryParser_Tests()
        {
            _controllerContextMock = new Mock<IControllerContext>();
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

            _controllerContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.None));

            var queryParser = new QueryParser(_controllerContextMock.Object, new JsonApiOptions());

            // act
            var querySet = queryParser.Parse(_queryCollectionMock.Object);

            // assert
            Assert.Equal("value", querySet.Filters.Single(f => f.Attribute == "key").Value);
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

            _controllerContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.None));

            var queryParser = new QueryParser(_controllerContextMock.Object, new JsonApiOptions());

            // act
            var querySet = queryParser.Parse(_queryCollectionMock.Object);

            // assert
            Assert.Equal(dt, querySet.Filters.Single(f => f.Attribute == "key").Value);
            Assert.Equal("le", querySet.Filters.Single(f => f.Attribute == "key").Operation);
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

            _controllerContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.None));

            var queryParser = new QueryParser(_controllerContextMock.Object, new JsonApiOptions());

            // act
            var querySet = queryParser.Parse(_queryCollectionMock.Object);

            // assert
            Assert.Equal(dt, querySet.Filters.Single(f => f.Attribute == "key").Value);
            Assert.Equal(string.Empty, querySet.Filters.Single(f => f.Attribute == "key").Operation);
            Assert.Equal(FilterOperations.eq, querySet.Filters.Single(f => f.Attribute == "key").OperationType);
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

            _controllerContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.Filter));

            var queryParser = new QueryParser(_controllerContextMock.Object, new JsonApiOptions());

            // act
            var querySet = queryParser.Parse(_queryCollectionMock.Object);

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

            _controllerContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.Sort));

            var queryParser = new QueryParser(_controllerContextMock.Object, new JsonApiOptions());

            // act
            var querySet = queryParser.Parse(_queryCollectionMock.Object);

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

            _controllerContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.Include));

            var queryParser = new QueryParser(_controllerContextMock.Object, new JsonApiOptions());

            // act
            var querySet = queryParser.Parse(_queryCollectionMock.Object);

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

            _controllerContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.Page));

            var queryParser = new QueryParser(_controllerContextMock.Object, new JsonApiOptions());

            // act
            var querySet = queryParser.Parse(_queryCollectionMock.Object);

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

            _controllerContextMock
                .Setup(m => m.GetControllerAttribute<DisableQueryAttribute>())
                .Returns(new DisableQueryAttribute(QueryParams.Fields));

            var queryParser = new QueryParser(_controllerContextMock.Object, new JsonApiOptions());

            // act
            var querySet = queryParser.Parse(_queryCollectionMock.Object);

            // assert
            Assert.Empty(querySet.Fields);
        }

        [Fact]
        public void Can_Parse_Fields_Query()
        {
            // arrange
            const string type = "articles";
            const string attrName = "some-field";
            const string internalAttrName = "SomeField";

            var query = new Dictionary<string, StringValues> { { $"fields[{type}]", new StringValues(attrName) } };

            _queryCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(query.GetEnumerator());

            _controllerContextMock
                .Setup(m => m.RequestEntity)
                .Returns(new ContextEntity
                {
                    EntityName = type,
                        Attributes = new List<AttrAttribute>
                        {
                            new AttrAttribute(attrName)
                            {
                                InternalAttributeName = internalAttrName
                            }
                        }
                });

            var queryParser = new QueryParser(_controllerContextMock.Object, new JsonApiOptions());

            // act
            var querySet = queryParser.Parse(_queryCollectionMock.Object);

            // assert
            Assert.NotEmpty(querySet.Fields);
            Assert.Equal(2, querySet.Fields.Count);
            Assert.Equal("Id", querySet.Fields[0]);
            Assert.Equal(internalAttrName, querySet.Fields[1]);
        }

        [Fact]
        public void Throws_JsonApiException_If_Field_DoesNotExist()
        {
            // arrange
            const string type = "articles";
            const string attrName = "dne";

            var query = new Dictionary<string, StringValues> { { $"fields[{type}]", new StringValues(attrName) } };

            _queryCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(query.GetEnumerator());

            _controllerContextMock
                .Setup(m => m.RequestEntity)
                .Returns(new ContextEntity
                {
                    EntityName = type,
                        Attributes = new List<AttrAttribute>()
                });

            var queryParser = new QueryParser(_controllerContextMock.Object, new JsonApiOptions());

            // act , assert
            var ex = Assert.Throws<JsonApiException>(() => queryParser.Parse(_queryCollectionMock.Object));
            Assert.Equal(400, ex.GetStatusCode());
        }

        [Theory]
        [InlineData("1", 1, false)]
        [InlineData("abcde", 0, true)]
        [InlineData("", 0, true)]
        public void Can_Parse_Page_Size_Query(string value, int expectedValue, bool shouldThrow)
        {
            // arrange
            var query = new Dictionary<string, StringValues>
                { { "page[size]", new StringValues(value) }
                };

            _queryCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(query.GetEnumerator());

            var queryParser = new QueryParser(_controllerContextMock.Object, new JsonApiOptions());

            // act
            if (shouldThrow)
            {
                var ex = Assert.Throws<JsonApiException>(() => queryParser.Parse(_queryCollectionMock.Object));
                Assert.Equal(400, ex.GetStatusCode());
            }
            else
            {
                var querySet = queryParser.Parse(_queryCollectionMock.Object);
                Assert.Equal(expectedValue, querySet.PageQuery.PageSize);
            }
        }

        [Theory]
        [InlineData("1", 1, false)]
        [InlineData("abcde", 0, true)]
        [InlineData("", 0, true)]
        public void Can_Parse_Page_Number_Query(string value, int expectedValue, bool shouldThrow)
        {
            // arrange
            var query = new Dictionary<string, StringValues>
                { { "page[number]", new StringValues(value) }
                };

            _queryCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(query.GetEnumerator());

            var queryParser = new QueryParser(_controllerContextMock.Object, new JsonApiOptions());

            // act
            if (shouldThrow)
            {
                var ex = Assert.Throws<JsonApiException>(() => queryParser.Parse(_queryCollectionMock.Object));
                Assert.Equal(400, ex.GetStatusCode());
            }
            else
            {
                var querySet = queryParser.Parse(_queryCollectionMock.Object);
                Assert.Equal(expectedValue, querySet.PageQuery.PageOffset);
            }
        }
    }
}
