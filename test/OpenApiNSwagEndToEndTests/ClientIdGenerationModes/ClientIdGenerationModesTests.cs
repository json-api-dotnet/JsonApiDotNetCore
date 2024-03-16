using System.Net;
using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Newtonsoft.Json;
using OpenApiNSwagEndToEndTests.ClientIdGenerationModes.GeneratedCode;
using OpenApiTests;
using OpenApiTests.ClientIdGenerationModes;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.ClientIdGenerationModes;

public sealed class ClientIdGenerationModesTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly ClientIdGenerationFakers _fakers = new();

    public ClientIdGenerationModesTests(IntegrationTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<PlayersController>();
        testContext.UseController<GamesController>();
        testContext.UseController<PlayerGroupsController>();
    }

    [Fact]
    public async Task Cannot_create_resource_without_ID_when_supplying_ID_is_required()
    {
        // Arrange
        Player newPlayer = _fakers.Player.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new PlayerPostRequestDocument
        {
            Data = new PlayerDataInPostRequest
            {
                Attributes = new PlayerAttributesInPostRequest
                {
                    UserName = newPlayer.UserName
                }
            }
        };

        // Act
        Func<Task<PlayerPrimaryResponseDocument?>> action = () => ApiResponse.TranslateAsync(() => apiClient.PostPlayerAsync(null, requestBody));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        assertion.Which.Message.Should().Be("Cannot write a null value for property 'id'. Property requires a value. Path 'data'.");
    }

    [Fact]
    public async Task Can_create_resource_with_ID_when_supplying_ID_is_required()
    {
        // Arrange
        Player newPlayer = _fakers.Player.Generate();
        newPlayer.Id = Guid.NewGuid();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new PlayerPostRequestDocument
        {
            Data = new PlayerDataInPostRequest
            {
                Id = newPlayer.StringId!,
                Attributes = new PlayerAttributesInPostRequest
                {
                    UserName = newPlayer.UserName
                }
            }
        };

        // Act
        PlayerPrimaryResponseDocument? document = await ApiResponse.TranslateAsync(() => apiClient.PostPlayerAsync(null, requestBody));

        // Assert
        document.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Player playerInDatabase = await dbContext.Players.FirstWithIdAsync(newPlayer.Id);

            playerInDatabase.UserName.Should().Be(newPlayer.UserName);
        });
    }

    [Fact]
    public async Task Can_create_resource_without_ID_when_supplying_ID_is_allowed()
    {
        // Arrange
        Game newGame = _fakers.Game.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new GamePostRequestDocument
        {
            Data = new GameDataInPostRequest
            {
                Attributes = new GameAttributesInPostRequest
                {
                    Title = newGame.Title,
                    PurchasePrice = (double)newGame.PurchasePrice
                }
            }
        };

        // Act
        GamePrimaryResponseDocument? document = await ApiResponse.TranslateAsync(() => apiClient.PostGameAsync(null, requestBody));

        // Assert
        document.ShouldNotBeNull();
        document.Data.Id.ShouldNotBeNullOrEmpty();

        Guid newGameId = Guid.Parse(document.Data.Id);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.FirstWithIdAsync(newGameId);

            gameInDatabase.Title.Should().Be(newGame.Title);
            gameInDatabase.PurchasePrice.Should().Be(newGame.PurchasePrice);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_ID_when_supplying_ID_is_allowed()
    {
        // Arrange
        Game newGame = _fakers.Game.Generate();
        newGame.Id = Guid.NewGuid();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new GamePostRequestDocument
        {
            Data = new GameDataInPostRequest
            {
                Id = newGame.StringId!,
                Attributes = new GameAttributesInPostRequest
                {
                    Title = newGame.Title,
                    PurchasePrice = (double)newGame.PurchasePrice
                }
            }
        };

        // Act
        GamePrimaryResponseDocument? document = await ApiResponse.TranslateAsync(() => apiClient.PostGameAsync(null, requestBody));

        // Assert
        document.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.FirstWithIdAsync(newGame.Id);

            gameInDatabase.Title.Should().Be(newGame.Title);
            gameInDatabase.PurchasePrice.Should().Be(newGame.PurchasePrice);
        });
    }

    [Fact]
    public async Task Cannot_create_resource_with_existing_ID_when_supplying_ID_is_allowed()
    {
        // Arrange
        Game existingGame = _fakers.Game.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Games.Add(existingGame);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new GamePostRequestDocument
        {
            Data = new GameDataInPostRequest
            {
                Id = existingGame.StringId!,
                Attributes = new GameAttributesInPostRequest
                {
                    Title = existingGame.Title,
                    PurchasePrice = (double)existingGame.PurchasePrice
                }
            }
        };

        // Act
        Func<Task> action = async () => _ = await apiClient.PostGameAsync(null, requestBody);

        // Assert
        ApiException<ErrorResponseDocument> exception = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which;
        exception.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        exception.Message.Should().Be("HTTP 409: The request body contains conflicting information or another resource with the same ID already exists.");
        exception.Result.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Result.Errors.ElementAt(0);
        error.Status.Should().Be("409");
        error.Title.Should().Be("Another resource with the specified ID already exists.");
        error.Detail.Should().Be($"Another resource of type 'games' with ID '{existingGame.StringId}' already exists.");
    }

    [Fact]
    public async Task Can_create_resource_without_ID_when_supplying_ID_is_forbidden()
    {
        // Arrange
        PlayerGroup newPlayerGroup = _fakers.Group.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new PlayerGroupPostRequestDocument
        {
            Data = new PlayerGroupDataInPostRequest
            {
                Attributes = new PlayerGroupAttributesInPostRequest
                {
                    Name = newPlayerGroup.Name
                }
            }
        };

        // Act
        PlayerGroupPrimaryResponseDocument? document = await ApiResponse.TranslateAsync(() => apiClient.PostPlayerGroupAsync(null, requestBody));

        // Assert
        document.ShouldNotBeNull();
        document.Data.Id.ShouldNotBeNullOrEmpty();

        long newPlayerGroupId = long.Parse(document.Data.Id);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            PlayerGroup playerGroupInDatabase = await dbContext.PlayerGroups.FirstWithIdAsync(newPlayerGroupId);

            playerGroupInDatabase.Name.Should().Be(newPlayerGroup.Name);
        });
    }
}
