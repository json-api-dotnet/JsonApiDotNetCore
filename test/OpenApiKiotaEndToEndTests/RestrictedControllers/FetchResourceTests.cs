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

public sealed class FetchResourceTests : IClassFixture<IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly RestrictionFakers _fakers = new();

    public FetchResourceTests(IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<ReadOnlyChannelsController>();
    }

    [Fact]
    public async Task Can_get_primary_resources()
    {
        // Arrange
        List<ReadOnlyChannel> channels = _fakers.ReadOnlyChannel.GenerateList(2);
        channels.ForEach(channel => channel.VideoStream = _fakers.DataStream.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<ReadOnlyChannel>();
            dbContext.ReadOnlyChannels.AddRange(channels);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        // Act
        ReadOnlyChannelCollectionResponseDocument? response = await apiClient.ReadOnlyChannels.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        DataInReadOnlyChannelResponse channel1 = response.Data.Single(channel => channel.Id == channels.ElementAt(0).StringId);
        channel1.Attributes.Should().NotBeNull();
        channel1.Attributes.Name.Should().Be(channels[0].Name);
        channel1.Attributes.IsCommercial.Should().Be(channels[0].IsCommercial);
        channel1.Attributes.IsAdultOnly.Should().Be(channels[0].IsAdultOnly);
        channel1.Relationships.Should().NotBeNull();
        channel1.Relationships.VideoStream.Should().NotBeNull();
        channel1.Relationships.VideoStream.Data.Should().BeNull();
        channel1.Relationships.UltraHighDefinitionVideoStream.Should().NotBeNull();
        channel1.Relationships.UltraHighDefinitionVideoStream.Data.Should().BeNull();
        channel1.Relationships.AudioStreams.Should().NotBeNull();
        channel1.Relationships.AudioStreams.Data.Should().BeNull();

        DataInReadOnlyChannelResponse channel2 = response.Data.Single(channel => channel.Id == channels.ElementAt(1).StringId);
        channel2.Attributes.Should().NotBeNull();
        channel2.Attributes.Name.Should().Be(channels[1].Name);
        channel2.Attributes.IsCommercial.Should().Be(channels[1].IsCommercial);
        channel2.Attributes.IsAdultOnly.Should().Be(channels[1].IsAdultOnly);
        channel2.Relationships.Should().NotBeNull();
        channel2.Relationships.VideoStream.Should().NotBeNull();
        channel2.Relationships.VideoStream.Data.Should().BeNull();
        channel2.Relationships.UltraHighDefinitionVideoStream.Should().NotBeNull();
        channel2.Relationships.UltraHighDefinitionVideoStream.Data.Should().BeNull();
        channel2.Relationships.AudioStreams.Should().NotBeNull();
        channel2.Relationships.AudioStreams.Data.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_primary_resource_by_ID()
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
        PrimaryReadOnlyChannelResponseDocument? response = await apiClient.ReadOnlyChannels[channel.StringId!].GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(channel.StringId);
        response.Data.Attributes.Should().NotBeNull();
        response.Data.Attributes.Name.Should().Be(channel.Name);
        response.Data.Attributes.IsCommercial.Should().Be(channel.IsCommercial);
        response.Data.Attributes.IsAdultOnly.Should().Be(channel.IsAdultOnly);
        response.Data.Relationships.Should().NotBeNull();
        response.Data.Relationships.VideoStream.Should().NotBeNull();
        response.Data.Relationships.VideoStream.Data.Should().BeNull();
        response.Data.Relationships.UltraHighDefinitionVideoStream.Should().NotBeNull();
        response.Data.Relationships.UltraHighDefinitionVideoStream.Data.Should().BeNull();
        response.Data.Relationships.AudioStreams.Should().NotBeNull();
        response.Data.Relationships.AudioStreams.Data.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_get_primary_resource_for_unknown_ID()
    {
        // Arrange
        string unknownChannelId = Unknown.StringId.For<ReadOnlyChannel, long>();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        // Act
        Func<Task> action = async () => _ = await apiClient.ReadOnlyChannels[unknownChannelId].GetAsync();

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors.ElementAt(0);
        error.Status.Should().Be("404");
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'readOnlyChannels' with ID '{unknownChannelId}' does not exist.");
    }

    [Fact]
    public async Task Can_get_secondary_ToOne_resource()
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
        SecondaryDataStreamResponseDocument? response = await apiClient.ReadOnlyChannels[channel.StringId!].VideoStream.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(channel.VideoStream.StringId);
        response.Data.Attributes.Should().NotBeNull();
        response.Data.Attributes.BytesTransmitted.Should().Be((long?)channel.VideoStream.BytesTransmitted);
    }

    [Fact]
    public async Task Can_get_unknown_secondary_ToOne_resource()
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
        NullableSecondaryDataStreamResponseDocument? response = await apiClient.ReadOnlyChannels[channel.StringId!].UltraHighDefinitionVideoStream.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_secondary_ToMany_resources()
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
        DataStreamCollectionResponseDocument? response = await apiClient.ReadOnlyChannels[channel.StringId!].AudioStreams.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        DataInDataStreamResponse audioStream1 = response.Data.Single(autoStream => autoStream.Id == channel.AudioStreams.ElementAt(0).StringId);
        audioStream1.Attributes.Should().NotBeNull();
        audioStream1.Attributes.BytesTransmitted.Should().Be((long?)channel.AudioStreams.ElementAt(0).BytesTransmitted);

        DataInDataStreamResponse audioStream2 = response.Data.Single(autoStream => autoStream.Id == channel.AudioStreams.ElementAt(1).StringId);
        audioStream2.Attributes.Should().NotBeNull();
        audioStream2.Attributes.BytesTransmitted.Should().Be((long?)channel.AudioStreams.ElementAt(1).BytesTransmitted);
    }

    [Fact]
    public async Task Can_get_no_secondary_ToMany_resources()
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
        DataStreamCollectionResponseDocument? response = await apiClient.ReadOnlyChannels[channel.StringId!].AudioStreams.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(0);
    }

    [Fact]
    public async Task Cannot_get_secondary_resource_for_unknown_primary_ID()
    {
        // Arrange
        string unknownChannelId = Unknown.StringId.For<ReadOnlyChannel, long>();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        // Act
        Func<Task> action = async () => _ = await apiClient.ReadOnlyChannels[unknownChannelId].VideoStream.GetAsync();

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

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
