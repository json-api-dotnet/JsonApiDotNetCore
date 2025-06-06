using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving;

public sealed class ArchiveTests : IClassFixture<IntegrationTestContext<TestableStartup<TelevisionDbContext>, TelevisionDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<TelevisionDbContext>, TelevisionDbContext> _testContext;
    private readonly TelevisionFakers _fakers = new();

    public ArchiveTests(IntegrationTestContext<TestableStartup<TelevisionDbContext>, TelevisionDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<TelevisionNetworksController>();
        testContext.UseController<TelevisionStationsController>();
        testContext.UseController<TelevisionBroadcastsController>();
        testContext.UseController<BroadcastCommentsController>();

        testContext.ConfigureServices(services => services.AddResourceDefinition<TelevisionBroadcastDefinition>());
    }

    [Fact]
    public async Task Can_get_archived_resource_by_ID()
    {
        // Arrange
        TelevisionBroadcast broadcast = _fakers.TelevisionBroadcast.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Broadcasts.Add(broadcast);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/televisionBroadcasts/{broadcast.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(broadcast.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().Be(broadcast.ArchivedAt);
    }

    [Fact]
    public async Task Can_get_unarchived_resource_by_ID()
    {
        // Arrange
        TelevisionBroadcast broadcast = _fakers.TelevisionBroadcast.GenerateOne();
        broadcast.ArchivedAt = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Broadcasts.Add(broadcast);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/televisionBroadcasts/{broadcast.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(broadcast.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().BeNull();
    }

    [Fact]
    public async Task Get_primary_resources_excludes_archived()
    {
        // Arrange
        List<TelevisionBroadcast> broadcasts = _fakers.TelevisionBroadcast.GenerateList(2);
        broadcasts[1].ArchivedAt = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<TelevisionBroadcast>();
            dbContext.Broadcasts.AddRange(broadcasts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/televisionBroadcasts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(broadcasts[1].StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().BeNull();
    }

    [Fact]
    public async Task Get_primary_resources_with_filter_includes_archived()
    {
        // Arrange
        List<TelevisionBroadcast> broadcasts = _fakers.TelevisionBroadcast.GenerateList(2);
        broadcasts[1].ArchivedAt = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<TelevisionBroadcast>();
            dbContext.Broadcasts.AddRange(broadcasts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/televisionBroadcasts?filter=or(equals(archivedAt,null),not(equals(archivedAt,null)))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(broadcasts[0].StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().Be(broadcasts[0].ArchivedAt);

        responseDocument.Data.ManyValue[1].Id.Should().Be(broadcasts[1].StringId);
        responseDocument.Data.ManyValue[1].Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().BeNull();
    }

    [Fact]
    public async Task Get_primary_resource_by_ID_with_include_excludes_archived()
    {
        // Arrange
        TelevisionStation station = _fakers.TelevisionStation.GenerateOne();
        station.Broadcasts = _fakers.TelevisionBroadcast.GenerateSet(2);
        station.Broadcasts.ElementAt(1).ArchivedAt = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stations.Add(station);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/televisionStations/{station.StringId}?include=broadcasts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(station.StringId);

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Id.Should().Be(station.Broadcasts.ElementAt(1).StringId);
        responseDocument.Included[0].Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().BeNull();
    }

    [Fact]
    public async Task Get_primary_resource_by_ID_with_include_and_filter_includes_archived()
    {
        // Arrange
        TelevisionStation station = _fakers.TelevisionStation.GenerateOne();
        station.Broadcasts = _fakers.TelevisionBroadcast.GenerateSet(2);
        station.Broadcasts.ElementAt(1).ArchivedAt = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stations.Add(station);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/televisionStations/{station.StringId}?include=broadcasts&filter[broadcasts]=or(equals(archivedAt,null),not(equals(archivedAt,null)))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(station.StringId);

        responseDocument.Included.Should().HaveCount(2);
        responseDocument.Included[0].Id.Should().Be(station.Broadcasts.ElementAt(0).StringId);
        responseDocument.Included[0].Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().Be(station.Broadcasts.ElementAt(0).ArchivedAt);
        responseDocument.Included[1].Id.Should().Be(station.Broadcasts.ElementAt(1).StringId);
        responseDocument.Included[1].Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().BeNull();
    }

    [Fact]
    public async Task Get_secondary_resource_includes_archived()
    {
        // Arrange
        BroadcastComment comment = _fakers.BroadcastComment.GenerateOne();
        comment.AppliesTo = _fakers.TelevisionBroadcast.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Comments.Add(comment);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/broadcastComments/{comment.StringId}/appliesTo";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(comment.AppliesTo.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().Be(comment.AppliesTo.ArchivedAt);
    }

    [Fact]
    public async Task Get_secondary_resources_excludes_archived()
    {
        // Arrange
        TelevisionStation station = _fakers.TelevisionStation.GenerateOne();
        station.Broadcasts = _fakers.TelevisionBroadcast.GenerateSet(2);
        station.Broadcasts.ElementAt(1).ArchivedAt = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stations.Add(station);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/televisionStations/{station.StringId}/broadcasts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(station.Broadcasts.ElementAt(1).StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().BeNull();
    }

    [Fact]
    public async Task Get_secondary_resources_with_filter_includes_archived()
    {
        // Arrange
        TelevisionStation station = _fakers.TelevisionStation.GenerateOne();
        station.Broadcasts = _fakers.TelevisionBroadcast.GenerateSet(2);
        station.Broadcasts.ElementAt(1).ArchivedAt = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stations.Add(station);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/televisionStations/{station.StringId}/broadcasts?filter=or(equals(archivedAt,null),not(equals(archivedAt,null)))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        DateTimeOffset archivedAt0 = station.Broadcasts.ElementAt(0).ArchivedAt!.Value;

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(station.Broadcasts.ElementAt(0).StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().Be(archivedAt0);

        responseDocument.Data.ManyValue[1].Id.Should().Be(station.Broadcasts.ElementAt(1).StringId);
        responseDocument.Data.ManyValue[1].Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().BeNull();
    }

    [Fact]
    public async Task Get_secondary_resource_by_ID_with_include_excludes_archived()
    {
        // Arrange
        TelevisionNetwork network = _fakers.TelevisionNetwork.GenerateOne();
        network.Stations = _fakers.TelevisionStation.GenerateSet(1);
        network.Stations.ElementAt(0).Broadcasts = _fakers.TelevisionBroadcast.GenerateSet(2);
        network.Stations.ElementAt(0).Broadcasts.ElementAt(1).ArchivedAt = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Networks.Add(network);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/televisionNetworks/{network.StringId}/stations?include=broadcasts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(network.Stations.ElementAt(0).StringId);

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Id.Should().Be(network.Stations.ElementAt(0).Broadcasts.ElementAt(1).StringId);
        responseDocument.Included[0].Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().BeNull();
    }

    [Fact]
    public async Task Get_secondary_resource_by_ID_with_include_and_filter_includes_archived()
    {
        TelevisionNetwork network = _fakers.TelevisionNetwork.GenerateOne();
        network.Stations = _fakers.TelevisionStation.GenerateSet(1);
        network.Stations.ElementAt(0).Broadcasts = _fakers.TelevisionBroadcast.GenerateSet(2);
        network.Stations.ElementAt(0).Broadcasts.ElementAt(1).ArchivedAt = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Networks.Add(network);
            await dbContext.SaveChangesAsync();
        });

        string route =
            $"/televisionNetworks/{network.StringId}/stations?include=broadcasts&filter[broadcasts]=or(equals(archivedAt,null),not(equals(archivedAt,null)))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        DateTimeOffset archivedAt0 = network.Stations.ElementAt(0).Broadcasts.ElementAt(0).ArchivedAt!.Value;

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(network.Stations.ElementAt(0).StringId);

        responseDocument.Included.Should().HaveCount(2);
        responseDocument.Included[0].Id.Should().Be(network.Stations.ElementAt(0).Broadcasts.ElementAt(0).StringId);
        responseDocument.Included[0].Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().Be(archivedAt0);
        responseDocument.Included[1].Id.Should().Be(network.Stations.ElementAt(0).Broadcasts.ElementAt(1).StringId);
        responseDocument.Included[1].Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().BeNull();
    }

    [Fact]
    public async Task Get_ToMany_relationship_excludes_archived()
    {
        // Arrange
        TelevisionStation station = _fakers.TelevisionStation.GenerateOne();
        station.Broadcasts = _fakers.TelevisionBroadcast.GenerateSet(2);
        station.Broadcasts.ElementAt(1).ArchivedAt = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stations.Add(station);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/televisionStations/{station.StringId}/relationships/broadcasts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(station.Broadcasts.ElementAt(1).StringId);
    }

    [Fact]
    public async Task Get_ToMany_relationship_with_filter_includes_archived()
    {
        // Arrange
        TelevisionStation station = _fakers.TelevisionStation.GenerateOne();
        station.Broadcasts = _fakers.TelevisionBroadcast.GenerateSet(2);
        station.Broadcasts.ElementAt(1).ArchivedAt = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stations.Add(station);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/televisionStations/{station.StringId}/relationships/broadcasts?filter=or(equals(archivedAt,null),not(equals(archivedAt,null)))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(station.Broadcasts.ElementAt(0).StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(station.Broadcasts.ElementAt(1).StringId);
    }

    [Fact]
    public async Task Can_create_unarchived_resource()
    {
        // Arrange
        TelevisionBroadcast newBroadcast = _fakers.TelevisionBroadcast.GenerateOne();

        var requestBody = new
        {
            data = new
            {
                type = "televisionBroadcasts",
                attributes = new
                {
                    title = newBroadcast.Title,
                    airedAt = newBroadcast.AiredAt
                }
            }
        };

        const string route = "/televisionBroadcasts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("title").WhoseValue.Should().Be(newBroadcast.Title);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("airedAt").WhoseValue.Should().Be(newBroadcast.AiredAt);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("archivedAt").WhoseValue.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_create_archived_resource()
    {
        // Arrange
        TelevisionBroadcast newBroadcast = _fakers.TelevisionBroadcast.GenerateOne();

        var requestBody = new
        {
            data = new
            {
                type = "televisionBroadcasts",
                attributes = new
                {
                    title = newBroadcast.Title,
                    airedAt = newBroadcast.AiredAt,
                    archivedAt = newBroadcast.ArchivedAt
                }
            }
        };

        const string route = "/televisionBroadcasts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("Television broadcasts cannot be created in archived state.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Can_archive_resource()
    {
        // Arrange
        TelevisionBroadcast existingBroadcast = _fakers.TelevisionBroadcast.GenerateOne();
        existingBroadcast.ArchivedAt = null;

        DateTimeOffset newArchivedAt = _fakers.TelevisionBroadcast.GenerateOne().ArchivedAt!.Value;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Broadcasts.Add(existingBroadcast);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "televisionBroadcasts",
                id = existingBroadcast.StringId,
                attributes = new
                {
                    archivedAt = newArchivedAt
                }
            }
        };

        string route = $"/televisionBroadcasts/{existingBroadcast.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TelevisionBroadcast broadcastInDatabase = await dbContext.Broadcasts.FirstWithIdAsync(existingBroadcast.Id);

            broadcastInDatabase.ArchivedAt.Should().Be(newArchivedAt);
        });
    }

    [Fact]
    public async Task Can_unarchive_resource()
    {
        // Arrange
        TelevisionBroadcast broadcast = _fakers.TelevisionBroadcast.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Broadcasts.Add(broadcast);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "televisionBroadcasts",
                id = broadcast.StringId,
                attributes = new
                {
                    archivedAt = (DateTimeOffset?)null
                }
            }
        };

        string route = $"/televisionBroadcasts/{broadcast.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TelevisionBroadcast broadcastInDatabase = await dbContext.Broadcasts.FirstWithIdAsync(broadcast.Id);

            broadcastInDatabase.ArchivedAt.Should().BeNull();
        });
    }

    [Fact]
    public async Task Cannot_shift_archive_date()
    {
        // Arrange
        TelevisionBroadcast broadcast = _fakers.TelevisionBroadcast.GenerateOne();

        DateTimeOffset? newArchivedAt = _fakers.TelevisionBroadcast.GenerateOne().ArchivedAt;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Broadcasts.Add(broadcast);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "televisionBroadcasts",
                id = broadcast.StringId,
                attributes = new
                {
                    archivedAt = newArchivedAt
                }
            }
        };

        string route = $"/televisionBroadcasts/{broadcast.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("Archive date of television broadcasts cannot be shifted. Unarchive it first.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Can_delete_archived_resource()
    {
        // Arrange
        TelevisionBroadcast broadcast = _fakers.TelevisionBroadcast.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Broadcasts.Add(broadcast);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/televisionBroadcasts/{broadcast.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TelevisionBroadcast? broadcastInDatabase = await dbContext.Broadcasts.FirstWithIdOrDefaultAsync(broadcast.Id);

            broadcastInDatabase.Should().BeNull();
        });
    }

    [Fact]
    public async Task Cannot_delete_unarchived_resource()
    {
        // Arrange
        TelevisionBroadcast broadcast = _fakers.TelevisionBroadcast.GenerateOne();
        broadcast.ArchivedAt = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Broadcasts.Add(broadcast);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/televisionBroadcasts/{broadcast.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("Television broadcasts must first be archived before they can be deleted.");
        error.Detail.Should().BeNull();
    }
}
