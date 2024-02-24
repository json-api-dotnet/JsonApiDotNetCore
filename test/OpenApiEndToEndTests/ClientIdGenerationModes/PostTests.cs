using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.OpenApi.Client;
using Newtonsoft.Json;
using OpenApiEndToEndTests.ClientIdGenerationModes.GeneratedCode;
using OpenApiTests;
using OpenApiTests.ClientIdGenerationModes;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiEndToEndTests.ClientIdGenerationModes;

public sealed class PostTests : IClassFixture<IntegrationTestContext<OpenApiStartup<ClientIdGenerationModesDbContext>, ClientIdGenerationModesDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<ClientIdGenerationModesDbContext>, ClientIdGenerationModesDbContext> _testContext;
    private readonly ClientIdGenerationModesFakers _fakers = new();

    public PostTests(IntegrationTestContext<OpenApiStartup<ClientIdGenerationModesDbContext>, ClientIdGenerationModesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PlayersController>();
        testContext.UseController<GamesController>();
        testContext.UseController<PlayerGroupsController>();
    }

    [Fact]
    public async Task Cannot_create_resource_without_ID_when_mode_is_required()
    {
        // Arrange
        Player player = _fakers.Player.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientIdGenerationModesClient apiClient = new(httpClient);

        // Act
        Func<Task<PlayerPrimaryResponseDocument?>> action = () => ApiResponse.TranslateAsync(() => apiClient.PostPlayerAsync(null, new PlayerPostRequestDocument
        {
            Data = new PlayerDataInPostRequest
            {
                Id = null!,
                Attributes = new PlayerAttributesInPostRequest
                {
                    UserName = player.UserName
                }
            }
        }));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        assertion.Which.Message.Should().Be("Cannot write a null value for property 'id'. Property requires a value. Path 'data'.");
    }

    [Fact]
    public async Task Can_create_resource_with_ID_when_mode_is_required()
    {
        // Arrange
        Player player = _fakers.Player.Generate();
        player.Id = Guid.NewGuid();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientIdGenerationModesClient apiClient = new(httpClient);

        // Act
        Func<Task<PlayerPrimaryResponseDocument?>> action = () => ApiResponse.TranslateAsync(() => apiClient.PostPlayerAsync(null, new PlayerPostRequestDocument
        {
            Data = new PlayerDataInPostRequest
            {
                Id = player.StringId!,
                Attributes = new PlayerAttributesInPostRequest
                {
                    UserName = player.UserName
                }
            }
        }));

        // Assert
        PlayerPrimaryResponseDocument? doc = (await action.Should().NotThrowAsync()).Subject;
        doc.Should().BeNull();
    }

    [Fact]
    public async Task Can_create_resource_without_ID_when_mode_is_allowed()
    {
        // Arrange
        Game game = _fakers.Game.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientIdGenerationModesClient apiClient = new(httpClient);

        // Act
        Func<Task<GamePrimaryResponseDocument?>> action = () => ApiResponse.TranslateAsync(() => apiClient.PostGameAsync(null, new GamePostRequestDocument
        {
            Data = new GameDataInPostRequest
            {
                Id = null!,
                Attributes = new GameAttributesInPostRequest
                {
                    Title = game.Title,
                    PurchasePrice = (double)game.PurchasePrice
                }
            }
        }));

        // Assert
        GamePrimaryResponseDocument? doc = (await action.Should().NotThrowAsync()).Subject;
        doc?.Data.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Can_create_resource_with_ID_when_mode_is_allowed()
    {
        // Arrange
        Game game = _fakers.Game.Generate();
        game.Id = Guid.NewGuid();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientIdGenerationModesClient apiClient = new(httpClient);

        // Act
        Func<Task<GamePrimaryResponseDocument?>> action = () => ApiResponse.TranslateAsync(() => apiClient.PostGameAsync(null, new GamePostRequestDocument
        {
            Data = new GameDataInPostRequest
            {
                Id = game.StringId!,
                Attributes = new GameAttributesInPostRequest
                {
                    Title = game.Title,
                    PurchasePrice = (double)game.PurchasePrice
                }
            }
        }));

        // Assert
        GamePrimaryResponseDocument? doc = (await action.Should().NotThrowAsync()).Subject;
        doc.Should().BeNull();
    }

    [Fact]
    public async Task Can_create_resource_without_ID_when_mode_is_forbidden()
    {
        // Arrange
        PlayerGroup playerGroup = _fakers.Group.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientIdGenerationModesClient apiClient = new(httpClient);

        // Act
        Func<Task<PlayerGroupPrimaryResponseDocument?>> action = () => ApiResponse.TranslateAsync(() => apiClient.PostPlayerGroupAsync(null,
            new PlayerGroupPostRequestDocument
            {
                Data = new PlayerGroupDataInPostRequest
                {
                    Attributes = new PlayerGroupAttributesInPostRequest
                    {
                        Name = playerGroup.Name
                    }
                }
            }));

        // Assert
        PlayerGroupPrimaryResponseDocument? doc = (await action.Should().NotThrowAsync()).Subject;
        doc?.Data.Id.Should().NotBeNullOrEmpty();
    }
}
