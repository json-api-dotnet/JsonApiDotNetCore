using System.Net;
using FluentAssertions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode;
using OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.ClientIdGenerationModes;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.ClientIdGenerationModes;

public sealed class ClientIdGenerationModesTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly ClientIdGenerationFakers _fakers = new();

    public ClientIdGenerationModesTests(IntegrationTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<PlayersController>();
        testContext.UseController<GamesController>();
        testContext.UseController<PlayerGroupsController>();
    }

    [Fact]
    public async Task Cannot_create_resource_without_ID_when_supplying_ID_is_required()
    {
        // Arrange
        Player newPlayer = _fakers.Player.Generate();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ClientIdGenerationModesClient apiClient = new(requestAdapter);

        var requestBody = new PlayerPostRequestDocument
        {
            Data = new PlayerDataInPostRequest
            {
                Type = PlayerResourceType.Players,
                Attributes = new PlayerAttributesInPostRequest
                {
                    UserName = newPlayer.UserName
                }
            }
        };

        // Act
        Func<Task> action = async () => _ = await apiClient.Players.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("422");
        error.Title.Should().Be("Failed to deserialize request body: The 'id' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data");
    }

    [Fact]
    public async Task Can_create_resource_with_ID_when_supplying_ID_is_required()
    {
        // Arrange
        Player newPlayer = _fakers.Player.Generate();
        newPlayer.Id = Guid.NewGuid();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ClientIdGenerationModesClient apiClient = new(requestAdapter);

        var requestBody = new PlayerPostRequestDocument
        {
            Data = new PlayerDataInPostRequest
            {
                Type = PlayerResourceType.Players,
                Id = newPlayer.StringId,
                Attributes = new PlayerAttributesInPostRequest
                {
                    UserName = newPlayer.UserName
                }
            }
        };

        // Act
        PlayerPrimaryResponseDocument? document = await apiClient.Players.PostAsync(requestBody);

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ClientIdGenerationModesClient apiClient = new(requestAdapter);

        var requestBody = new GamePostRequestDocument
        {
            Data = new GameDataInPostRequest
            {
                Type = GameResourceType.Games,
                Attributes = new GameAttributesInPostRequest
                {
                    Title = newGame.Title,
                    PurchasePrice = (double)newGame.PurchasePrice
                }
            }
        };

        // Act
        GamePrimaryResponseDocument? document = await apiClient.Games.PostAsync(requestBody);

        // Assert
        document.ShouldNotBeNull();
        document.Data.ShouldNotBeNull();
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ClientIdGenerationModesClient apiClient = new(requestAdapter);

        var requestBody = new GamePostRequestDocument
        {
            Data = new GameDataInPostRequest
            {
                Type = GameResourceType.Games,
                Id = newGame.StringId,
                Attributes = new GameAttributesInPostRequest
                {
                    Title = newGame.Title,
                    PurchasePrice = (double)newGame.PurchasePrice
                }
            }
        };

        // Act
        GamePrimaryResponseDocument? document = await apiClient.Games.PostAsync(requestBody);

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ClientIdGenerationModesClient apiClient = new(requestAdapter);

        var requestBody = new GamePostRequestDocument
        {
            Data = new GameDataInPostRequest
            {
                Type = GameResourceType.Games,
                Id = existingGame.StringId,
                Attributes = new GameAttributesInPostRequest
                {
                    Title = existingGame.Title,
                    PurchasePrice = (double)existingGame.PurchasePrice
                }
            }
        };

        // Act
        Func<Task> action = async () => _ = await apiClient.Games.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.Conflict);
        exception.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Errors.ElementAt(0);
        error.Status.Should().Be("409");
        error.Title.Should().Be("Another resource with the specified ID already exists.");
        error.Detail.Should().Be($"Another resource of type 'games' with ID '{existingGame.StringId}' already exists.");
    }

    [Fact]
    public async Task Can_create_resource_without_ID_when_supplying_ID_is_forbidden()
    {
        // Arrange
        PlayerGroup newPlayerGroup = _fakers.Group.Generate();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ClientIdGenerationModesClient apiClient = new(requestAdapter);

        var requestBody = new PlayerGroupPostRequestDocument
        {
            Data = new PlayerGroupDataInPostRequest
            {
                Type = PlayerGroupResourceType.PlayerGroups,
                Attributes = new PlayerGroupAttributesInPostRequest
                {
                    Name = newPlayerGroup.Name
                }
            }
        };

        // Act
        PlayerGroupPrimaryResponseDocument? document = await apiClient.PlayerGroups.PostAsync(requestBody);

        // Assert
        document.ShouldNotBeNull();
        document.Data.ShouldNotBeNull();
        document.Data.Id.ShouldNotBeNullOrEmpty();

        long newPlayerGroupId = long.Parse(document.Data.Id);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            PlayerGroup playerGroupInDatabase = await dbContext.PlayerGroups.FirstWithIdAsync(newPlayerGroupId);

            playerGroupInDatabase.Name.Should().Be(newPlayerGroup.Name);
        });
    }
}
