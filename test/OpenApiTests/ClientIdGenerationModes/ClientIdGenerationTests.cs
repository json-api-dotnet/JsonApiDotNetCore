using System.Text.Json;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.ClientIdGenerationModes;

public sealed class ClientIdGenerationTests : IClassFixture<OpenApiTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext> _testContext;

    public ClientIdGenerationTests(OpenApiTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<PlayersController>();
        testContext.UseController<GamesController>();
        testContext.UseController<PlayerGroupsController>();

        testContext.SetTestOutputHelper(testOutputHelper);
        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Fact]
    public async Task Schema_property_for_ID_is_required_in_post_request()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.dataInCreatePlayerRequest.allOf[1]").With(dataElement =>
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
        document.Should().ContainPath("components.schemas.dataInCreateGameRequest.allOf[1]").With(dataElement =>
        {
            dataElement.Should().NotContainPath("required");
        });
    }

    [Fact]
    public async Task Schema_property_for_ID_is_omitted_in_post_request()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.dataInCreatePlayerGroupRequest.allOf[1]").With(dataElement =>
        {
            dataElement.Should().NotContainPath("required");
        });
    }
}
