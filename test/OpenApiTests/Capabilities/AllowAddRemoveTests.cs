using System.Text.Json;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.Capabilities;

public sealed class AllowAddRemoveTests : IClassFixture<OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext> _testContext;

    public AllowAddRemoveTests(OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<AllowAddRemoveCapabilitiesController>();

        testContext.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public async Task Does_not_hide_relationship_property()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        const string fieldOnName = "childrenOn";
        const string fieldAddOffName = "childrenAddOff";
        const string fieldRemoveOffName = "childrenRemoveOff";

        document.Should().ContainPath("components.schemas.relationshipsInAllowAddRemoveCapabilityResponse.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().ContainPath(fieldAddOffName);
            propertiesElement.Should().ContainPath(fieldRemoveOffName);
        });

        document.Should().ContainPath("components.schemas.relationshipsInCreateAllowAddRemoveCapabilityRequest.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().ContainPath(fieldAddOffName);
            propertiesElement.Should().ContainPath(fieldRemoveOffName);
        });

        document.Should().ContainPath("components.schemas.relationshipsInUpdateAllowAddRemoveCapabilityRequest.allOf[1].properties").With(propertiesElement =>
        {
            propertiesElement.Should().ContainPath(fieldOnName);
            propertiesElement.Should().ContainPath(fieldAddOffName);
            propertiesElement.Should().ContainPath(fieldRemoveOffName);
        });
    }

    [Fact]
    public async Task Hides_add_or_remove_ToMany_relationship_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./allowAddRemoveCapabilities/{id}/relationships/childrenOn").With(endpointElement =>
        {
            endpointElement.Should().ContainPath("get");
            endpointElement.Should().ContainPath("head");
            endpointElement.Should().ContainPath("post");
            endpointElement.Should().ContainPath("patch");
            endpointElement.Should().ContainPath("delete");
        });

        document.Should().ContainPath("paths./allowAddRemoveCapabilities/{id}/relationships/childrenAddOff").With(endpointElement =>
        {
            endpointElement.Should().ContainPath("get");
            endpointElement.Should().ContainPath("head");
            endpointElement.Should().NotContainPath("post");
            endpointElement.Should().ContainPath("patch");
            endpointElement.Should().ContainPath("delete");
        });

        document.Should().ContainPath("paths./allowAddRemoveCapabilities/{id}/relationships/childrenRemoveOff").With(endpointElement =>
        {
            endpointElement.Should().ContainPath("get");
            endpointElement.Should().ContainPath("head");
            endpointElement.Should().ContainPath("post");
            endpointElement.Should().ContainPath("patch");
            endpointElement.Should().NotContainPath("delete");
        });
    }
}
