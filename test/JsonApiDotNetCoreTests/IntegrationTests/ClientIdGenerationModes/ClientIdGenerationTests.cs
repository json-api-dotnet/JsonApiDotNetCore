using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ClientIdGenerationModes;

public sealed class ClientIdGenerationTests : IClassFixture<IntegrationTestContext<TestableStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext> _testContext;
    private readonly ClientIdGenerationFakers _fakers = new();

    public ClientIdGenerationTests(IntegrationTestContext<TestableStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<TournamentsController>();
        testContext.UseController<PlayersController>();
        testContext.UseController<GamesController>();
        testContext.UseController<PlayerGroupsController>();
    }

    [Fact]
    public async Task Cannot_create_resource_without_ID_when_supplying_ID_is_required()
    {
        // Arrange
        Player newPlayer = _fakers.Player.GenerateOne();

        var requestBody = new
        {
            data = new
            {
                type = "players",
                attributes = new
                {
                    userName = newPlayer.UserName
                }
            }
        };

        const string route = "/players";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
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

        var requestBody = new
        {
            data = new
            {
                type = "players",
                id = newPlayer.StringId,
                attributes = new
                {
                    userName = newPlayer.UserName
                }
            }
        };

        const string route = "/players";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

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

        var requestBody = new
        {
            data = new
            {
                type = "games",
                attributes = new
                {
                    title = newGame.Title,
                    purchasePrice = newGame.PurchasePrice
                }
            }
        };

        const string route = "/games";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        Guid newGameId = Guid.Parse(responseDocument.Data.SingleValue.Id.Should().NotBeNull().And.Subject);
        newGameId.Should().NotBeEmpty();

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
        Game newGame = _fakers.Game.GenerateOne();
        newGame.Id = Guid.NewGuid();

        var requestBody = new
        {
            data = new
            {
                type = "games",
                id = newGame.StringId,
                attributes = new
                {
                    title = newGame.Title,
                    purchasePrice = newGame.PurchasePrice
                }
            }
        };

        const string route = "/games";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

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

        var requestBody = new
        {
            data = new
            {
                type = "games",
                id = existingGame.StringId,
                attributes = new
                {
                    title = existingGame.Title,
                    purchasePrice = existingGame.PurchasePrice
                }
            }
        };

        const string route = "/games";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Another resource with the specified ID already exists.");
        error.Detail.Should().Be($"Another resource of type 'games' with ID '{existingGame.StringId}' already exists.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_create_resource_without_ID_when_supplying_ID_is_forbidden()
    {
        // Arrange
        PlayerGroup newPlayerGroup = _fakers.Group.GenerateOne();

        var requestBody = new
        {
            data = new
            {
                type = "playerGroups",
                attributes = new
                {
                    name = newPlayerGroup.Name
                }
            }
        };

        const string route = "/playerGroups";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        long newPlayerGroupId = long.Parse(responseDocument.Data.SingleValue.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            PlayerGroup playerGroupInDatabase = await dbContext.PlayerGroups.FirstWithIdAsync(newPlayerGroupId);

            playerGroupInDatabase.Name.Should().Be(newPlayerGroup.Name);
        });
    }

    [Fact]
    public async Task Cannot_create_resource_with_ID_when_supplying_ID_is_forbidden()
    {
        // Arrange
        PlayerGroup newPlayerGroup = _fakers.Group.GenerateOne();

        var requestBody = new
        {
            data = new
            {
                type = "playerGroups",
                id = "12345",
                attributes = new
                {
                    name = newPlayerGroup.Name
                }
            }
        };

        const string route = "/playerGroups";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("Failed to deserialize request body: The use of client-generated IDs is disabled.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/id");
    }

    [Fact]
    public async Task Can_create_resource_without_ID_when_supplying_ID_is_globally_forbidden()
    {
        // Arrange
        Tournament newTournament = _fakers.Tournament.GenerateOne();

        var requestBody = new
        {
            data = new
            {
                type = "tournaments",
                attributes = new
                {
                    title = newTournament.Title
                }
            }
        };

        const string route = "/tournaments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        long newTournamentId = long.Parse(responseDocument.Data.SingleValue.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Tournament tournamentInDatabase = await dbContext.Tournaments.FirstWithIdAsync(newTournamentId);

            tournamentInDatabase.Title.Should().Be(newTournament.Title);
        });
    }

    [Fact]
    public async Task Cannot_create_resource_with_ID_when_supplying_ID_is_globally_forbidden()
    {
        // Arrange
        Tournament newTournament = _fakers.Tournament.GenerateOne();

        var requestBody = new
        {
            data = new
            {
                type = "tournaments",
                id = "12345",
                attributes = new
                {
                    title = newTournament.Title
                }
            }
        };

        const string route = "/tournaments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("Failed to deserialize request body: The use of client-generated IDs is disabled.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/id");
    }
}
