using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OpenApiNSwagEndToEndTests.Links.GeneratedCode;
using OpenApiTests;
using OpenApiTests.Links;
using Swashbuckle.AspNetCore.Swagger;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.Links;

public sealed class AlternateOpenApiRouteTests : IClassFixture<IntegrationTestContext<OpenApiStartup<LinkDbContext>, LinkDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<LinkDbContext>, LinkDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly LinkFakers _fakers = new();

    public AlternateOpenApiRouteTests(IntegrationTestContext<OpenApiStartup<LinkDbContext>, LinkDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.ConfigureServices(services =>
            services.Configure<SwaggerOptions>(options => options.RouteTemplate = "/api-docs/{documentName}/swagger.yaml"));

        testContext.UseController<ExcursionsController>();
    }

    [Fact]
    public async Task DescribedBy_link_matches_alternate_OpenAPI_route()
    {
        // Arrange
        Excursion excursion = _fakers.Excursion.GenerateOne();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new LinksClient(httpClient);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Excursions.Add(excursion);
            await dbContext.SaveChangesAsync();
        });

        // Act
        ExcursionPrimaryResponseDocument response = await apiClient.GetExcursionAsync(excursion.StringId!);

        // Assert
        response.Links.Describedby.Should().Be("/api-docs/v1/swagger.yaml");
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
