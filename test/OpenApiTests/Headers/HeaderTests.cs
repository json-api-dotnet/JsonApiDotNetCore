using Xunit;

namespace OpenApiTests.Headers;

public sealed class HeaderTests : IClassFixture<OpenApiTestContext<OpenApiStartup<HeadersDbContext>, HeadersDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<HeadersDbContext>, HeadersDbContext> _testContext;

    public HeaderTests(OpenApiTestContext<OpenApiStartup<HeadersDbContext>, HeadersDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CountriesController>();

        testContext.SwaggerDocumentOutputDirectory = "test/OpenApiEndToEndTests/Headers";
    }

    [Fact]
    public Task Dummy()
    {
        return _testContext.GetSwaggerDocumentAsync();
    }
}
