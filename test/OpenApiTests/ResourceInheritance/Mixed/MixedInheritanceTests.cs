using JsonApiDotNetCore.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OpenApiTests.ResourceInheritance.Mixed;

public sealed class MixedInheritanceTests : IClassFixture<OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;

    public MixedInheritanceTests(OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<DistrictsController>();

        testContext.UseController<BuildingsController>();
        //testContext.UseController<ResidencesController>();
        testContext.UseController<FamilyHomesController>();
        //testContext.UseController<MansionsController>();

        //testContext.UseController<RoomsController>();
        //testContext.UseController<KitchensController>();
        //testContext.UseController<BedroomsController>();
        //testContext.UseController<BathroomsController>();
        //testContext.UseController<LivingRoomsController>();
        //testContext.UseController<ToiletsController>();

        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";

        testContext.ConfigureServices(services => services.AddSingleton<IJsonApiEndpointFilter, MixedJsonApiEndpointFilter>());
    }

    [Fact]
    public async Task Test1()
    {
        // Act
        _ = await _testContext.GetSwaggerDocumentAsync();
    }
}
