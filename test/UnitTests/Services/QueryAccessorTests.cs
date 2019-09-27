using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class QueryAccessorTests
    {
        private readonly Mock<IRequestManager> _rmMock;
        private readonly Mock<ILogger<QueryAccessor>> _loggerMock;
        private readonly Mock<IQueryCollection> _queryMock;

        public QueryAccessorTests()
        {
            _rmMock = new Mock<IRequestManager>();
            _loggerMock = new Mock<ILogger<QueryAccessor>>();
            _queryMock = new Mock<IQueryCollection>();
        }

        [Fact]
        public void GetGuid_GuidIsValid_IsReturnedCorrectly()
        {
            // Arrange
            const string key = "SomeId";
            var value = Guid.NewGuid();
            var querySet = new QuerySet
            {
                Filters = new List<FilterQuery> {
                    new FilterQuery(key, value.ToString(), "eq")
                }
            };

            _rmMock.Setup(c => c.QuerySet).Returns(querySet);

            var service = new QueryAccessor(_rmMock.Object, _loggerMock.Object);

            // act
            var success = service.TryGetValue<Guid>("SomeId", out Guid result);

            // assert
            Assert.True(success);
            Assert.Equal(value, result);
        }

        [Fact]
        public void GetRequired_Throws_If_Not_Present()
        {
            // Arrange
            const string key = "SomeId";
            var value = Guid.NewGuid();

            var querySet = new QuerySet
            {
                Filters = new List<FilterQuery> {
                    new FilterQuery(key, value.ToString(), "eq")
                }
            };

            _rmMock.Setup(c => c.QuerySet).Returns(querySet);

            var service = new QueryAccessor(_rmMock.Object, _loggerMock.Object);

            // act
            var exception = Assert.Throws<JsonApiException>(() => service.GetRequired<Guid>("Invalid"));

            // assert
            Assert.Equal(422, exception.GetStatusCode());
        }

        [Fact]
        public void GetRequired_Does_Not_Throw_If_Present()
        {
            // arrange
            const string key = "SomeId";
            var value = Guid.NewGuid();

            var querySet = new QuerySet
            {
                Filters = new List<FilterQuery> {
                    new FilterQuery(key, value.ToString(), "eq")
                }
            };

            _rmMock.Setup(c => c.QuerySet).Returns(querySet);

            var service = new QueryAccessor(_rmMock.Object, _loggerMock.Object);

            // Act
            var result = service.GetRequired<Guid>("SomeId");

            // Assert
            Assert.Equal(value, result);
        }
    }
}
