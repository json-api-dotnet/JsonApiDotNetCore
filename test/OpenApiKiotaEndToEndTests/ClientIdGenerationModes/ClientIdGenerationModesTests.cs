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
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext>>, IDisposable
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
        Player newPlayer = _fakers.Player.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ClientIdGenerationModesClient apiClient = new(requestAdapter);

        var requestBody = new CreatePlayerRequestDocument
        {
            Data = new DataInCreatePlayerRequest
            {
                Type = PlayerResourceType.Players,
                Attributes = new AttributesInCreatePlayerRequest
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
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("422");
        error.Title.Should().Be("Failed to deserialize request body: The 'id' element is required.");
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ClientIdGenerationModesClient apiClient = new(requestAdapter);

        var requestBody = new CreatePlayerRequestDocument
        {
            Data = new DataInCreatePlayerRequest
            {
                Type = PlayerResourceType.Players,
                Id = newPlayer.Id,
                Attributes = new AttributesInCreatePlayerRequest
                {
                    UserName = newPlayer.UserName
                }
            }
        };

        // Act
        PlayerPrimaryResponseDocument? response = await apiClient.Players.PostAsync(requestBody);

        // Assert
        response.Should().BeNull();

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ClientIdGenerationModesClient apiClient = new(requestAdapter);

        var requestBody = new CreateGameRequestDocument
        {
            Data = new DataInCreateGameRequest
            {
                Type = GameResourceType.Games,
                Attributes = new AttributesInCreateGameRequest
                {
                    Title = newGame.Title,
                    PurchasePrice = (double)newGame.PurchasePrice
                }
            }
        };

        // Act
        GamePrimaryResponseDocument? response = await apiClient.Games.PostAsync(requestBody);

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().NotBeNull();
        response.Data.Id.Value.Should().NotBe(Guid.Empty);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.FirstWithIdAsync(response.Data.Id.Value);

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ClientIdGenerationModesClient apiClient = new(requestAdapter);

        var requestBody = new CreateGameRequestDocument
        {
            Data = new DataInCreateGameRequest
            {
                Type = GameResourceType.Games,
                Id = newGame.Id,
                Attributes = new AttributesInCreateGameRequest
                {
                    Title = newGame.Title,
                    PurchasePrice = (double)newGame.PurchasePrice
                }
            }
        };

        // Act
        GamePrimaryResponseDocument? response = await apiClient.Games.PostAsync(requestBody);

        // Assert
        response.Should().BeNull();

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ClientIdGenerationModesClient apiClient = new(requestAdapter);

        var requestBody = new CreateGameRequestDocument
        {
            Data = new DataInCreateGameRequest
            {
                Type = GameResourceType.Games,
                Id = existingGame.Id,
                Attributes = new AttributesInCreateGameRequest
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
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors.ElementAt(0);
        error.Status.Should().Be("409");
        error.Title.Should().Be("Another resource with the specified ID already exists.");
        error.Detail.Should().Be($"Another resource of type 'games' with ID '{existingGame.StringId}' already exists.");
    }

    [Fact]
    public async Task Can_create_resource_without_ID_when_supplying_ID_is_forbidden()
    {
        // Arrange
        PlayerGroup newPlayerGroup = _fakers.Group.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ClientIdGenerationModesClient apiClient = new(requestAdapter);

        var requestBody = new CreatePlayerGroupRequestDocument
        {
            Data = new DataInCreatePlayerGroupRequest
            {
                Type = PlayerGroupResourceType.PlayerGroups,
                Attributes = new AttributesInCreatePlayerGroupRequest
                {
                    Name = newPlayerGroup.Name
                }
            }
        };

        // Act
        PlayerGroupPrimaryResponseDocument? response = await apiClient.PlayerGroups.PostAsync(requestBody);

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().NotBeNullOrEmpty();

        long newPlayerGroupId = long.Parse(response.Data.Id);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            PlayerGroup playerGroupInDatabase = await dbContext.PlayerGroups.FirstWithIdAsync(newPlayerGroupId);

            playerGroupInDatabase.Name.Should().Be(newPlayerGroup.Name);
        });
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
