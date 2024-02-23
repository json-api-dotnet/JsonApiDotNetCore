using FluentAssertions;
using OpenApiEndToEndTests.ClientGeneratedId.GeneratedCode;
using OpenApiTests;
using OpenApiTests.ClientGeneratedId;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiEndToEndTests.ClientGeneratedId;

public sealed class PostTests : IClassFixture<IntegrationTestContext<OpenApiStartup<ClientGeneratedIdDbContext>, ClientGeneratedIdDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<ClientGeneratedIdDbContext>, ClientGeneratedIdDbContext> _testContext;
    private readonly ClientGeneratedIdFakers _fakers = new();

    public PostTests(IntegrationTestContext<OpenApiStartup<ClientGeneratedIdDbContext>, ClientGeneratedIdDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PlayersController>();
        testContext.UseController<GamesController>();
    }

    [Fact]
    public async Task Returns_error_if_required_id_is_omitted()
    {
        // Arrange
        Player player = _fakers.Player.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientGeneratedIdClient apiClient = new(httpClient);

        // Act
        Func<Task<PlayerPrimaryResponseDocument>> action = () => apiClient.PostPlayerAsync(null, new PlayerPostRequestDocument
        {
            Data = new PlayerDataInPostRequest
            {
                Id = null!, // FIXME: passing "" here works fine 🤔
                Attributes = new PlayerAttributesInPostRequest
                {
                    Name = player.Name
                }
            }
        });

        // Assert
        var exception = (await action.Should().ThrowAsync<Exception>()).Subject.First();
        // Exception is Newtonsoft.Json.JsonSerializationException: Cannot write a null value for property 'id'. Property requires a value. Path 'data'.
        // Probably not what we want.
    }

    [Fact]
    public async Task Requires_passing_id()
    {
        // Arrange
        Player player = _fakers.Player.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientGeneratedIdClient apiClient = new(httpClient);

        // Act
        Func<Task<PlayerPrimaryResponseDocument>> action = () => apiClient.PostPlayerAsync(null, new PlayerPostRequestDocument
        {
            Data = new PlayerDataInPostRequest
            {
                Id = player.StringId!,
                Attributes = new PlayerAttributesInPostRequest
                {
                    Name = player.Name
                }
            }
        });

        // Assert
        PlayerPrimaryResponseDocument doc = (await action.Should().NotThrowAsync()).Subject;
        doc.Data.Id.Should().Be(player.StringId);
    }

    [Fact]
    public async Task Allows_passing_id()
    {
        // Arrange
        Game game = _fakers.Game.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientGeneratedIdClient apiClient = new(httpClient);

        // Act
        Func<Task<GamePrimaryResponseDocument>> action = () => apiClient.PostGameAsync(null, new GamePostRequestDocument
        {
            Data = new GameDataInPostRequest
            {
                Id = game.StringId!, // FIXME: StringId is null, how to generate an id?
                Attributes = new GameAttributesInPostRequest
                {
                    Name = game.Name,
                    Price = (double)game.Price
                }
            }
        });

        // Assert
        GamePrimaryResponseDocument doc = (await action.Should().NotThrowAsync()).Subject;
        doc.Data.Id.Should().Be(game.StringId);
    }

    [Fact]
    public async Task Allow_omitting_id()
    {
        // Arrange
        Game game = _fakers.Game.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientGeneratedIdClient apiClient = new(httpClient);

        // Act
        Func<Task<GamePrimaryResponseDocument>> action = () => apiClient.PostGameAsync(null, new GamePostRequestDocument
        {
            Data = new GameDataInPostRequest
            {
                Id = null!, // FIXME: incorrect nullability here
                Attributes = new GameAttributesInPostRequest
                {
                    Name = game.Name,
                    Price = (double)game.Price
                }
            }
        });

        // Assert
        GamePrimaryResponseDocument doc = (await action.Should().NotThrowAsync()).Subject;
        doc.Data.Id.Should().NotBeNullOrEmpty();
    }
}