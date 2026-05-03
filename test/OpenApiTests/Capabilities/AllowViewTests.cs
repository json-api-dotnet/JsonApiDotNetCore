using System.Text.Json;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.Capabilities;

public sealed class AllowViewTests : IClassFixture<OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext> _testContext;

    public AllowViewTests(OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<AllowViewCapabilitiesController>();

        testContext.SetTestOutputHelper(testOutputHelper);
    }

    // TODO: Fix BUG that allows secondary/relationship GET endpoint, while it should block.
    // Once fixed, should assert here that endpoints are excluded in OpenAPI.

    [Fact]
    public async Task Hides_attribute_property_in_response()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        const string fieldOnName = "attributeViewOn";
        const string fieldOffName = "attributeViewOff";

        document.Should().ContainPath("components.schemas.attributesInAllowViewCapabilityResponse.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().NotContainPath(fieldOffName);
        });

        document.Should().ContainPath("components.schemas.attributesInCreateAllowViewCapabilityRequest.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().ContainPath(fieldOffName);
        });

        document.Should().ContainPath("components.schemas.attributesInUpdateAllowViewCapabilityRequest.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().ContainPath(fieldOffName);
        });
    }

    [Fact]
    public async Task Hides_ToOne_relationship_property_in_response()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        const string fieldOnName = "parentViewOn";
        const string fieldOffName = "parentViewOff";

        document.Should().ContainPath("components.schemas.relationshipsInAllowViewCapabilityResponse.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().NotContainPath(fieldOffName);
        });

        document.Should().ContainPath("components.schemas.relationshipsInCreateAllowViewCapabilityRequest.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().ContainPath(fieldOffName);
        });

        document.Should().ContainPath("components.schemas.relationshipsInUpdateAllowViewCapabilityRequest.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().ContainPath(fieldOffName);
        });
    }

    [Fact]
    public async Task Hides_ToMany_relationship_property_in_response()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        const string fieldOnName = "childrenViewOn";
        const string fieldOffName = "childrenViewOff";

        document.Should().ContainPath("components.schemas.relationshipsInAllowViewCapabilityResponse.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().NotContainPath(fieldOffName);
        });

        document.Should().ContainPath("components.schemas.relationshipsInCreateAllowViewCapabilityRequest.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().ContainPath(fieldOffName);
        });

        document.Should().ContainPath("components.schemas.relationshipsInUpdateAllowViewCapabilityRequest.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().ContainPath(fieldOffName);
        });
    }
}
