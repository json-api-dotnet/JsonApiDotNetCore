using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode;
using OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.RestrictedControllers;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.RestrictedControllers;

public sealed class UpdateRelationshipTests : IClassFixture<IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly RestrictionFakers _fakers = new();

    public UpdateRelationshipTests(IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<WriteOnlyChannelsController>();
    }

    [Fact]
    public async Task Can_replace_ToOne_relationship()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.GenerateOne();
        existingChannel.VideoStream = _fakers.DataStream.GenerateOne();
        existingChannel.UltraHighDefinitionVideoStream = _fakers.DataStream.GenerateOne();

        DataStream existingVideoStream = _fakers.DataStream.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            dbContext.DataStreams.Add(existingVideoStream);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        var requestBody = new NullableToOneDataStreamInRequest
        {
            Data = new DataStreamIdentifierInRequest
            {
                Type = ResourceType.DataStreams,
                Id = existingVideoStream.StringId!
            }
        };

        // Act
        await apiClient.WriteOnlyChannels[existingChannel.StringId!].Relationships.UltraHighDefinitionVideoStream.PatchAsync(requestBody);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            WriteOnlyChannel channelInDatabase = await dbContext.WriteOnlyChannels
                .Include(channel => channel.UltraHighDefinitionVideoStream)
                .FirstWithIdAsync(existingChannel.Id);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            channelInDatabase.UltraHighDefinitionVideoStream.Should().NotBeNull();
            channelInDatabase.UltraHighDefinitionVideoStream.Id.Should().Be(existingVideoStream.Id);
        });
    }

    [Fact]
    public async Task Can_clear_ToOne_relationship()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.GenerateOne();
        existingChannel.VideoStream = _fakers.DataStream.GenerateOne();
        existingChannel.UltraHighDefinitionVideoStream = _fakers.DataStream.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        var requestBody = new NullableToOneDataStreamInRequest
        {
            Data = null
        };

        // Act
        await apiClient.WriteOnlyChannels[existingChannel.StringId!].Relationships.UltraHighDefinitionVideoStream.PatchAsync(requestBody);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            WriteOnlyChannel channelInDatabase = await dbContext.WriteOnlyChannels
                .Include(channel => channel.UltraHighDefinitionVideoStream)
                .FirstWithIdAsync(existingChannel.Id);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            channelInDatabase.UltraHighDefinitionVideoStream.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_replace_ToMany_relationship()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.GenerateOne();
        existingChannel.VideoStream = _fakers.DataStream.GenerateOne();
        existingChannel.AudioStreams = _fakers.DataStream.GenerateSet(2);

        DataStream existingAudioStream = _fakers.DataStream.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            dbContext.DataStreams.Add(existingAudioStream);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        var requestBody = new ToManyDataStreamInRequest
        {
            Data =
            [
                new DataStreamIdentifierInRequest
                {
                    Type = ResourceType.DataStreams,
                    Id = existingAudioStream.StringId!
                }
            ]
        };

        // Act
        await apiClient.WriteOnlyChannels[existingChannel.StringId!].Relationships.AudioStreams.PatchAsync(requestBody);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            WriteOnlyChannel channelInDatabase = await dbContext.WriteOnlyChannels
                .Include(channel => channel.AudioStreams)
                .FirstWithIdAsync(existingChannel.Id);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            channelInDatabase.AudioStreams.Should().HaveCount(1);
            channelInDatabase.AudioStreams.ElementAt(0).Id.Should().Be(existingAudioStream.Id);
        });
    }

    [Fact]
    public async Task Can_clear_ToMany_relationship()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.GenerateOne();
        existingChannel.VideoStream = _fakers.DataStream.GenerateOne();
        existingChannel.AudioStreams = _fakers.DataStream.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        var requestBody = new ToManyDataStreamInRequest
        {
            Data = []
        };

        // Act
        await apiClient.WriteOnlyChannels[existingChannel.StringId!].Relationships.AudioStreams.PatchAsync(requestBody);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            WriteOnlyChannel channelInDatabase = await dbContext.WriteOnlyChannels
                .Include(channel => channel.AudioStreams)
                .FirstWithIdAsync(existingChannel.Id);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            channelInDatabase.AudioStreams.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_add_to_ToMany_relationship()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.GenerateOne();
        existingChannel.VideoStream = _fakers.DataStream.GenerateOne();
        existingChannel.AudioStreams = _fakers.DataStream.GenerateSet(1);

        DataStream existingAudioStream = _fakers.DataStream.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            dbContext.DataStreams.Add(existingAudioStream);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        var requestBody = new ToManyDataStreamInRequest
        {
            Data =
            [
                new DataStreamIdentifierInRequest
                {
                    Type = ResourceType.DataStreams,
                    Id = existingAudioStream.StringId!
                }
            ]
        };

        // Act
        await apiClient.WriteOnlyChannels[existingChannel.StringId!].Relationships.AudioStreams.PostAsync(requestBody);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            WriteOnlyChannel channelInDatabase = await dbContext.WriteOnlyChannels
                .Include(channel => channel.AudioStreams)
                .FirstWithIdAsync(existingChannel.Id);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            channelInDatabase.AudioStreams.Should().HaveCount(2);
            channelInDatabase.AudioStreams.Should().ContainSingle(stream => stream.Id == existingChannel.AudioStreams.ElementAt(0).Id);
            channelInDatabase.AudioStreams.Should().ContainSingle(stream => stream.Id == existingAudioStream.Id);
        });
    }

    [Fact]
    public async Task Can_remove_from_ToMany_relationship()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.GenerateOne();
        existingChannel.VideoStream = _fakers.DataStream.GenerateOne();
        existingChannel.AudioStreams = _fakers.DataStream.GenerateSet(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        var requestBody = new ToManyDataStreamInRequest
        {
            Data =
            [
                new DataStreamIdentifierInRequest
                {
                    Type = ResourceType.DataStreams,
                    Id = existingChannel.AudioStreams.ElementAt(0).StringId!
                },
                new DataStreamIdentifierInRequest
                {
                    Type = ResourceType.DataStreams,
                    Id = existingChannel.AudioStreams.ElementAt(1).StringId!
                }
            ]
        };

        // Act
        await apiClient.WriteOnlyChannels[existingChannel.StringId!].Relationships.AudioStreams.DeleteAsync(requestBody);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            WriteOnlyChannel channelInDatabase = await dbContext.WriteOnlyChannels
                .Include(channel => channel.AudioStreams)
                .FirstWithIdAsync(existingChannel.Id);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            channelInDatabase.AudioStreams.Should().HaveCount(1);
            channelInDatabase.AudioStreams.ElementAt(0).Id.Should().Be(existingChannel.AudioStreams.ElementAt(2).Id);
        });
    }

    [Fact]
    public async Task Cannot_update_relationship_for_missing_request_body()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.GenerateOne();
        existingChannel.VideoStream = _fakers.DataStream.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        NullableToOneDataStreamInRequest requestBody = null!;

        // Act
        Func<Task> action = async () =>
            await apiClient.WriteOnlyChannels[existingChannel.StringId!].Relationships.UltraHighDefinitionVideoStream.PatchAsync(requestBody);

        // Assert
        await action.Should().ThrowExactlyAsync<ArgumentNullException>().WithParameterName("body");
    }

    [Fact]
    public async Task Cannot_update_relationship_with_unknown_relationship_IDs()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.GenerateOne();
        existingChannel.VideoStream = _fakers.DataStream.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        string unknownAudioStreamId1 = Unknown.StringId.For<DataStream, long>();
        string unknownAudioStreamId2 = Unknown.StringId.AltFor<DataStream, long>();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        var requestBody = new ToManyDataStreamInRequest
        {
            Data =
            [
                new DataStreamIdentifierInRequest
                {
                    Type = ResourceType.DataStreams,
                    Id = unknownAudioStreamId1
                },
                new DataStreamIdentifierInRequest
                {
                    Type = ResourceType.DataStreams,
                    Id = unknownAudioStreamId2
                }
            ]
        };

        // Act
        Func<Task> action = async () => await apiClient.WriteOnlyChannels[existingChannel.StringId!].Relationships.AudioStreams.PatchAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(2);

        ErrorObject error1 = exception.Errors.ElementAt(0);
        error1.Status.Should().Be("404");
        error1.Title.Should().Be("A related resource does not exist.");
        error1.Detail.Should().Be($"Related resource of type 'dataStreams' with ID '{unknownAudioStreamId1}' in relationship 'audioStreams' does not exist.");

        ErrorObject error2 = exception.Errors.ElementAt(1);
        error2.Status.Should().Be("404");
        error2.Title.Should().Be("A related resource does not exist.");
        error2.Detail.Should().Be($"Related resource of type 'dataStreams' with ID '{unknownAudioStreamId2}' in relationship 'audioStreams' does not exist.");
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
