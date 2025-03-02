using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using OpenApiNSwagEndToEndTests.ClientIdGenerationModes.GeneratedCode;
using OpenApiTests;
using OpenApiTests.ClientIdGenerationModes;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.ClientIdGenerationModes;

public sealed class ClientIdGenerationModesTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext>>, IDisposable
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
        Player newPlayer = _fakers.Player.GenerateOne();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new CreatePlayerRequestDocument
        {
            Data = new DataInCreatePlayerRequest
            {
                Attributes = new AttributesInCreatePlayerRequest
                {
                    UserName = newPlayer.UserName
                }
            }
        };

        // Act
        Func<Task> action = async () => await ApiResponse.TranslateAsync(async () => await apiClient.PostPlayerAsync(requestBody));

        // Assert
        ApiException<ErrorResponseDocument> exception = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which;
        exception.StatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be("HTTP 422: Validation of the request body failed.");
        exception.Result.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Result.Errors.ElementAt(0);
        error.Status.Should().Be("422");
        error.Title.Should().Be("Failed to deserialize request body: The 'id' element is invalid.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data");
    }

    [Fact]
    public async Task Can_create_resource_with_ID_when_supplying_ID_is_required()
    {
        // Arrange
        Player newPlayer = _fakers.Player.GenerateOne();
        newPlayer.Id = Guid.NewGuid();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new CreatePlayerRequestDocument
        {
            Data = new DataInCreatePlayerRequest
            {
                Id = newPlayer.Id,
                Attributes = new AttributesInCreatePlayerRequest
                {
                    UserName = newPlayer.UserName
                }
            }
        };

        // Act
        PlayerPrimaryResponseDocument? document = await ApiResponse.TranslateAsync(async () => await apiClient.PostPlayerAsync(requestBody));

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
        Game newGame = _fakers.Game.GenerateOne();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new CreateGameRequestDocument
        {
            Data = new DataInCreateGameRequest
            {
                Attributes = new AttributesInCreateGameRequest
                {
                    Title = newGame.Title,
                    PurchasePrice = (double)newGame.PurchasePrice
                }
            }
        };

        // Act
        GamePrimaryResponseDocument? document = await ApiResponse.TranslateAsync(async () => await apiClient.PostGameAsync(requestBody));

        // Assert
        document.Should().NotBeNull();
        document.Data.Id.Should().NotBe(Guid.Empty);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.FirstWithIdAsync(document.Data.Id);

            gameInDatabase.Title.Should().Be(newGame.Title);
            gameInDatabase.PurchasePrice.Should().Be(newGame.PurchasePrice);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_ID_when_supplying_ID_is_allowed()
    {
        // Arrange
        Game newGame = _fakers.Game.GenerateOne();
        newGame.Id = Guid.NewGuid();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new CreateGameRequestDocument
        {
            Data = new DataInCreateGameRequest
            {
                Id = newGame.Id,
                Attributes = new AttributesInCreateGameRequest
                {
                    Title = newGame.Title,
                    PurchasePrice = (double)newGame.PurchasePrice
                }
            }
        };

        // Act
        GamePrimaryResponseDocument? document = await ApiResponse.TranslateAsync(async () => await apiClient.PostGameAsync(requestBody));

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
        Game existingGame = _fakers.Game.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Games.Add(existingGame);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new CreateGameRequestDocument
        {
            Data = new DataInCreateGameRequest
            {
                Id = existingGame.Id,
                Attributes = new AttributesInCreateGameRequest
                {
                    Title = existingGame.Title,
                    PurchasePrice = (double)existingGame.PurchasePrice
                }
            }
        };

        // Act
        Func<Task> action = async () => _ = await apiClient.PostGameAsync(requestBody);

        // Assert
        ApiException<ErrorResponseDocument> exception = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which;
        exception.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        exception.Message.Should().Be("HTTP 409: The request body contains conflicting information or another resource with the same ID already exists.");
        exception.Result.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Result.Errors.ElementAt(0);
        error.Status.Should().Be("409");
        error.Title.Should().Be("Another resource with the specified ID already exists.");
        error.Detail.Should().Be($"Another resource of type 'games' with ID '{existingGame.StringId}' already exists.");
    }

    [Fact]
    public async Task Can_create_resource_without_ID_when_supplying_ID_is_forbidden()
    {
        // Arrange
        PlayerGroup newPlayerGroup = _fakers.Group.GenerateOne();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new CreatePlayerGroupRequestDocument
        {
            Data = new DataInCreatePlayerGroupRequest
            {
                Attributes = new AttributesInCreatePlayerGroupRequest
                {
                    Name = newPlayerGroup.Name
                }
            }
        };

        // Act
        PlayerGroupPrimaryResponseDocument? document = await ApiResponse.TranslateAsync(async () => await apiClient.PostPlayerGroupAsync(requestBody));

        // Assert
        document.Should().NotBeNull();
        document.Data.Id.Should().NotBeNullOrEmpty();

        long newPlayerGroupId = long.Parse(document.Data.Id);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            PlayerGroup playerGroupInDatabase = await dbContext.PlayerGroups.FirstWithIdAsync(newPlayerGroupId);

            playerGroupInDatabase.Name.Should().Be(newPlayerGroup.Name);
        });
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
