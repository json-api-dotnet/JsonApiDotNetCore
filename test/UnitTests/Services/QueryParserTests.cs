//using System.Collections.Generic;
//using System.Linq;
//using JsonApiDotNetCore.Configuration;
//using JsonApiDotNetCore.Controllers;
//using JsonApiDotNetCore.Internal;
//using JsonApiDotNetCore.Internal.Contracts;
//using JsonApiDotNetCore.Managers.Contracts;
//using JsonApiDotNetCore.Models;
//using JsonApiDotNetCore.Query;
//using JsonApiDotNetCore.Services;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Primitives;
//using Moq;
//using Xunit;

//namespace UnitTests.Services
//{
//    public class QueryParserTests
//    {
//        private readonly Mock<ICurrentRequest> _requestMock;
//        private readonly Mock<IQueryCollection> _queryCollectionMock;
//        private readonly Mock<IPageQueryService> _pageQueryMock;
//        private readonly ISparseFieldsService _sparseFieldsService = new Mock<ISparseFieldsService>().Object;
//        private readonly IIncludeService _includeService = new Mock<IIncludeService>().Object;
//        private readonly IContextEntityProvider _graph = new Mock<IContextEntityProvider>().Object;

//        public QueryParserTests()
//        {
//            _requestMock = new Mock<ICurrentRequest>();
//            _queryCollectionMock = new Mock<IQueryCollection>();
//            _pageQueryMock = new Mock<IPageQueryService>();
//        }

//        private QueryParser GetQueryParser()
//        {
//            return new QueryParser(new IncludeService(), _sparseFieldsService, _requestMock.Object, _graph, _pageQueryMock.Object, new JsonApiOptions());
//        }

//        [Fact]
//        public void Can_Build_Filters()
//        {
//            // arrange
//            var query = new Dictionary<string, StringValues> {
//                { "filter[key]", new StringValues("value") }
//            };

//            _queryCollectionMock
//                .Setup(m => m.GetEnumerator())
//                .Returns(query.GetEnumerator());

//            _requestMock
//                .Setup(m => m.DisabledQueryParams)
//                .Returns(QueryParams.None);

//            var queryParser = GetQueryParser();

//            // act
//            var querySet = queryParser.Parse(_queryCollectionMock.Object);

//            // assert
//            Assert.Equal("value", querySet.Filters.Single(f => f.Attribute == "key").Value);
//        }

//        [Fact]
//        public void Filters_Properly_Parses_DateTime_With_Operation()
//        {
//            // arrange
//            const string dt = "2017-08-15T22:43:47.0156350-05:00";
//            var query = new Dictionary<string, StringValues> {
//                { "filter[key]", new StringValues("le:" + dt) }
//            };

//            _queryCollectionMock
//                .Setup(m => m.GetEnumerator())
//                .Returns(query.GetEnumerator());

//            _requestMock
//                .Setup(m => m.DisabledQueryParams)
//                .Returns(QueryParams.None);

//            var queryParser = GetQueryParser();

//            // act
//            var querySet = queryParser.Parse(_queryCollectionMock.Object);

//            // assert
//            Assert.Equal(dt, querySet.Filters.Single(f => f.Attribute == "key").Value);
//            Assert.Equal("le", querySet.Filters.Single(f => f.Attribute == "key").Operation);
//        }

//        [Fact]
//        public void Filters_Properly_Parses_DateTime_Without_Operation()
//        {
//            // arrange
//            const string dt = "2017-08-15T22:43:47.0156350-05:00";
//            var query = new Dictionary<string, StringValues> {
//                { "filter[key]", new StringValues(dt) }
//            };

//            _queryCollectionMock
//                .Setup(m => m.GetEnumerator())
//                .Returns(query.GetEnumerator());

//            _requestMock
//                .Setup(m => m.DisabledQueryParams)
//                .Returns(QueryParams.None);

//            var queryParser = GetQueryParser();

//            // act
//            var querySet = queryParser.Parse(_queryCollectionMock.Object);

//            // assert
//            Assert.Equal(dt, querySet.Filters.Single(f => f.Attribute == "key").Value);
//            Assert.Equal(string.Empty, querySet.Filters.Single(f => f.Attribute == "key").Operation);
//        }

//        [Fact]
//        public void Can_Disable_Filters()
//        {
//            // Arrange
//            var query = new Dictionary<string, StringValues> {
//                { "filter[key]", new StringValues("value") }
//            };

//            _queryCollectionMock
//                .Setup(m => m.GetEnumerator())
//                .Returns(query.GetEnumerator());

//            _requestMock
//                .Setup(m => m.DisabledQueryParams)
//                .Returns(QueryParams.Filters);

//            var queryParser = GetQueryParser();

//            // Act
//            var querySet = queryParser.Parse(_queryCollectionMock.Object);

//            // Assert
//            Assert.Empty(querySet.Filters);
//        }
//        [Theory]
//        [InlineData("text,,1")]
//        [InlineData("text,hello,,5")]
//        [InlineData(",,2")]
//        public void Parse_EmptySortSegment_ReceivesJsonApiException(string stringSortQuery)
//        {
//            // Arrange
//            var query = new Dictionary<string, StringValues> {
//                { "sort", new StringValues(stringSortQuery) }
//            };

//            _queryCollectionMock
//                .Setup(m => m.GetEnumerator())
//                .Returns(query.GetEnumerator());

//            var queryParser = GetQueryParser();

//            // Act / Assert
//            var exception = Assert.Throws<JsonApiException>(() =>
//            {
//                var querySet = queryParser.Parse(_queryCollectionMock.Object);
//            });
//            Assert.Contains("sort", exception.Message);
//        }
//        [Fact]
//        public void Can_Disable_Sort()
//        {
//            // Arrange
//            var query = new Dictionary<string, StringValues> {
//                { "sort", new StringValues("-key") }
//            };

//            _queryCollectionMock
//                .Setup(m => m.GetEnumerator())
//                .Returns(query.GetEnumerator());

//            _requestMock
//                .Setup(m => m.DisabledQueryParams)
//                .Returns(QueryParams.Sort);

//            var queryParser = GetQueryParser();

//            // act
//            var querySet = queryParser.Parse(_queryCollectionMock.Object);

//            // assert
//            Assert.Empty(querySet.SortParameters);
//        }

//        [Fact]
//        public void Can_Disable_Page()
//        {
//            // arrange
//            var query = new Dictionary<string, StringValues> {
//                { "page[size]", new StringValues("1") }
//            };

//            _queryCollectionMock
//                .Setup(m => m.GetEnumerator())
//                .Returns(query.GetEnumerator());

//            _requestMock
//                .Setup(m => m.DisabledQueryParams)
//                .Returns(QueryParams.Page);

//            var queryParser = GetQueryParser();

//            // act
//            var querySet = queryParser.Parse(_queryCollectionMock.Object);

//            // assert
//            Assert.Equal(null, querySet.PageQuery.PageSize);
//        }

//        [Fact]
//        public void Can_Disable_Fields()
//        {
//            // arrange
//            var query = new Dictionary<string, StringValues> {
//                { "fields", new StringValues("key") }
//            };

//            _queryCollectionMock
//                .Setup(m => m.GetEnumerator())
//                .Returns(query.GetEnumerator());

//            _requestMock
//                .Setup(m => m.DisabledQueryParams)
//                .Returns(QueryParams.Fields);

//            var queryParser = GetQueryParser();

//            // act
//            var querySet = queryParser.Parse(_queryCollectionMock.Object);

//            // Assert
//            Assert.Empty(querySet.Fields);
//        }

//        [Fact]
//        public void Can_Parse_Fields_Query()
//        {
//            // arrange
//            const string type = "articles";
//            const string attrName = "some-field";
//            const string internalAttrName = "SomeField";

//            var query = new Dictionary<string, StringValues> { { $"fields[{type}]", new StringValues(attrName) } };

//            _queryCollectionMock
//                .Setup(m => m.GetEnumerator())
//                .Returns(query.GetEnumerator());

//            _requestMock
//                .Setup(m => m.GetRequestResource())
//                .Returns(new ContextEntity
//                {
//                    EntityName = type,
//                    Attributes = new List<AttrAttribute>
//                        {
//                            new AttrAttribute(attrName)
//                            {
//                                InternalAttributeName = internalAttrName
//                            }
//                        },
//                    Relationships = new List<RelationshipAttribute>()
//                });

//            var queryParser = GetQueryParser();

//            // act
//            var querySet = queryParser.Parse(_queryCollectionMock.Object);

//            // assert
//            Assert.NotEmpty(querySet.Fields);
//            Assert.Equal(2, querySet.Fields.Count);
//            Assert.Equal("Id", querySet.Fields[0]);
//            Assert.Equal(internalAttrName, querySet.Fields[1]);
//        }

//        [Fact]
//        public void Throws_JsonApiException_If_Field_DoesNotExist()
//        {
//            // arrange
//            const string type = "articles";
//            const string attrName = "dne";

//            var query = new Dictionary<string, StringValues> { { $"fields[{type}]", new StringValues(attrName) } };

//            _queryCollectionMock
//                .Setup(m => m.GetEnumerator())
//                .Returns(query.GetEnumerator());

//            _requestMock
//                .Setup(m => m.GetRequestResource())
//                .Returns(new ContextEntity
//                {
//                    EntityName = type,
//                    Attributes = new List<AttrAttribute>(),
//                    Relationships = new List<RelationshipAttribute>()
//                });

//            var queryParser = GetQueryParser();

//            // act , assert
//            var ex = Assert.Throws<JsonApiException>(() => queryParser.Parse(_queryCollectionMock.Object));
//            Assert.Equal(400, ex.GetStatusCode());
//        }



//        [Theory]
//        [InlineData("1", 1, false)]
//        [InlineData("abcde", 0, true)]
//        [InlineData("", 0, true)]
//        public void Can_Parse_Page_Size_Query(string value, int expectedValue, bool shouldThrow)
//        {
//            // arrange
//            var query = new Dictionary<string, StringValues>
//                { { "page[size]", new StringValues(value) }
//                };

//            _queryCollectionMock
//                .Setup(m => m.GetEnumerator())
//                .Returns(query.GetEnumerator());

//            var queryParser = GetQueryParser();

//            // act
//            if (shouldThrow)
//            {
//                var ex = Assert.Throws<JsonApiException>(() => queryParser.Parse(_queryCollectionMock.Object));
//                Assert.Equal(400, ex.GetStatusCode());
//            }
//            else
//            {
//                var querySet = queryParser.Parse(_queryCollectionMock.Object);
//                Assert.Equal(expectedValue, querySet.PageQuery.PageSize);
//            }
//        }

//        [Theory]
//        [InlineData("1", 1, false)]
//        [InlineData("abcde", 0, true)]
//        [InlineData("", 0, true)]
//        public void Can_Parse_Page_Number_Query(string value, int expectedValue, bool shouldThrow)
//        {
//            // arrange
//            var query = new Dictionary<string, StringValues>
//                { { "page[number]", new StringValues(value) }
//                };

//            _queryCollectionMock
//                .Setup(m => m.GetEnumerator())
//                .Returns(query.GetEnumerator());

//            var queryParser = GetQueryParser();

//            // act
//            if (shouldThrow)
//            {
//                var ex = Assert.Throws<JsonApiException>(() => queryParser.Parse(_queryCollectionMock.Object));
//                Assert.Equal(400, ex.GetStatusCode());
//            }
//            else
//            {
//                var querySet = queryParser.Parse(_queryCollectionMock.Object);
//                Assert.Equal(expectedValue, querySet.PageQuery.PageOffset);
//            }
//        }
//    }
//}
