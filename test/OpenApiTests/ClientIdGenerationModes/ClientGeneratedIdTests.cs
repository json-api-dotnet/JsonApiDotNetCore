using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.ClientIdGenerationModes;

public sealed class ClientIdGenerationModesTests
    : IClassFixture<OpenApiTestContext<OpenApiStartup<ClientIdGenerationModesDbContext>, ClientIdGenerationModesDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<ClientIdGenerationModesDbContext>, ClientIdGenerationModesDbContext> _testContext;

    public ClientIdGenerationModesTests(OpenApiTestContext<OpenApiStartup<ClientIdGenerationModesDbContext>, ClientIdGenerationModesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PlayersController>();
        testContext.UseController<GamesController>();
        testContext.UseController<PlayerGroupsController>();

        testContext.SwaggerDocumentOutputDirectory = "test/OpenApiEndToEndTests/ClientIdGenerationModes";
    }

    [Fact]
    public async Task Post_data_should_have_required_id()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.playerDataInPostRequest").With(dataElement =>
        {
            dataElement.Should().ContainPath("required").With(requiredElement =>
            {
                requiredElement.Should().ContainArrayElement("id");
            });

            dataElement.Should().ContainPath("properties.id");
        });
    }

    [Fact]
    public async Task Post_data_should_have_non_required_id()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.gameDataInPostRequest").With(dataElement =>
        {
            dataElement.Should().ContainPath("required").With(requiredElement =>
            {
                requiredElement.Should().NotContainArrayElement("id");
            });

            dataElement.Should().ContainPath("properties.id");
        });
    }

    [Fact]
    public async Task Post_data_should_not_have_id()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.playerGroupDataInPostRequest").With(dataElement =>
        {
            dataElement.Should().ContainPath("required").With(requiredElement =>
            {
                requiredElement.Should().NotContainArrayElement("id");
            });

            dataElement.Should().NotContainPath("properties.id");
        });
    }
}
