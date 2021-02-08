using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings.Pagination
{
    public sealed class RangeValidationWithMaximumTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;

        private const int _maximumPageSize = 15;
        private const int _maximumPageNumber = 20;

        public RangeValidationWithMaximumTests(ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = new PageSize(5);
            options.MaximumPageSize = new PageSize(_maximumPageSize);
            options.MaximumPageNumber = new PageNumber(_maximumPageNumber);
        }

        [Fact]
        public async Task Can_use_page_number_below_maximum()
        {
            // Arrange
            const int pageNumber = _maximumPageNumber - 1;
            var route = "/blogs?page[number]=" + pageNumber;

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Can_use_page_number_equal_to_maximum()
        {
            // Arrange
            const int pageNumber = _maximumPageNumber;
            var route = "/blogs?page[number]=" + pageNumber;

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Cannot_use_page_number_over_maximum()
        {
            // Arrange
            const int pageNumber = _maximumPageNumber + 1;
            var route = "/blogs?page[number]=" + pageNumber;

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
        public async Task Cannot_use_zero_page_size()
        {
            // Arrange
            var route = "/blogs?page[size]=0";

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
        public async Task Can_use_page_size_below_maximum()
        {
            // Arrange
            const int pageSize = _maximumPageSize - 1;
            var route = "/blogs?page[size]=" + pageSize;

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Can_use_page_size_equal_to_maximum()
        {
            // Arrange
            const int pageSize = _maximumPageSize;
            var route = "/blogs?page[size]=" + pageSize;

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Cannot_use_page_size_over_maximum()
        {
            // Arrange
            const int pageSize = _maximumPageSize + 1;
            var route = "/blogs?page[size]=" + pageSize;

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
