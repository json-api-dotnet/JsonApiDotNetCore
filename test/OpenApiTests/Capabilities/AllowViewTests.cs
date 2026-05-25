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

    [Fact]
    public async Task Hides_attribute_property_in_response()
    {
        // Act
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

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
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

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
    public async Task Hides_get_ToOne_relationship_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./allowViewCapabilities/{id}/relationships/parentViewOn").With(endpointElement =>
        {
            endpointElement.Should().ContainPath("get");
            endpointElement.Should().ContainPath("head");
            endpointElement.Should().ContainPath("patch");
        });

        document.Should().ContainPath("paths./allowViewCapabilities/{id}/relationships/parentViewOff").With(endpointElement =>
        {
            endpointElement.Should().NotContainPath("get");
            endpointElement.Should().NotContainPath("head");
            endpointElement.Should().ContainPath("patch");
        });
    }

    [Fact]
    public async Task Hides_ToMany_relationship_property_in_response()
    {
        // Act
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

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

    [Fact]
    public async Task Hides_get_ToMany_relationship_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./allowViewCapabilities/{id}/relationships/childrenViewOn").With(endpointElement =>
        {
            endpointElement.Should().ContainPath("get");
            endpointElement.Should().ContainPath("head");
            endpointElement.Should().ContainPath("post");
            endpointElement.Should().ContainPath("patch");
            endpointElement.Should().ContainPath("delete");
        });

        document.Should().ContainPath("paths./allowViewCapabilities/{id}/relationships/childrenViewOff").With(endpointElement =>
        {
            endpointElement.Should().NotContainPath("get");
            endpointElement.Should().NotContainPath("head");
            endpointElement.Should().ContainPath("post");
            endpointElement.Should().ContainPath("patch");
            endpointElement.Should().ContainPath("delete");
        });
    }
}
