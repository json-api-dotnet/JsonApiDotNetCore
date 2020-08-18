using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Pagination
{
    public sealed class PaginationRangeWithMaximumTests : IClassFixture<IntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly IntegrationTestContext<Startup, AppDbContext> _testContext;

        private const int _maximumPageSize = 15;
        private const int _maximumPageNumber = 20;

        public PaginationRangeWithMaximumTests(IntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = new PageSize(5);
            options.MaximumPageSize = new PageSize(_maximumPageSize);
            options.MaximumPageNumber = new PageNumber(_maximumPageNumber);
        }

        [Fact]
        public async Task When_page_number_is_below_maximum_it_must_succeed()
        {
            // Arrange
            const int pageNumber = _maximumPageNumber - 1;
            var route = "/api/v1/todoItems?page[number]=" + pageNumber;

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_page_number_equals_maximum_it_must_succeed()
        {
            // Arrange
            const int pageNumber = _maximumPageNumber;
            var route = "/api/v1/todoItems?page[number]=" + pageNumber;

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_page_number_is_over_maximum_it_must_fail()
        {
            // Arrange
            const int pageNumber = _maximumPageNumber + 1;
            var route = "/api/v1/todoItems?page[number]=" + pageNumber;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified paging is invalid.");
            responseDocument.Errors[0].Detail.Should().Be($"Page number cannot be higher than {_maximumPageNumber}.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("page[number]");
        }

        [Fact]
        public async Task When_page_size_equals_zero_it_must_fail()
        {
            // Arrange
            var route = "/api/v1/todoItems?page[size]=0";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified paging is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Page size cannot be unconstrained.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("page[size]");
        }

        [Fact]
        public async Task When_page_size_is_below_maximum_it_must_succeed()
        {
            // Arrange
            const int pageSize = _maximumPageSize - 1;
            var route = "/api/v1/todoItems?page[size]=" + pageSize;

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_page_size_equals_maximum_it_must_succeed()
        {
            // Arrange
            const int pageSize = _maximumPageSize;
            var route = "/api/v1/todoItems?page[size]=" + pageSize;

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_page_size_is_over_maximum_it_must_fail()
        {
            // Arrange
            const int pageSize = _maximumPageSize + 1;
            var route = "/api/v1/todoItems?page[size]=" + pageSize;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified paging is invalid.");
            responseDocument.Errors[0].Detail.Should().Be($"Page size cannot be higher than {_maximumPageSize}.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("page[size]");
        }
    }
}
