using System.Text.Json;
using JsonApiDotNetCore.Resources.Annotations;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;
using RequestType = OpenApiTests.AttributeTypes.CapabilitiesUtils.RequestType;

namespace OpenApiTests.AttributeTypes;

public sealed class AttributeCapabilitiesTests : IClassFixture<OpenApiTestContext<AttributeTypesStartup, AttributeTypesDbContext>>
{
    private static readonly Dictionary<RequestType, AttrCapabilities> CapabilitiesByRequestType = new()
    {
        { RequestType.Response, AttrCapabilities.AllowView },
        { RequestType.Create, AttrCapabilities.AllowCreate },
        { RequestType.Update, AttrCapabilities.AllowChange }
    };

    private static readonly Dictionary<RequestType, string> SchemaPathByRequestType = new()
    {
        { RequestType.Response, "components.schemas.attributesInBookResponse.allOf[1].properties" },
        { RequestType.Create, "components.schemas.attributesInCreateBookRequest.allOf[1].properties" },
        { RequestType.Update, "components.schemas.attributesInUpdateBookRequest.allOf[1].properties" }
    };

    private static readonly Dictionary<AttrCapabilities, List<string>> BookModelAttrsByCapability = new()
    {
        {
            AttrCapabilities.AllowView, [
                "title",
                "isbn",
                "publishedOn"
            ]
        },
        {
            AttrCapabilities.AllowChange, [
                "title",
                "internalNotes"
            ]
        },
        { AttrCapabilities.AllowCreate, ["draftContent"] },
        { AttrCapabilities.None, ["secretCode"] }
    };

    private readonly OpenApiTestContext<AttributeTypesStartup, AttributeTypesDbContext> _testContext;

    public AttributeCapabilitiesTests(
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
    private async Task Generated_Schema_Includes_Only_Expected_Attrs_For_Request_Type_Async(RequestType requestType)
    {
        // Arrange
        string path = SchemaPathByRequestType[requestType];
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();
        JsonElement attrs = document.Should().ContainPath(path);

        IList<string> bookExpectedAttrs = BookModelAttrsByCapability[CapabilitiesByRequestType[requestType]];

        IList<string> bookUnexpectedAttrs = BookModelAttrsByCapability.SelectMany(kvPair => kvPair.Value)
            .Except(bookExpectedAttrs).ToList();

        // Assert
        foreach (string attrName in bookExpectedAttrs)
        {
            attrs.Should().ContainProperty(attrName);
        }

        foreach (string attrName in bookUnexpectedAttrs)
        {
            attrs.Should().NotContainPath($"properties.{attrName}");
        }
    }
}
