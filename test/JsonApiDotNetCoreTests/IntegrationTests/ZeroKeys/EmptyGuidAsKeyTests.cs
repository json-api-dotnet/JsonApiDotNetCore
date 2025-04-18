using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys;

public sealed class EmptyGuidAsKeyTests : IClassFixture<IntegrationTestContext<TestableStartup<ZeroKeyDbContext>, ZeroKeyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ZeroKeyDbContext>, ZeroKeyDbContext> _testContext;
    private readonly ZeroKeyFakers _fakers = new();

    public EmptyGuidAsKeyTests(IntegrationTestContext<TestableStartup<ZeroKeyDbContext>, ZeroKeyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PlayersController>();
        testContext.UseController<GamesController>();
        testContext.UseController<MapsController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.UseRelativeLinks = true;
    }

    [Fact]
    public async Task Can_filter_by_empty_ID_on_primary_resources()
    {
        // Arrange
        List<Map> maps = _fakers.Map.GenerateList(2);
        maps[0].Id = Guid.Empty;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Map>();
            dbContext.Maps.AddRange(maps);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/maps?filter=equals(id,'00000000-0000-0000-0000-000000000000')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be("00000000-0000-0000-0000-000000000000");

        responseDocument.Data.ManyValue[0].With(resource =>
        {
            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be("/maps/00000000-0000-0000-0000-000000000000");
        });
    }

    [Fact]
    public async Task Can_get_primary_resource_by_empty_ID_with_include()
    {
        // Arrange
        Map map = _fakers.Map.GenerateOne();
        map.Id = Guid.Empty;
        map.Game = _fakers.Game.GenerateOne();
        map.Game.Id = 0;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTablesAsync<Map, Game>();
            dbContext.Maps.Add(map);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/maps/00000000-0000-0000-0000-000000000000?include=game";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be("00000000-0000-0000-0000-000000000000");
        responseDocument.Data.SingleValue.Links.Should().NotBeNull();
        responseDocument.Data.SingleValue.Links.Self.Should().Be("/maps/00000000-0000-0000-0000-000000000000");

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Id.Should().Be("0");
    }

    [Fact]
    public async Task Can_create_resource_with_empty_ID()
    {
        // Arrange
        string newName = _fakers.Map.GenerateOne().Name;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Map>();
        });

        var requestBody = new
        {
            data = new
            {
                type = "maps",
                id = "00000000-0000-0000-0000-000000000000",
                attributes = new
                {
                    name = newName
                }
            }
        };

        const string route = "/maps";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        httpResponse.Headers.Location.Should().Be("/maps/00000000-0000-0000-0000-000000000000");

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Map mapInDatabase = await dbContext.Maps.FirstWithIdAsync((Guid?)Guid.Empty);

            mapInDatabase.Should().NotBeNull();
            mapInDatabase.Name.Should().Be(newName);
        });
    }

    [Fact]
    public async Task Can_update_resource_with_empty_ID()
    {
        // Arrange
        Map existingMap = _fakers.Map.GenerateOne();
        existingMap.Id = Guid.Empty;

        string newName = _fakers.Map.GenerateOne().Name;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Map>();
            dbContext.Maps.Add(existingMap);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "maps",
                id = "00000000-0000-0000-0000-000000000000",
                attributes = new
                {
                    name = newName
                }
            }
        };

        const string route = "/maps/00000000-0000-0000-0000-000000000000";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Map mapInDatabase = await dbContext.Maps.FirstWithIdAsync((Guid?)Guid.Empty);

            mapInDatabase.Should().NotBeNull();
            mapInDatabase.Name.Should().Be(newName);
        });
    }

    [Fact]
    public async Task Can_clear_ToOne_relationship_with_empty_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();
        existingGame.ActiveMap = _fakers.Map.GenerateOne();
        existingGame.ActiveMap.Id = Guid.Empty;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Map>();
            dbContext.Games.Add(existingGame);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/games/{existingGame.StringId}/relationships/activeMap";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.ActiveMap).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.Should().NotBeNull();
            gameInDatabase.ActiveMap.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_assign_ToOne_relationship_with_empty_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();

        Map existingMap = _fakers.Map.GenerateOne();
        existingMap.Id = Guid.Empty;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Map>();
            dbContext.AddInRange(existingGame, existingMap);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "maps",
                id = "00000000-0000-0000-0000-000000000000"
            }
        };

        string route = $"/games/{existingGame.StringId}/relationships/activeMap";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.ActiveMap).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.Should().NotBeNull();
            gameInDatabase.ActiveMap.Should().NotBeNull();
            gameInDatabase.ActiveMap.Id.Should().Be(Guid.Empty);
        });
    }

    [Fact]
    public async Task Can_replace_ToOne_relationship_with_empty_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();
        existingGame.ActiveMap = _fakers.Map.GenerateOne();

        Map existingMap = _fakers.Map.GenerateOne();
        existingMap.Id = Guid.Empty;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Map>();
            dbContext.AddInRange(existingGame, existingMap);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "maps",
                id = "00000000-0000-0000-0000-000000000000"
            }
        };

        string route = $"/games/{existingGame.StringId}/relationships/activeMap";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.ActiveMap).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.Should().NotBeNull();
            gameInDatabase.ActiveMap.Should().NotBeNull();
            gameInDatabase.ActiveMap.Id.Should().Be(Guid.Empty);
        });
    }

    [Fact]
    public async Task Can_clear_ToMany_relationship_with_empty_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();
        existingGame.Maps = _fakers.Map.GenerateList(2);
        existingGame.Maps.ElementAt(0).Id = Guid.Empty;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Map>();
            dbContext.Games.Add(existingGame);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string route = $"/games/{existingGame.StringId}/relationships/maps";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.Maps).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.Should().NotBeNull();
            gameInDatabase.Maps.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_assign_ToMany_relationship_with_empty_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();

        Map existingMap = _fakers.Map.GenerateOne();
        existingMap.Id = Guid.Empty;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Map>();
            dbContext.AddInRange(existingGame, existingMap);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "maps",
                    id = "00000000-0000-0000-0000-000000000000"
                }
            }
        };

        string route = $"/games/{existingGame.StringId}/relationships/maps";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.Maps).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.Should().NotBeNull();
            gameInDatabase.Maps.Should().HaveCount(1);
            gameInDatabase.Maps.ElementAt(0).Id.Should().Be(Guid.Empty);
        });
    }

    [Fact]
    public async Task Can_replace_ToMany_relationship_with_empty_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();
        existingGame.Maps = _fakers.Map.GenerateList(2);

        Map existingMap = _fakers.Map.GenerateOne();
        existingMap.Id = Guid.Empty;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Map>();
            dbContext.AddInRange(existingGame, existingMap);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "maps",
                    id = "00000000-0000-0000-0000-000000000000"
                }
            }
        };

        string route = $"/games/{existingGame.StringId}/relationships/maps";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.Maps).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.Should().NotBeNull();
            gameInDatabase.Maps.Should().HaveCount(1);
            gameInDatabase.Maps.ElementAt(0).Id.Should().Be(Guid.Empty);
        });
    }

    [Fact]
    public async Task Can_add_to_ToMany_relationship_with_empty_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();
        existingGame.Maps = _fakers.Map.GenerateList(1);

        Map existingMap = _fakers.Map.GenerateOne();
        existingMap.Id = Guid.Empty;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Map>();
            dbContext.AddInRange(existingGame, existingMap);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "maps",
                    id = "00000000-0000-0000-0000-000000000000"
                }
            }
        };

        string route = $"/games/{existingGame.StringId}/relationships/maps";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.Maps).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.Should().NotBeNull();
            gameInDatabase.Maps.Should().HaveCount(2);
            gameInDatabase.Maps.Should().ContainSingle(map => map.Id == Guid.Empty);
        });
    }

    [Fact]
    public async Task Can_remove_from_ToMany_relationship_with_empty_ID()
    {
        // Arrange
        Game existingGame = _fakers.Game.GenerateOne();
        existingGame.Maps = _fakers.Map.GenerateList(2);
        existingGame.Maps.ElementAt(0).Id = Guid.Empty;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Map>();
            dbContext.Games.Add(existingGame);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "maps",
                    id = "00000000-0000-0000-0000-000000000000"
                }
            }
        };

        string route = $"/games/{existingGame.StringId}/relationships/maps";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.Include(game => game.Maps).FirstWithIdAsync(existingGame.Id);

            gameInDatabase.Should().NotBeNull();
            gameInDatabase.Maps.Should().HaveCount(1);
            gameInDatabase.Maps.Should().ContainSingle(map => map.Id != Guid.Empty);
        });
    }

    [Fact]
    public async Task Can_delete_resource_with_empty_ID()
    {
        // Arrange
        Map existingMap = _fakers.Map.GenerateOne();
        existingMap.Id = Guid.Empty;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Map>();
            dbContext.Maps.Add(existingMap);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/maps/00000000-0000-0000-0000-000000000000";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Map? mapInDatabase = await dbContext.Maps.FirstWithIdOrDefaultAsync(existingMap.Id);

            mapInDatabase.Should().BeNull();
        });
    }
}
