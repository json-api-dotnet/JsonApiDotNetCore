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

public sealed class CreateResourceTests : IClassFixture<IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly RestrictionFakers _fakers = new();

    public CreateResourceTests(IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<WriteOnlyChannelsController>();
    }

    [Fact]
    public async Task Can_create_resource_with_includes_and_fieldsets()
    {
        // Arrange
        DataStream existingVideoStream = _fakers.DataStream.Generate();
        DataStream existingAudioStream = _fakers.DataStream.Generate();
        WriteOnlyChannel newChannel = _fakers.WriteOnlyChannel.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DataStreams.AddRange(existingVideoStream, existingAudioStream);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        var requestBody = new WriteOnlyChannelPostRequestDocument
        {
            Data = new WriteOnlyChannelDataInPostRequest
            {
                Type = WriteOnlyChannelResourceType.WriteOnlyChannels,
                Attributes = new WriteOnlyChannelAttributesInPostRequest
                {
                    Name = newChannel.Name,
                    IsAdultOnly = newChannel.IsAdultOnly
                },
                Relationships = new WriteOnlyChannelRelationshipsInPostRequest
                {
                    VideoStream = new ToOneDataStreamInRequest
                    {
                        Data = new DataStreamIdentifier
                        {
                            Type = DataStreamResourceType.DataStreams,
                            Id = existingVideoStream.StringId!
                        }
                    },
                    AudioStreams = new ToManyDataStreamInRequest
                    {
                        Data =
                        [
                            new DataStreamIdentifier
                            {
                                Type = DataStreamResourceType.DataStreams,
                                Id = existingAudioStream.StringId!
                            }
                        ]
                    }
                }
            }
        };

        var queryString = new Dictionary<string, string?>
        {
            ["include"] = "videoStream,audioStreams",
            ["fields[writeOnlyChannels]"] = "name,isCommercial,videoStream,audioStreams",
            ["fields[dataStreams]"] = "bytesTransmitted"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            WriteOnlyChannelPrimaryResponseDocument? response = await apiClient.WriteOnlyChannels.PostAsync(requestBody);

            response.ShouldNotBeNull();

            response.Data.ShouldNotBeNull();
            response.Data.Attributes.ShouldNotBeNull();
            response.Data.Attributes.Name.Should().Be(newChannel.Name);
            response.Data.Attributes.IsCommercial.Should().BeNull();
            response.Data.Attributes.IsAdultOnly.Should().BeNull();
            response.Data.Relationships.ShouldNotBeNull();
            response.Data.Relationships.VideoStream.ShouldNotBeNull();
            response.Data.Relationships.VideoStream.Data.ShouldNotBeNull();
            response.Data.Relationships.VideoStream.Data.Id.Should().Be(existingVideoStream.StringId);
            response.Data.Relationships.UltraHighDefinitionVideoStream.Should().BeNull();
            response.Data.Relationships.AudioStreams.ShouldNotBeNull();
            response.Data.Relationships.AudioStreams.Data.ShouldHaveCount(1);
            response.Data.Relationships.AudioStreams.Data.ElementAt(0).Id.Should().Be(existingAudioStream.StringId);

            response.Included.ShouldHaveCount(2);
            DataStreamDataInResponse[] dataStreamIncludes = response.Included.OfType<DataStreamDataInResponse>().ToArray();

            DataStreamDataInResponse videoStream = dataStreamIncludes.Single(include => include.Id == existingVideoStream.StringId);
            videoStream.Attributes.ShouldNotBeNull();
            videoStream.Attributes.BytesTransmitted.Should().Be((long?)existingVideoStream.BytesTransmitted);

            DataStreamDataInResponse audioStream = dataStreamIncludes.Single(include => include.Id == existingAudioStream.StringId);
            audioStream.Attributes.ShouldNotBeNull();
            audioStream.Attributes.BytesTransmitted.Should().Be((long?)existingAudioStream.BytesTransmitted);

            long newChannelId = int.Parse(response.Data.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                // @formatter:wrap_chained_method_calls chop_always
                // @formatter:keep_existing_linebreaks true

                WriteOnlyChannel channelInDatabase = await dbContext.WriteOnlyChannels
                    .Include(channel => channel.VideoStream)
                    .Include(channel => channel.AudioStreams)
                    .FirstWithIdAsync(newChannelId);

                // @formatter:keep_existing_linebreaks restore
                // @formatter:wrap_chained_method_calls restore

                channelInDatabase.Name.Should().Be(newChannel.Name);
                channelInDatabase.IsCommercial.Should().BeNull();
                channelInDatabase.IsAdultOnly.Should().Be(newChannel.IsAdultOnly);

                channelInDatabase.VideoStream.ShouldNotBeNull();
                channelInDatabase.VideoStream.Id.Should().Be(existingVideoStream.Id);

                channelInDatabase.AudioStreams.ShouldHaveCount(1);
                channelInDatabase.AudioStreams.ElementAt(0).Id.Should().Be(existingAudioStream.Id);
            });
        }
    }

    [Fact]
    public async Task Cannot_create_resource_for_missing_request_body()
    {
        // Arrange
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        WriteOnlyChannelPostRequestDocument requestBody = null!;

        // Act
        Func<Task> action = async () => _ = await apiClient.WriteOnlyChannels.PostAsync(requestBody);

        // Assert
        await action.Should().ThrowExactlyAsync<ArgumentNullException>().WithParameterName("body");
    }

    [Fact]
    public async Task Cannot_create_resource_with_unknown_relationship_ID()
    {
        // Arrange
        WriteOnlyChannel newChannel = _fakers.WriteOnlyChannel.Generate();

        string unknownVideoStreamId = Unknown.StringId.For<DataStream, long>();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        var requestBody = new WriteOnlyChannelPostRequestDocument
        {
            Data = new WriteOnlyChannelDataInPostRequest
            {
                Type = WriteOnlyChannelResourceType.WriteOnlyChannels,
                Attributes = new WriteOnlyChannelAttributesInPostRequest
                {
                    Name = newChannel.Name
                },
                Relationships = new WriteOnlyChannelRelationshipsInPostRequest
                {
                    VideoStream = new ToOneDataStreamInRequest
                    {
                        Data = new DataStreamIdentifier
                        {
                            Type = DataStreamResourceType.DataStreams,
                            Id = unknownVideoStreamId
                        }
                    }
                }
            }
        };

        // Act
        Func<Task> action = async () => _ = await apiClient.WriteOnlyChannels.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Errors.ElementAt(0);
        error.Status.Should().Be("404");
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'dataStreams' with ID '{unknownVideoStreamId}' in relationship 'videoStream' does not exist.");
    }
}
