using System.Text.Json;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.Capabilities;

public sealed class AllowCreateChangeTests : IClassFixture<OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext> _testContext;

    public AllowCreateChangeTests(OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<AllowCreateChangeCapabilitiesController>();

        testContext.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public async Task Hides_attribute_property_in_create_or_update_resource_request()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        const string fieldOnName = "attributeOn";
        const string fieldCreateOffName = "attributeCreateOff";
        const string fieldChangeOffName = "attributeChangeOff";

        document.Should().ContainPath("components.schemas.attributesInAllowCreateChangeCapabilityResponse.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().ContainPath(fieldCreateOffName);
            propertiesElement.Should().ContainPath(fieldChangeOffName);
        });

        document.Should().ContainPath("components.schemas.attributesInCreateAllowCreateChangeCapabilityRequest.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().NotContainPath(fieldCreateOffName);
            propertiesElement.Should().ContainPath(fieldChangeOffName);
        });

        document.Should().ContainPath("components.schemas.attributesInUpdateAllowCreateChangeCapabilityRequest.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().ContainPath(fieldCreateOffName);
            propertiesElement.Should().NotContainPath(fieldChangeOffName);
        });
    }
}
