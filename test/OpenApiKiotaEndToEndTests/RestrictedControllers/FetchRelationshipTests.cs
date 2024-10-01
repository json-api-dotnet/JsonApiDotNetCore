using System.Net;
using FluentAssertions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode;
using OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.RestrictedControllers;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.RestrictedControllers;

public sealed class FetchRelationshipTests : IClassFixture<IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly RestrictionFakers _fakers = new();

    public FetchRelationshipTests(IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        // Act
        DataStreamIdentifierResponseDocument? response = await apiClient.ReadOnlyChannels[channel.StringId].Relationships.VideoStream.GetAsync();

        // Assert
        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull();
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        // Act
        NullableDataStreamIdentifierResponseDocument? response =
            await apiClient.ReadOnlyChannels[channel.StringId].Relationships.UltraHighDefinitionVideoStream.GetAsync();

        // Assert
        response.ShouldNotBeNull();
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        // Act
        DataStreamIdentifierCollectionResponseDocument? response = await apiClient.ReadOnlyChannels[channel.StringId].Relationships.AudioStreams.GetAsync();

        // Assert
        response.ShouldNotBeNull();
        response.Data.ShouldHaveCount(2);
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        // Act
        DataStreamIdentifierCollectionResponseDocument? response = await apiClient.ReadOnlyChannels[channel.StringId].Relationships.AudioStreams.GetAsync();

        // Assert
        response.ShouldNotBeNull();
        response.Data.ShouldHaveCount(0);
    }

    [Fact]
    public async Task Cannot_get_relationship_for_unknown_primary_ID()
    {
        // Arrange
        string unknownChannelId = Unknown.StringId.For<ReadOnlyChannel, long>();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        // Act
        Func<Task> action = async () => _ = await apiClient.ReadOnlyChannels[unknownChannelId].Relationships.VideoStream.GetAsync();

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Errors.ElementAt(0);
        error.Status.Should().Be("404");
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'readOnlyChannels' with ID '{unknownChannelId}' does not exist.");
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
