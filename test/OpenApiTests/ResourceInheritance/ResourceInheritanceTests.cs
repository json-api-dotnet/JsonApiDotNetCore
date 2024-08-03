using System.Text.Json;
using Xunit;

namespace OpenApiTests.ResourceInheritance;

public sealed class ResourceInheritanceTests : IClassFixture<OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;

    public ResourceInheritanceTests(OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<DistrictsController>();
        testContext.UseController<BuildingsController>();
        testContext.UseController<ResidencesController>();
        testContext.UseController<FamilyHomesController>();
        testContext.UseController<MansionsController>();

        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Fact]
    public async Task Test1()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();
    }
}
