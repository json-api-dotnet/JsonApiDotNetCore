using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Microsoft.EntityFrameworkCore;
using OpenApiNSwagEndToEndTests.RestrictedControllers.GeneratedCode;
using OpenApiTests;
using OpenApiTests.RestrictedControllers;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.RestrictedControllers;

public sealed class UpdateRelationshipTests : IClassFixture<IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly RestrictionFakers _fakers = new();

    public UpdateRelationshipTests(IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<WriteOnlyChannelsController>();
    }

    [Fact]
    public async Task Can_replace_ToOne_relationship()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.Generate();
        existingChannel.VideoStream = _fakers.DataStream.Generate();
        existingChannel.UltraHighDefinitionVideoStream = _fakers.DataStream.Generate();

        DataStream existingVideoStream = _fakers.DataStream.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            dbContext.DataStreams.Add(existingVideoStream);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        var requestBody = new NullableToOneDataStreamInRequest
        {
            Data = new DataStreamIdentifierInRequest
            {
                Id = existingVideoStream.StringId!
            }
        };

        // Act
        await ApiResponse.TranslateAsync(async () =>
            await apiClient.PatchWriteOnlyChannelUltraHighDefinitionVideoStreamRelationshipAsync(existingChannel.StringId!, requestBody));

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            WriteOnlyChannel channelInDatabase = await dbContext.WriteOnlyChannels
                .Include(channel => channel.UltraHighDefinitionVideoStream)
                .FirstWithIdAsync(existingChannel.Id);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            channelInDatabase.UltraHighDefinitionVideoStream.ShouldNotBeNull();
            channelInDatabase.UltraHighDefinitionVideoStream.Id.Should().Be(existingVideoStream.Id);
        });
    }

    [Fact]
    public async Task Can_clear_ToOne_relationship()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.Generate();
        existingChannel.VideoStream = _fakers.DataStream.Generate();
        existingChannel.UltraHighDefinitionVideoStream = _fakers.DataStream.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        var requestBody = new NullableToOneDataStreamInRequest
        {
            Data = null
        };

        // Act
        await ApiResponse.TranslateAsync(async () =>
            await apiClient.PatchWriteOnlyChannelUltraHighDefinitionVideoStreamRelationshipAsync(existingChannel.StringId!, requestBody));

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
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.Generate();
        existingChannel.VideoStream = _fakers.DataStream.Generate();
        existingChannel.AudioStreams = _fakers.DataStream.Generate(2).ToHashSet();

        DataStream existingAudioStream = _fakers.DataStream.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            dbContext.DataStreams.Add(existingAudioStream);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        var requestBody = new ToManyDataStreamInRequest
        {
            Data =
            [
                new DataStreamIdentifierInRequest
                {
                    Id = existingAudioStream.StringId!
                }
            ]
        };

        // Act
        await ApiResponse.TranslateAsync(async () =>
            await apiClient.PatchWriteOnlyChannelAudioStreamsRelationshipAsync(existingChannel.StringId!, requestBody));

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
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.Generate();
        existingChannel.VideoStream = _fakers.DataStream.Generate();
        existingChannel.AudioStreams = _fakers.DataStream.Generate(2).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        var requestBody = new ToManyDataStreamInRequest
        {
            Data = []
        };

        // Act
        await ApiResponse.TranslateAsync(async () =>
            await apiClient.PatchWriteOnlyChannelAudioStreamsRelationshipAsync(existingChannel.StringId!, requestBody));

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
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.Generate();
        existingChannel.VideoStream = _fakers.DataStream.Generate();
        existingChannel.AudioStreams = _fakers.DataStream.Generate(1).ToHashSet();

        DataStream existingAudioStream = _fakers.DataStream.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            dbContext.DataStreams.Add(existingAudioStream);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        var requestBody = new ToManyDataStreamInRequest
        {
            Data =
            [
                new DataStreamIdentifierInRequest
                {
                    Id = existingAudioStream.StringId!
                }
            ]
        };

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostWriteOnlyChannelAudioStreamsRelationshipAsync(existingChannel.StringId!, requestBody));

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
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.Generate();
        existingChannel.VideoStream = _fakers.DataStream.Generate();
        existingChannel.AudioStreams = _fakers.DataStream.Generate(3).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        var requestBody = new ToManyDataStreamInRequest
        {
            Data =
            [
                new DataStreamIdentifierInRequest
                {
                    Id = existingChannel.AudioStreams.ElementAt(0).StringId!
                },
                new DataStreamIdentifierInRequest
                {
                    Id = existingChannel.AudioStreams.ElementAt(1).StringId!
                }
            ]
        };

        // Act
        await ApiResponse.TranslateAsync(async () =>
            await apiClient.DeleteWriteOnlyChannelAudioStreamsRelationshipAsync(existingChannel.StringId!, requestBody));

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
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.Generate();
        existingChannel.VideoStream = _fakers.DataStream.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        NullableToOneDataStreamInRequest requestBody = null!;

        // Act
        Func<Task> action = async () =>
            await apiClient.PatchWriteOnlyChannelUltraHighDefinitionVideoStreamRelationshipAsync(existingChannel.StringId!, requestBody);

        // Assert
        await action.Should().ThrowExactlyAsync<ArgumentNullException>().WithParameterName("body");
    }

    [Fact]
    public async Task Cannot_update_relationship_with_unknown_relationship_IDs()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.Generate();
        existingChannel.VideoStream = _fakers.DataStream.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        string unknownAudioStreamId1 = Unknown.StringId.For<DataStream, long>();
        string unknownAudioStreamId2 = Unknown.StringId.AltFor<DataStream, long>();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        var requestBody = new ToManyDataStreamInRequest
        {
            Data =
            [
                new DataStreamIdentifierInRequest
                {
                    Id = unknownAudioStreamId1
                },
                new DataStreamIdentifierInRequest
                {
                    Id = unknownAudioStreamId2
                }
            ]
        };

        // Act
        Func<Task> action = async () => await apiClient.PatchWriteOnlyChannelAudioStreamsRelationshipAsync(existingChannel.StringId!, requestBody);

        // Assert
        ApiException<ErrorResponseDocument> exception = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which;
        exception.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be("HTTP 404: The writeOnlyChannel or a related resource does not exist.");
        exception.Result.Errors.ShouldHaveCount(2);

        ErrorObject error1 = exception.Result.Errors.ElementAt(0);
        error1.Status.Should().Be("404");
        error1.Title.Should().Be("A related resource does not exist.");
        error1.Detail.Should().Be($"Related resource of type 'dataStreams' with ID '{unknownAudioStreamId1}' in relationship 'audioStreams' does not exist.");

        ErrorObject error2 = exception.Result.Errors.ElementAt(1);
        error2.Status.Should().Be("404");
        error2.Title.Should().Be("A related resource does not exist.");
        error2.Detail.Should().Be($"Related resource of type 'dataStreams' with ID '{unknownAudioStreamId2}' in relationship 'audioStreams' does not exist.");
    }
}
