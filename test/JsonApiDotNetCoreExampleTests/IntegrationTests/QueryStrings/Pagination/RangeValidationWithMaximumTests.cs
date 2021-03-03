using System.Net;
using System.Net.Http;
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
        private const int MaximumPageSize = 15;
        private const int MaximumPageNumber = 20;
        private readonly ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;

        public RangeValidationWithMaximumTests(ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = new PageSize(5);
            options.MaximumPageSize = new PageSize(MaximumPageSize);
            options.MaximumPageNumber = new PageNumber(MaximumPageNumber);
        }

        [Fact]
        public async Task Can_use_page_number_below_maximum()
        {
            // Arrange
            const int pageNumber = MaximumPageNumber - 1;
            string route = "/blogs?page[number]=" + pageNumber;

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Can_use_page_number_equal_to_maximum()
        {
            // Arrange
            const int pageNumber = MaximumPageNumber;
            string route = "/blogs?page[number]=" + pageNumber;

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Cannot_use_page_number_over_maximum()
        {
            // Arrange
            const int pageNumber = MaximumPageNumber + 1;
            string route = "/blogs?page[number]=" + pageNumber;

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("The specified paging is invalid.");
            error.Detail.Should().Be($"Page number cannot be higher than {MaximumPageNumber}.");
            error.Source.Parameter.Should().Be("page[number]");
        }

        [Fact]
        public async Task Cannot_use_zero_page_size()
        {
            // Arrange
            const string route = "/blogs?page[size]=0";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("The specified paging is invalid.");
            error.Detail.Should().Be("Page size cannot be unconstrained.");
            error.Source.Parameter.Should().Be("page[size]");
        }

        [Fact]
        public async Task Can_use_page_size_below_maximum()
        {
            // Arrange
            const int pageSize = MaximumPageSize - 1;
            string route = "/blogs?page[size]=" + pageSize;

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Can_use_page_size_equal_to_maximum()
        {
            // Arrange
            const int pageSize = MaximumPageSize;
            string route = "/blogs?page[size]=" + pageSize;

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Cannot_use_page_size_over_maximum()
        {
            // Arrange
            const int pageSize = MaximumPageSize + 1;
            string route = "/blogs?page[size]=" + pageSize;

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("The specified paging is invalid.");
            error.Detail.Should().Be($"Page size cannot be higher than {MaximumPageSize}.");
            error.Source.Parameter.Should().Be("page[size]");
        }
    }
}
