using System.Text.Json;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;
using RequestType = OpenApiTests.AttributeTypes.CapabilitiesUtils.RequestType;

namespace OpenApiTests.AttributeTypes;

public sealed class RelationshipCapabilitiesTests : IClassFixture<OpenApiTestContext<AttributeTypesStartup, AttributeTypesDbContext>>
{
    private static readonly Dictionary<RequestType, string> SchemaPathByRequestType = new()
    {
        { RequestType.Response, "components.schemas.relationshipsInBookResponse.allOf[1].properties" },
        { RequestType.Create, "components.schemas.relationshipsInCreateBookRequest.allOf[1].properties" },
        { RequestType.Update, "components.schemas.relationshipsInUpdateBookRequest.allOf[1].properties" }
    };

    private static readonly Dictionary<string, List<string>> BookModelRelsByCapability = new()
    {
        { "AllowView", ["author"] },
        { "AllowSet", ["reviews"] }
    };

    private static readonly Dictionary<RequestType, string> CapabilitiesByRequestType = new()
    {
        { RequestType.Response, "AllowView" },
        { RequestType.Create, "AllowSet" },
        { RequestType.Update, "AllowSet" }
    };

    private readonly OpenApiTestContext<AttributeTypesStartup, AttributeTypesDbContext> _testContext;

    public RelationshipCapabilitiesTests(
        OpenApiTestContext<AttributeTypesStartup, AttributeTypesDbContext> testContext,
        ITestOutputHelper output)
    {
        _testContext = testContext;
        _testContext.UseController<BooksController>();
        _testContext.SetTestOutputHelper(output);
    }

    [Theory]
    [InlineData(RequestType.Response)]
    [InlineData(RequestType.Create)]
    [InlineData(RequestType.Update)]
    public async Task Generated_Schema_Includes_Only_Expected_Relationships_For_Request_Type_Async(RequestType requestType)
    {
        // Arrange
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();
        JsonElement properties = document.Should().ContainPath(SchemaPathByRequestType[requestType]);
        string selectedCapability = CapabilitiesByRequestType[requestType];

        IList<string> expected = BookModelRelsByCapability[selectedCapability];

        IList<string> unexpected = BookModelRelsByCapability.SelectMany(kvp => kvp.Value)
            .Except(expected).ToList();

        // Assert
        foreach (string relName in expected)
        {
            properties.Should().ContainProperty(relName);
        }

        foreach (string relName in unexpected)
        {
            properties.Should().NotContainPath($"properties.{relName}");
        }
    }
}
