using System.Text.Json;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.Capabilities;

public sealed class AllowSetTests : IClassFixture<OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext> _testContext;

    public AllowSetTests(OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<AllowSetCapabilitiesController>();

        testContext.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public async Task Hides_ToOne_relationship_property_in_create_and_update_resource_request()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        const string fieldOnName = "parentSetOn";
        const string fieldOffName = "parentSetOff";

        document.Should().ContainPath("components.schemas.relationshipsInAllowSetCapabilityResponse.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().ContainPath(fieldOffName);
        });

        document.Should().ContainPath("components.schemas.relationshipsInCreateAllowSetCapabilityRequest.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().NotContainPath(fieldOffName);
        });

        document.Should().ContainPath("components.schemas.relationshipsInUpdateAllowSetCapabilityRequest.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().NotContainPath(fieldOffName);
        });
    }

    [Fact]
    public async Task Hides_update_ToOne_relationship_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./allowSetCapabilities/{id}/relationships/parentSetOn").With(endpointElement =>
        {
            endpointElement.Should().ContainPath("get");
            endpointElement.Should().ContainPath("head");
            endpointElement.Should().ContainPath("patch");
        });

        document.Should().ContainPath("paths./allowSetCapabilities/{id}/relationships/parentSetOff").With(endpointElement =>
        {
            endpointElement.Should().ContainPath("get");
            endpointElement.Should().ContainPath("head");
            endpointElement.Should().NotContainPath("patch");
        });
    }

    [Fact]
    public async Task Hides_ToMany_relationship_property_in_create_and_update_resource_request()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        const string fieldOnName = "childrenSetOn";
        const string fieldOffName = "childrenSetOff";

        document.Should().ContainPath("components.schemas.relationshipsInAllowSetCapabilityResponse.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().ContainPath(fieldOffName);
        });

        document.Should().ContainPath("components.schemas.relationshipsInCreateAllowSetCapabilityRequest.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().NotContainPath(fieldOffName);
        });

        document.Should().ContainPath("components.schemas.relationshipsInUpdateAllowSetCapabilityRequest.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().NotContainPath(fieldOffName);
        });
    }

    [Fact]
    public async Task Hides_update_ToMany_relationship_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./allowSetCapabilities/{id}/relationships/childrenSetOn").With(endpointElement =>
        {
            endpointElement.Should().ContainPath("get");
            endpointElement.Should().ContainPath("head");
            endpointElement.Should().ContainPath("post");
            endpointElement.Should().ContainPath("patch");
            endpointElement.Should().ContainPath("delete");
        });

        document.Should().ContainPath("paths./allowSetCapabilities/{id}/relationships/childrenSetOff").With(endpointElement =>
        {
            endpointElement.Should().ContainPath("get");
            endpointElement.Should().ContainPath("head");
            endpointElement.Should().ContainPath("post");
            endpointElement.Should().NotContainPath("patch");
            endpointElement.Should().ContainPath("delete");
        });
    }
}
