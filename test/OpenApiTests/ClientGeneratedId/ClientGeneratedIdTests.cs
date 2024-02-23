using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.ClientGeneratedId;

public sealed class ClientGeneratedIdTests : IClassFixture<OpenApiTestContext<OpenApiStartup<ClientGeneratedIdDbContext>, ClientGeneratedIdDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<ClientGeneratedIdDbContext>, ClientGeneratedIdDbContext> _testContext;

    public ClientGeneratedIdTests(OpenApiTestContext<OpenApiStartup<ClientGeneratedIdDbContext>, ClientGeneratedIdDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PlayersController>();
        testContext.UseController<GamesController>();
        testContext.UseController<GroupsController>();

        testContext.SwaggerDocumentOutputDirectory = "test/OpenApiEndToEndTests/ClientGeneratedId";
    }

    [Fact]
    public async Task Post_data_should_have_required_id()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.playerDataInPostRequest").With(resourceDataInPostRequestElement =>
        {
            resourceDataInPostRequestElement.Should().ContainPath("required").With(requiredElement =>
            {
                requiredElement.Should().ContainArrayElement("id");
            });

            resourceDataInPostRequestElement.Should().ContainPath("properties.id");
        });
    }

    [Fact]
    public async Task Post_data_should_have_non_required_id()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.gameDataInPostRequest").With(resourceDataInPostRequestElement =>
        {
            resourceDataInPostRequestElement.Should().ContainPath("required").With(requiredElement =>
            {
                requiredElement.Should().NotContainArrayElement("id");
            });

            resourceDataInPostRequestElement.Should().ContainPath("properties.id");
        });
    }

    [Fact]
    public async Task Post_data_should_not_have_id()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.groupDataInPostRequest").With(resourceDataInPostRequestElement =>
        {
            resourceDataInPostRequestElement.Should().ContainPath("required").With(requiredElement =>
            {
                requiredElement.Should().NotContainArrayElement("id");
            });

            resourceDataInPostRequestElement.Should().NotContainPath("properties.id");
        });
    }
}
