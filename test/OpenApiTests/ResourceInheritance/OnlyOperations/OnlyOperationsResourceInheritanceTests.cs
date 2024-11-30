using Xunit;

namespace OpenApiTests.ResourceInheritance.OnlyOperations;

public sealed class OnlyOperationsResourceInheritanceTests
    : IClassFixture<OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;

    public OnlyOperationsResourceInheritanceTests(OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();

        testContext.ConfigureServices(services => services.RegisterTestOperationFilter());

        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Fact]
    public async Task Test2()
    {
        // Act
        _ = await _testContext.GetSwaggerDocumentAsync();
    }
}
