using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.ClientIdGenerationModes;

public sealed class ClientIdGenerationTests : IClassFixture<OpenApiTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext> _testContext;

    public ClientIdGenerationTests(OpenApiTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PlayersController>();
        testContext.UseController<GamesController>();
        testContext.UseController<PlayerGroupsController>();

        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Fact]
    public async Task Schema_property_for_ID_is_required_in_post_request()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.dataInCreatePlayerRequest").With(dataElement =>
        {
            dataElement.Should().ContainPath("required").With(requiredElement =>
            {
                requiredElement.Should().ContainArrayElement("id");
                requiredElement.Should().NotContainArrayElement("lid");
            });

            dataElement.Should().ContainPath("properties.id");
        });
    }

    [Fact]
    public async Task Schema_property_for_ID_is_optional_in_post_request()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.dataInCreateGameRequest").With(dataElement =>
        {
            dataElement.Should().ContainPath("required").With(requiredElement =>
            {
                requiredElement.Should().NotContainArrayElement("id");
                requiredElement.Should().NotContainArrayElement("lid");
            });

            dataElement.Should().ContainPath("properties.id");
        });
    }

    [Fact]
    public async Task Schema_property_for_ID_is_omitted_in_post_request()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.dataInCreatePlayerGroupRequest").With(dataElement =>
        {
            dataElement.Should().ContainPath("required").With(requiredElement =>
            {
                requiredElement.Should().NotContainArrayElement("id");
                requiredElement.Should().NotContainArrayElement("lid");
            });

            dataElement.Should().NotContainPath("properties.id");
        });
    }
}
