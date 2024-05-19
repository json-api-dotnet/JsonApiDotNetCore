using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.Links.GeneratedCode;
using OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.Links;
using Swashbuckle.AspNetCore.Swagger;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.Links;

public sealed class AlternateOpenApiRouteTests : IClassFixture<IntegrationTestContext<OpenApiStartup<LinkDbContext>, LinkDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<LinkDbContext>, LinkDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly LinkFakers _fakers = new();

    public AlternateOpenApiRouteTests(IntegrationTestContext<OpenApiStartup<LinkDbContext>, LinkDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.ConfigureServices(services =>
            services.Configure<SwaggerOptions>(options => options.RouteTemplate = "/api-docs/{documentName}/swagger.yaml"));

        testContext.UseController<ExcursionsController>();
    }

    [Fact]
    public async Task DescribedBy_link_matches_alternate_OpenAPI_route()
    {
        // Arrange
        Excursion excursion = _fakers.Excursion.Generate();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new LinksClient(requestAdapter);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Excursions.Add(excursion);
            await dbContext.SaveChangesAsync();
        });

        // Act
        ExcursionPrimaryResponseDocument? response = await apiClient.Excursions[excursion.StringId].GetAsync();

        // Assert
        response.ShouldNotBeNull();
        response.Links.ShouldNotBeNull();
        response.Links.Describedby.Should().Be("/api-docs/v1/swagger.yaml");
    }
}
