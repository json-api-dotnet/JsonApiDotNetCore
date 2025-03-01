using System.Net;
using System.Reflection;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys;

public sealed class WhiteSpaceAsKeyTests : IClassFixture<IntegrationTestContext<TestableStartup<ZeroKeyDbContext>, ZeroKeyDbContext>>
{
    // An empty string id makes no sense: get-by-id, update and delete resource are impossible, and rendered links are unusable.
    private const string SingleSpace = " ";

    private readonly IntegrationTestContext<TestableStartup<ZeroKeyDbContext>, ZeroKeyDbContext> _testContext;
    private readonly ZeroKeyFakers _fakers = new();

    public WhiteSpaceAsKeyTests(IntegrationTestContext<TestableStartup<ZeroKeyDbContext>, ZeroKeyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PlayersController>();
        testContext.UseController<GamesController>();
        testContext.UseController<MapsController>();

        testContext.PostConfigureServices(services =>
        {
            ServiceDescriptor serviceDescriptor = services.Single(descriptor => descriptor.ServiceType == typeof(IModelMetadataProvider));
            services.Remove(serviceDescriptor);
            Type existingProviderType = serviceDescriptor.ImplementationType!;

            services.AddSingleton<IModelMetadataProvider>(serviceProvider =>
            {
                var existingProvider = (ModelMetadataProvider)ActivatorUtilities.CreateInstance(serviceProvider, existingProviderType);
                return new PreserveWhitespaceModelMetadataProvider(existingProvider);
            });
        });

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.UseRelativeLinks = true;
    }

    [Fact]
    public async Task Can_filter_by_space_ID_on_primary_resources()
    {
        // Arrange
        List<Player> players = _fakers.Player.GenerateList(2);
        players[0].Id = SingleSpace;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Player>();
            dbContext.Players.AddRange(players);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/players?filter=equals(id,' ')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(SingleSpace);

        responseDocument.Data.ManyValue[0].With(resource =>
        {
            resource.Links.ShouldNotBeNull();
            resource.Links.Self.Should().Be("/players/%20");
        });
    }

    [Fact]
    public async Task Can_get_primary_resource_by_space_ID_with_include()
    {
        // Arrange
        Player player = _fakers.Player.GenerateOne();
        player.Id = SingleSpace;
        player.ActiveGame = _fakers.Game.GenerateOne();
        player.ActiveGame.Id = 0;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTablesAsync<Player, Game>();
            dbContext.Players.Add(player);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/players/%20?include=activeGame";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(SingleSpace);
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Links.Self.Should().Be("/players/%20");

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Id.Should().Be("0");
    }

    [Fact]
    public async Task Can_create_resource_with_space_ID()
    {
        // Arrange
        string newEmailAddress = _fakers.Player.GenerateOne().EmailAddress;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Player>();
        });

        var requestBody = new
        {
            data = new
            {
                type = "players",
                id = SingleSpace,
                attributes = new
                {
                    emailAddress = newEmailAddress
                }
            }
        };

        const string route = "/players";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        httpResponse.Headers.Location.Should().Be("/players/%20");

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Player playerInDatabase = await dbContext.Players.FirstWithIdAsync((string?)SingleSpace);

            playerInDatabase.ShouldNotBeNull();
            playerInDatabase.EmailAddress.Should().Be(newEmailAddress);
        });
    }

    [Fact]
    public async Task Can_update_resource_with_space_ID()
    {
        // Arrange
        Player existingPlayer = _fakers.Player.GenerateOne();
        existingPlayer.Id = SingleSpace;

        string newEmailAddress = _fakers.Player.GenerateOne().EmailAddress;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Player>();
            dbContext.Players.Add(existingPlayer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "players",
                id = SingleSpace,
                attributes = new
                {
                    emailAddress = newEmailAddress
                }
            }
        };

        const string route = "/players/%20";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Player playerInDatabase = await dbContext.Players.FirstWithIdAsync((string?)SingleSpace);

            playerInDatabase.ShouldNotBeNull();
            playerInDatabase.EmailAddress.Should().Be(newEmailAddress);
        });
    }

    [Fact]
    public async Task Can_clear_ToOne_relationship_with_space_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();
        existingGame.Host = _fakers.Player.GenerateOne();
        existingGame.Host.Id = string.Empty;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Player>();
            dbContext.Games.Add(existingGame);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/games/{existingGame.StringId}/relationships/host";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.Host).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.ShouldNotBeNull();
            gameInDatabase.Host.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_assign_ToOne_relationship_with_space_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();

        Player existingPlayer = _fakers.Player.GenerateOne();
        existingPlayer.Id = SingleSpace;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Player>();
            dbContext.AddInRange(existingGame, existingPlayer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "players",
                id = SingleSpace
            }
        };

        string route = $"/games/{existingGame.StringId}/relationships/host";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.Host).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.ShouldNotBeNull();
            gameInDatabase.Host.ShouldNotBeNull();
            gameInDatabase.Host.Id.Should().Be(SingleSpace);
        });
    }

    [Fact]
    public async Task Can_replace_ToOne_relationship_with_space_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();
        existingGame.Host = _fakers.Player.GenerateOne();

        Player existingPlayer = _fakers.Player.GenerateOne();
        existingPlayer.Id = SingleSpace;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Player>();
            dbContext.AddInRange(existingGame, existingPlayer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "players",
                id = SingleSpace
            }
        };

        string route = $"/games/{existingGame.StringId}/relationships/host";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.Host).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.ShouldNotBeNull();
            gameInDatabase.Host.ShouldNotBeNull();
            gameInDatabase.Host.Id.Should().Be(SingleSpace);
        });
    }

    [Fact]
    public async Task Can_clear_ToMany_relationship_with_space_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();
        existingGame.ActivePlayers = _fakers.Player.GenerateList(2);
        existingGame.ActivePlayers.ElementAt(0).Id = SingleSpace;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Player>();
            dbContext.Games.Add(existingGame);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string route = $"/games/{existingGame.StringId}/relationships/activePlayers";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.ActivePlayers).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.ShouldNotBeNull();
            gameInDatabase.ActivePlayers.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_assign_ToMany_relationship_with_space_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();

        Player existingPlayer = _fakers.Player.GenerateOne();
        existingPlayer.Id = SingleSpace;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Player>();
            dbContext.AddInRange(existingGame, existingPlayer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "players",
                    id = SingleSpace
                }
            }
        };

        string route = $"/games/{existingGame.StringId}/relationships/activePlayers";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.ActivePlayers).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.ShouldNotBeNull();
            gameInDatabase.ActivePlayers.Should().HaveCount(1);
            gameInDatabase.ActivePlayers.ElementAt(0).Id.Should().Be(SingleSpace);
        });
    }

    [Fact]
    public async Task Can_replace_ToMany_relationship_with_space_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();
        existingGame.ActivePlayers = _fakers.Player.GenerateList(2);

        Player existingPlayer = _fakers.Player.GenerateOne();
        existingPlayer.Id = SingleSpace;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Player>();
            dbContext.AddInRange(existingGame, existingPlayer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "players",
                    id = SingleSpace
                }
            }
        };

        string route = $"/games/{existingGame.StringId}/relationships/activePlayers";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.ActivePlayers).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.ShouldNotBeNull();
            gameInDatabase.ActivePlayers.Should().HaveCount(1);
            gameInDatabase.ActivePlayers.ElementAt(0).Id.Should().Be(SingleSpace);
        });
    }

    [Fact]
    public async Task Can_add_to_ToMany_relationship_with_space_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();
        existingGame.ActivePlayers = _fakers.Player.GenerateList(1);

        Player existingPlayer = _fakers.Player.GenerateOne();
        existingPlayer.Id = SingleSpace;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Player>();
            dbContext.AddInRange(existingGame, existingPlayer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "players",
                    id = SingleSpace
                }
            }
        };

        string route = $"/games/{existingGame.StringId}/relationships/activePlayers";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.ActivePlayers).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.ShouldNotBeNull();
            gameInDatabase.ActivePlayers.Should().HaveCount(2);
            gameInDatabase.ActivePlayers.Should().ContainSingle(player => player.Id == SingleSpace);
        });
    }

    [Fact]
    public async Task Can_remove_from_ToMany_relationship_with_space_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();
        existingGame.ActivePlayers = _fakers.Player.GenerateList(2);
        existingGame.ActivePlayers.ElementAt(0).Id = SingleSpace;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Player>();
            dbContext.Games.Add(existingGame);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "players",
                    id = SingleSpace
                }
            }
        };

        string route = $"/games/{existingGame.StringId}/relationships/activePlayers";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.ActivePlayers).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.ShouldNotBeNull();
            gameInDatabase.ActivePlayers.Should().HaveCount(1);
            gameInDatabase.ActivePlayers.Should().ContainSingle(player => player.Id != SingleSpace);
        });
    }

    [Fact]
    public async Task Can_delete_resource_with_space_ID()
    {
        // Arrange
        Player existingPlayer = _fakers.Player.GenerateOne();
        existingPlayer.Id = SingleSpace;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Player>();
            dbContext.Players.Add(existingPlayer);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/players/%20";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Player? playerInDatabase = await dbContext.Players.FirstWithIdOrDefaultAsync((string?)existingPlayer.Id);

            playerInDatabase.Should().BeNull();
        });
    }

    private sealed class PreserveWhitespaceModelMetadataProvider : ModelMetadataProvider
    {
        private readonly ModelMetadataProvider _innerProvider;

        public PreserveWhitespaceModelMetadataProvider(ModelMetadataProvider innerProvider)
        {
            ArgumentNullException.ThrowIfNull(innerProvider);

            _innerProvider = innerProvider;
        }

        public override ModelMetadata GetMetadataForType(Type modelType)
        {
            var metadata = (DefaultModelMetadata)_innerProvider.GetMetadataForType(modelType);

            TurnOffConvertEmptyStringToNull(metadata);

            return metadata;
        }

        public override IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType)
        {
            return _innerProvider.GetMetadataForProperties(modelType);
        }

        public override ModelMetadata GetMetadataForParameter(ParameterInfo parameter)
        {
            var metadata = (DefaultModelMetadata)_innerProvider.GetMetadataForParameter(parameter);

            TurnOffConvertEmptyStringToNull(metadata);

            return metadata;
        }

        public override ModelMetadata GetMetadataForParameter(ParameterInfo parameter, Type modelType)
        {
            return _innerProvider.GetMetadataForParameter(parameter, modelType);
        }

        public override ModelMetadata GetMetadataForProperty(PropertyInfo propertyInfo, Type modelType)
        {
            return _innerProvider.GetMetadataForProperty(propertyInfo, modelType);
        }

        public override ModelMetadata GetMetadataForConstructor(ConstructorInfo constructor, Type modelType)
        {
            return _innerProvider.GetMetadataForConstructor(constructor, modelType);
        }

        private static void TurnOffConvertEmptyStringToNull(DefaultModelMetadata metadata)
        {
            // https://github.com/dotnet/aspnetcore/issues/29948#issuecomment-2058747809
            metadata.DisplayMetadata.ConvertEmptyStringToNull = false;
        }
    }
}
