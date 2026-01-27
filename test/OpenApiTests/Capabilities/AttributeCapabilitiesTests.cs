using System.Text.Json;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.Capabilities;

public sealed class AttributeCapabilitiesTests : IClassFixture<OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext> _testContext;

    public AttributeCapabilitiesTests(OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<BooksController>();

        testContext.SetTestOutputHelper(testOutputHelper);
        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Theory]
    [InlineData("title")]
    [InlineData("isbn")]
    [InlineData("publishedOn")]
    [InlineData("hasEmptyTitle")]
    public async Task Attribute_with_AllowView_capability_appears_in_response_schema(string attributeName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.attributesInBookResponse.allOf[1].properties").With(properties =>
        {
            properties.Should().ContainProperty(attributeName);
        });
    }

    [Theory]
    [InlineData("draftContent")]
    [InlineData("internalNotes")]
    [InlineData("secretCode")]
    [InlineData("isDeleted")]
    public async Task Attribute_without_AllowView_capability_does_not_appear_in_response_schema(string attributeName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.attributesInBookResponse.allOf[1].properties").With(properties =>
        {
            properties.Should().NotContainPath(attributeName);
        });
    }

    [Theory]
    [InlineData("draftContent")]
    [InlineData("isDeleted")]
    public async Task Attribute_with_AllowCreate_capability_appears_in_create_request_schema(string attributeName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.attributesInCreateBookRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().ContainProperty(attributeName);
        });
    }

    [Theory]
    [InlineData("title")]
    [InlineData("isbn")]
    [InlineData("publishedOn")]
    [InlineData("internalNotes")]
    [InlineData("secretCode")]
    [InlineData("hasEmptyTitle")]
    public async Task Attribute_without_AllowCreate_capability_does_not_appear_in_create_request_schema(string attributeName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.attributesInCreateBookRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().NotContainPath(attributeName);
        });
    }

    [Theory]
    [InlineData("title")]
    [InlineData("internalNotes")]
    [InlineData("isDeleted")]
    public async Task Attribute_with_AllowChange_capability_appears_in_update_request_schema(string attributeName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.attributesInUpdateBookRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().ContainProperty(attributeName);
        });
    }

    [Theory]
    [InlineData("isbn")]
    [InlineData("publishedOn")]
    [InlineData("draftContent")]
    [InlineData("secretCode")]
    [InlineData("hasEmptyTitle")]
    public async Task Attribute_without_AllowChange_capability_does_not_appear_in_update_request_schema(string attributeName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.attributesInUpdateBookRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().NotContainPath(attributeName);
        });
    }

    [Fact]
    public async Task Attribute_with_None_capability_does_not_appear_in_any_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.attributesInBookResponse.allOf[1].properties").With(properties =>
        {
            properties.Should().NotContainPath("secretCode");
        });

        document.Should().ContainPath("components.schemas.attributesInCreateBookRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().NotContainPath("secretCode");
        });

        document.Should().ContainPath("components.schemas.attributesInUpdateBookRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().NotContainPath("secretCode");
        });
    }

    [Fact]
    public async Task Get_only_property_only_appears_in_response_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.attributesInBookResponse.allOf[1].properties").With(properties =>
        {
            properties.Should().ContainProperty("hasEmptyTitle");
        });

        document.Should().ContainPath("components.schemas.attributesInCreateBookRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().NotContainPath("hasEmptyTitle");
        });

        document.Should().ContainPath("components.schemas.attributesInUpdateBookRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().NotContainPath("hasEmptyTitle");
        });
    }

    [Fact]
    public async Task Set_only_property_only_appears_in_request_schemas()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.attributesInBookResponse.allOf[1].properties").With(properties =>
        {
            properties.Should().NotContainPath("isDeleted");
        });

        document.Should().ContainPath("components.schemas.attributesInCreateBookRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().ContainProperty("isDeleted");
        });

        document.Should().ContainPath("components.schemas.attributesInUpdateBookRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().ContainProperty("isDeleted");
        });
    }
}