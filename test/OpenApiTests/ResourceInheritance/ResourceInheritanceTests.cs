using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.ResourceInheritance;

public sealed class ResourceInheritanceTests : IClassFixture<OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;

    public ResourceInheritanceTests(OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<DistrictsController>();

        testContext.UseController<BuildingsController>();
        //testContext.UseController<ResidencesController>();
        //testContext.UseController<FamilyHomesController>();
        //testContext.UseController<MansionsController>();

        //testContext.UseController<RoomsController>();
        //testContext.UseController<KitchensController>();
        //testContext.UseController<BedroomsController>();
        //testContext.UseController<BathroomsController>();
        //testContext.UseController<LivingRoomsController>();
        //testContext.UseController<ToiletsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddLogging(loggingBuilder =>
            {
                var provider = new XUnitLoggerProvider(testOutputHelper, null, LogOutputFields.All);
                loggingBuilder.AddProvider(provider);
                // TODO: Why doesn't this work with lower log levels?
                loggingBuilder.SetMinimumLevel(LogLevel.Critical);
            });
        });

        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Fact]
    public async Task Test1()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();
    }
}
