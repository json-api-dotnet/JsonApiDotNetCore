using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

public sealed class DisableQueryStringTests : IClassFixture<IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;

    public DisableQueryStringTests(IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<SofasController>();
        testContext.UseController<PillowsController>();

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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
        error.Detail.Should().Be("The parameter 'sort' cannot be used at this endpoint.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("sort");
    }

    [Fact]
    public async Task Cannot_paginate_if_query_string_parameter_is_blocked_by_controller()
    {
        // Arrange
        const string route = "/sofas?page[number]=2";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
        error.Detail.Should().Be("The parameter 'page[number]' cannot be used at this endpoint.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("page[number]");
    }

    [Fact]
    public async Task Can_use_custom_query_string_parameter()
    {
        // Arrange
        const string route = "/sofas?skipCache";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Cannot_use_custom_query_string_parameter_if_blocked_by_controller()
    {
        // Arrange
        const string route = "/pillows?skipCache";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
        error.Detail.Should().Be("The parameter 'skipCache' cannot be used at this endpoint.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("skipCache");
    }
}
