using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RestrictedControllers
{
    public sealed class DisableQueryStringTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;

        public DisableQueryStringTests(ExampleIntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddScoped<SkipCacheQueryStringParameterReader>();
                services.AddScoped<IQueryStringParameterReader>(sp => sp.GetRequiredService<SkipCacheQueryStringParameterReader>());
            });
        }

        [Fact]
        public async Task Cannot_sort_if_query_string_parameter_is_blocked_by_controller()
        {
            // Arrange
            var route = "/sofas?sort=id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
            responseDocument.Errors[0].Detail.Should().Be("The parameter 'sort' cannot be used at this endpoint.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("sort");
        }

        [Fact]
        public async Task Cannot_paginate_if_query_string_parameter_is_blocked_by_controller()
        {
            // Arrange
            var route = "/sofas?page[number]=2";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
            responseDocument.Errors[0].Detail.Should().Be("The parameter 'page[number]' cannot be used at this endpoint.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("page[number]");
        }

        [Fact]
        public async Task Cannot_use_custom_query_string_parameter_if_blocked_by_controller()
        {
            // Arrange
            var route = "/beds?skipCache=true";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
            responseDocument.Errors[0].Detail.Should().Be("The parameter 'skipCache' cannot be used at this endpoint.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("skipCache");
        }
    }
}
