using System.Net;
using System.Net.Http;
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
    public sealed class DisableQueryStringTests : IClassFixture<ExampleIntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext>>
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
            const string route = "/sofas?sort=id";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
            error.Detail.Should().Be("The parameter 'sort' cannot be used at this endpoint.");
            error.Source.Parameter.Should().Be("sort");
        }

        [Fact]
        public async Task Cannot_paginate_if_query_string_parameter_is_blocked_by_controller()
        {
            // Arrange
            const string route = "/sofas?page[number]=2";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
            error.Detail.Should().Be("The parameter 'page[number]' cannot be used at this endpoint.");
            error.Source.Parameter.Should().Be("page[number]");
        }

        [Fact]
        public async Task Cannot_use_custom_query_string_parameter_if_blocked_by_controller()
        {
            // Arrange
            const string route = "/beds?skipCache=true";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
            error.Detail.Should().Be("The parameter 'skipCache' cannot be used at this endpoint.");
            error.Source.Parameter.Should().Be("skipCache");
        }
    }
}
