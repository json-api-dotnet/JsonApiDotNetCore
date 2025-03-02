using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using OpenApiNSwagEndToEndTests.RestrictedControllers.GeneratedCode;
using OpenApiTests;
using OpenApiTests.RestrictedControllers;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.RestrictedControllers;

public sealed class FetchRelationshipTests : IClassFixture<IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly RestrictionFakers _fakers = new();

    public FetchRelationshipTests(IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<ReadOnlyChannelsController>();
    }

    [Fact]
    public async Task Can_get_ToOne_relationship()
    {
        // Arrange
        ReadOnlyChannel channel = _fakers.ReadOnlyChannel.GenerateOne();
        channel.VideoStream = _fakers.DataStream.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ReadOnlyChannels.Add(channel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        // Act
        DataStreamIdentifierResponseDocument response = await apiClient.GetReadOnlyChannelVideoStreamRelationshipAsync(channel.StringId!);

        // Assert
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(channel.VideoStream.StringId);
    }

    [Fact]
    public async Task Can_get_empty_ToOne_relationship()
    {
        // Arrange
        ReadOnlyChannel channel = _fakers.ReadOnlyChannel.GenerateOne();
        channel.VideoStream = _fakers.DataStream.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ReadOnlyChannels.Add(channel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        // Act
        NullableDataStreamIdentifierResponseDocument response =
            await apiClient.GetReadOnlyChannelUltraHighDefinitionVideoStreamRelationshipAsync(channel.StringId!);

        // Assert
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_ToMany_relationship()
    {
        // Arrange
        ReadOnlyChannel channel = _fakers.ReadOnlyChannel.GenerateOne();
        channel.VideoStream = _fakers.DataStream.GenerateOne();
        channel.AudioStreams = _fakers.DataStream.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ReadOnlyChannels.Add(channel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        // Act
        DataStreamIdentifierCollectionResponseDocument response = await apiClient.GetReadOnlyChannelAudioStreamsRelationshipAsync(channel.StringId!);

        // Assert
        response.Data.Should().HaveCount(2);
        response.Data.Should().ContainSingle(autoStream => autoStream.Id == channel.AudioStreams.ElementAt(0).StringId);
        response.Data.Should().ContainSingle(autoStream => autoStream.Id == channel.AudioStreams.ElementAt(1).StringId);
    }

    [Fact]
    public async Task Can_get_empty_ToMany_relationship()
    {
        // Arrange
        ReadOnlyChannel channel = _fakers.ReadOnlyChannel.GenerateOne();
        channel.VideoStream = _fakers.DataStream.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ReadOnlyChannels.Add(channel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        // Act
        DataStreamIdentifierCollectionResponseDocument response = await apiClient.GetReadOnlyChannelAudioStreamsRelationshipAsync(channel.StringId!);

        // Assert
        response.Data.Should().HaveCount(0);
    }

    [Fact]
    public async Task Cannot_get_relationship_for_unknown_primary_ID()
    {
        // Arrange
        string unknownChannelId = Unknown.StringId.For<ReadOnlyChannel, long>();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        // Act
        Func<Task> action = async () => _ = await apiClient.GetReadOnlyChannelVideoStreamRelationshipAsync(unknownChannelId);

        // Assert
        ApiException<ErrorResponseDocument> exception = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which;
        exception.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be("HTTP 404: The readOnlyChannel does not exist.");
        exception.Result.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Result.Errors.ElementAt(0);
        error.Status.Should().Be("404");
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'readOnlyChannels' with ID '{unknownChannelId}' does not exist.");
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
