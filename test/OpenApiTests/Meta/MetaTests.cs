using System.Text.Json;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.Meta;

public sealed class MetaTests : IClassFixture<OpenApiTestContext<OpenApiStartup<MetaDbContext>, MetaDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<MetaDbContext>, MetaDbContext> _testContext;

    public MetaTests(OpenApiTestContext<OpenApiStartup<MetaDbContext>, MetaDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<JigsawPuzzlesController>();
        testContext.UseController<JigsawPuzzlePicturesController>();
        testContext.UseController<JigsawPuzzlePiecesController>();
        testContext.UseController<OperationsController>();

        testContext.SetTestOutputHelper(testOutputHelper);

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeJsonApiVersion = true;
    }

    [Fact]
    public async Task Includes_meta_definition()
    {
        // Act
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.meta").With(metaElement =>
        {
            metaElement.Should().BeJson("""
                {
                  "type": "object",
                  "additionalProperties": {
                    "nullable": true
                  }
                }
                """);
        });
    }

    [Theory]
    [InlineData("atomicOperation")]
    [InlineData("atomicResult")]
    [InlineData("createJigsawPuzzlePictureRequestDocument")]
    [InlineData("createJigsawPuzzlePieceRequestDocument")]
    [InlineData("createJigsawPuzzleRequestDocument")]
    [InlineData("errorObject")]
    [InlineData("errorResponseDocument")]
    [InlineData("identifierInRequest")]
    [InlineData("jigsawPuzzleCollectionResponseDocument")]
    [InlineData("jigsawPuzzleIdentifierInResponse")]
    [InlineData("jigsawPuzzleIdentifierResponseDocument")]
    [InlineData("jigsawPuzzlePictureCollectionResponseDocument")]
    [InlineData("jigsawPuzzlePictureIdentifierInResponse")]
    [InlineData("jigsawPuzzlePictureIdentifierResponseDocument")]
    [InlineData("jigsawPuzzlePieceCollectionResponseDocument")]
    [InlineData("jigsawPuzzlePieceIdentifierCollectionResponseDocument")]
    [InlineData("jigsawPuzzlePieceIdentifierInResponse")]
    [InlineData("jsonapi")]
    [InlineData("nullableJigsawPuzzlePictureIdentifierResponseDocument")]
    [InlineData("nullableSecondaryJigsawPuzzlePictureResponseDocument")]
    [InlineData("nullableToOneJigsawPuzzlePictureInRequest")]
    [InlineData("nullableToOneJigsawPuzzlePictureInResponse")]
    [InlineData("operationsRequestDocument")]
    [InlineData("operationsResponseDocument")]
    [InlineData("primaryJigsawPuzzlePictureResponseDocument")]
    [InlineData("primaryJigsawPuzzlePieceResponseDocument")]
    [InlineData("primaryJigsawPuzzleResponseDocument")]
    [InlineData("resourceInCreateRequest")]
    [InlineData("resourceInResponse")]
    [InlineData("resourceInUpdateRequest")]
    [InlineData("secondaryJigsawPuzzlePictureResponseDocument")]
    [InlineData("secondaryJigsawPuzzleResponseDocument")]
    [InlineData("toManyJigsawPuzzlePieceInRequest")]
    [InlineData("toManyJigsawPuzzlePieceInResponse")]
    [InlineData("toOneJigsawPuzzleInRequest")]
    [InlineData("toOneJigsawPuzzleInResponse")]
    [InlineData("toOneJigsawPuzzlePictureInRequest")]
    [InlineData("toOneJigsawPuzzlePictureInResponse")]
    [InlineData("updateJigsawPuzzlePictureRequestDocument")]
    [InlineData("updateJigsawPuzzlePieceRequestDocument")]
    [InlineData("updateJigsawPuzzleRequestDocument")]
    public async Task Includes_meta_property(string schemaId)
    {
        // Act
        JsonElement document = await _testContext.GetOpenApiDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{schemaId}.properties").With(propertiesElement =>
        {
            JsonElement metaElement = propertiesElement.Should().ContainPath("meta.allOf[0]");
            metaElement.Should().HaveProperty("$ref", "#/components/schemas/meta");
        });
    }
}
