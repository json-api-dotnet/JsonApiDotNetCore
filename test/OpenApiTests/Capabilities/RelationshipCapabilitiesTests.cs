using System.Text.Json;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.Capabilities;

public sealed class RelationshipCapabilitiesTests : IClassFixture<OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext> _testContext;

    public RelationshipCapabilitiesTests(OpenApiTestContext<OpenApiStartup<CapabilitiesDbContext>, CapabilitiesDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<BooksController>();
        testContext.UseController<ArticlesController>();

        testContext.SetTestOutputHelper(testOutputHelper);
        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Fact]
    public async Task HasOne_relationship_with_AllowView_appears_in_response_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.relationshipsInBookResponse.allOf[1].properties").With(properties =>
        {
            properties.Should().ContainProperty("author");
        });
    }

    [Fact]
    public async Task HasOne_relationship_without_AllowView_does_not_appear_in_response_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.relationshipsInArticleResponse.allOf[1].properties").With(properties =>
        {
            properties.Should().NotContainPath("writer");
        });
    }

    [Fact]
    public async Task HasOne_relationship_with_AllowSet_appears_in_create_request_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.relationshipsInCreateArticleRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().ContainProperty("writer");
        });
    }

    [Fact]
    public async Task HasOne_relationship_without_AllowSet_does_not_appear_in_create_request_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.relationshipsInCreateBookRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().NotContainPath("author");
        });
    }

    [Fact]
    public async Task HasOne_relationship_with_AllowSet_appears_in_update_request_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.relationshipsInUpdateArticleRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().ContainProperty("writer");
        });
    }

    [Fact]
    public async Task HasOne_relationship_without_AllowSet_does_not_appear_in_update_request_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.relationshipsInUpdateBookRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().NotContainPath("author");
        });
    }

    [Fact]
    public async Task HasMany_relationship_with_AllowView_appears_in_response_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.relationshipsInArticleResponse.allOf[1].properties").With(properties =>
        {
            properties.Should().ContainProperty("categories");
        });
    }

    [Fact]
    public async Task HasMany_relationship_without_AllowView_does_not_appear_in_response_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.relationshipsInBookResponse.allOf[1].properties").With(properties =>
        {
            properties.Should().NotContainPath("reviews");
        });
    }

    [Fact]
    public async Task HasMany_relationship_with_AllowSet_appears_in_create_request_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.relationshipsInCreateBookRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().ContainProperty("reviews");
        });
    }

    [Fact]
    public async Task HasMany_relationship_without_AllowSet_does_not_appear_in_create_request_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.relationshipsInCreateArticleRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().NotContainPath("categories");
        });
    }

    [Fact]
    public async Task HasMany_relationship_with_AllowSet_appears_in_update_request_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.relationshipsInUpdateBookRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().ContainProperty("reviews");
        });
    }

    [Fact]
    public async Task HasMany_relationship_without_AllowSet_does_not_appear_in_update_request_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.relationshipsInUpdateArticleRequest.allOf[1].properties").With(properties =>
        {
            properties.Should().NotContainPath("categories");
        });
    }

    [Fact]
    public async Task HasMany_relationship_with_AllowAdd_has_relationship_post_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./articles/{id}/relationships/tags.post");
    }

    [Fact]
    public async Task HasMany_relationship_without_AllowAdd_does_not_have_relationship_post_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().NotContainPath("paths./articles/{id}/relationships/categories.post");
        document.Should().NotContainPath("paths./books/{id}/relationships/reviews.post");
    }

    [Fact]
    public async Task HasMany_relationship_with_AllowRemove_has_relationship_delete_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./articles/{id}/relationships/comments.delete");
    }

    [Fact]
    public async Task HasMany_relationship_without_AllowRemove_does_not_have_relationship_delete_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().NotContainPath("paths./articles/{id}/relationships/categories.delete");
        document.Should().NotContainPath("paths./books/{id}/relationships/reviews.delete");
    }
}