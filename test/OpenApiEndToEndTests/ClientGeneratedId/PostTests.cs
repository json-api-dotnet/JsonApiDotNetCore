using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.OpenApi.Client;
using Newtonsoft.Json;
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
        testContext.UseController<GroupsController>();
    }

    [Fact]
    public async Task Omit_required_id()
    {
        // Arrange
        Player player = _fakers.Player.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientGeneratedIdClient apiClient = new(httpClient);

        // Act
        Func<Task<PlayerPrimaryResponseDocument?>> action = () => ApiResponse.TranslateAsync(() => apiClient.PostPlayerAsync(null, new PlayerPostRequestDocument
        {
            Data = new PlayerDataInPostRequest
            {
                Id = null!,
                Attributes = new PlayerAttributesInPostRequest
                {
                    Name = player.Name
                }
            }
        }));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        assertion.Which.Message.Should().Be("Cannot write a null value for property 'id'. Property requires a value. Path 'data'.");
    }

    [Fact]
    public async Task Pass_required_id()
    {
        // Arrange
        Player player = _fakers.Player.Generate();
        player.Id = Guid.NewGuid();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientGeneratedIdClient apiClient = new(httpClient);

        // Act
        Func<Task<PlayerPrimaryResponseDocument?>> action = () => ApiResponse.TranslateAsync(() => apiClient.PostPlayerAsync(null, new PlayerPostRequestDocument
        {
            Data = new PlayerDataInPostRequest
            {
                Id = player.StringId!,
                Attributes = new PlayerAttributesInPostRequest
                {
                    Name = player.Name
                }
            }
        }));

        // Assert
        PlayerPrimaryResponseDocument? doc = (await action.Should().NotThrowAsync()).Subject;
        doc.Should().BeNull();
    }

    [Fact]
    public async Task Omit_allowed_id()
    {
        // Arrange
        Game game = _fakers.Game.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientGeneratedIdClient apiClient = new(httpClient);

        // Act
        Func<Task<GamePrimaryResponseDocument?>> action = () => ApiResponse.TranslateAsync(() => apiClient.PostGameAsync(null, new GamePostRequestDocument
        {
            Data = new GameDataInPostRequest
            {
                Id = null!,
                Attributes = new GameAttributesInPostRequest
                {
                    Name = game.Name,
                    Price = (double)game.Price
                }
            }
        }));

        // Assert
        GamePrimaryResponseDocument? doc = (await action.Should().NotThrowAsync()).Subject;
        doc?.Data.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Pass_allowed_id()
    {
        // Arrange
        Game game = _fakers.Game.Generate();
        game.Id = Guid.NewGuid();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientGeneratedIdClient apiClient = new(httpClient);

        // Act
        Func<Task<GamePrimaryResponseDocument?>> action = () => ApiResponse.TranslateAsync(() => apiClient.PostGameAsync(null, new GamePostRequestDocument
        {
            Data = new GameDataInPostRequest
            {
                Id = game.StringId!,
                Attributes = new GameAttributesInPostRequest
                {
                    Name = game.Name,
                    Price = (double)game.Price
                }
            }
        }));

        // Assert
        GamePrimaryResponseDocument? doc = (await action.Should().NotThrowAsync()).Subject;
        doc.Should().BeNull();
    }

    [Fact]
    public async Task Omit_forbidden_id()
    {
        // Arrange
        Group group = _fakers.Group.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientGeneratedIdClient apiClient = new(httpClient);

        // Act
        Func<Task<GroupPrimaryResponseDocument?>> action = () => ApiResponse.TranslateAsync(() => apiClient.PostGroupAsync(null, new GroupPostRequestDocument
        {
            Data = new GroupDataInPostRequest
            {
                Attributes = new GroupAttributesInPostRequest
                {
                    Name = group.Name
                }
            }
        }));

        // Assert
        GroupPrimaryResponseDocument? doc = (await action.Should().NotThrowAsync()).Subject;
        doc?.Data.Id.Should().NotBeNullOrEmpty();
    }
}
