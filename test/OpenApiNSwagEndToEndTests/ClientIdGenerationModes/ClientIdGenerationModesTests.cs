using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Newtonsoft.Json;
using OpenApiNSwagEndToEndTests.ClientIdGenerationModes.GeneratedCode;
using OpenApiTests;
using OpenApiTests.ClientIdGenerationModes;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiNSwagEndToEndTests.ClientIdGenerationModes;

public sealed class ClientIdGenerationModesTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext> _testContext;
    private readonly ClientIdGenerationFakers _fakers = new();

    public ClientIdGenerationModesTests(IntegrationTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PlayersController>();
        testContext.UseController<GamesController>();
        testContext.UseController<PlayerGroupsController>();
    }

    [Fact]
    public async Task Cannot_create_resource_without_ID_when_supplying_ID_is_required()
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
    public async Task Can_create_resource_with_ID_when_supplying_ID_is_required()
    {
        // Arrange
        Player player = _fakers.Player.Generate();
        player.Id = Guid.NewGuid();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientIdGenerationModesClient apiClient = new(httpClient);

        // Act
        PlayerPrimaryResponseDocument? document = await ApiResponse.TranslateAsync(() => apiClient.PostPlayerAsync(null, new PlayerPostRequestDocument
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
        document.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Player playerInDatabase = await dbContext.Players.FirstWithIdAsync(player.Id);

            playerInDatabase.UserName.Should().Be(player.UserName);
        });
    }

    [Fact]
    public async Task Can_create_resource_without_ID_when_supplying_ID_is_allowed()
    {
        // Arrange
        Game game = _fakers.Game.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientIdGenerationModesClient apiClient = new(httpClient);

        // Act
        GamePrimaryResponseDocument? document = await ApiResponse.TranslateAsync(() => apiClient.PostGameAsync(null, new GamePostRequestDocument
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
        document.ShouldNotBeNull();
        document.Data.Id.Should().NotBeNullOrEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.FirstWithIdAsync(Guid.Parse(document.Data.Id));

            gameInDatabase.Title.Should().Be(game.Title);
            gameInDatabase.PurchasePrice.Should().Be(game.PurchasePrice);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_ID_when_supplying_ID_is_allowed()
    {
        // Arrange
        Game game = _fakers.Game.Generate();
        game.Id = Guid.NewGuid();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientIdGenerationModesClient apiClient = new(httpClient);

        // Act
        GamePrimaryResponseDocument? document = await ApiResponse.TranslateAsync(() => apiClient.PostGameAsync(null, new GamePostRequestDocument
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
        document.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.FirstWithIdAsync(game.Id);

            gameInDatabase.Title.Should().Be(game.Title);
            gameInDatabase.PurchasePrice.Should().Be(game.PurchasePrice);
        });
    }

    [Fact]
    public async Task Can_create_resource_without_ID_when_supplying_ID_is_forbidden()
    {
        // Arrange
        PlayerGroup playerGroup = _fakers.Group.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientIdGenerationModesClient apiClient = new(httpClient);

        // Act
        PlayerGroupPrimaryResponseDocument? document = await ApiResponse.TranslateAsync(() => apiClient.PostPlayerGroupAsync(null,
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
        document.ShouldNotBeNull();
        document.Data.Id.Should().NotBeNullOrEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            PlayerGroup playerGroupInDatabase = await dbContext.PlayerGroups.FirstWithIdAsync(long.Parse(document.Data.Id));

            playerGroupInDatabase.Name.Should().Be(playerGroup.Name);
        });
    }
}
