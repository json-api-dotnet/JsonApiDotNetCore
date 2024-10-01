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

public sealed class CreateResourceTests : IClassFixture<IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly RestrictionFakers _fakers = new();

    public CreateResourceTests(IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<WriteOnlyChannelsController>();
    }

    [Fact]
    public async Task Can_create_resource_with_includes_and_fieldsets()
    {
        // Arrange
        DataStream existingVideoStream = _fakers.DataStream.GenerateOne();
        DataStream existingAudioStream = _fakers.DataStream.GenerateOne();
        WriteOnlyChannel newChannel = _fakers.WriteOnlyChannel.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DataStreams.AddRange(existingVideoStream, existingAudioStream);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        var requestBody = new CreateWriteOnlyChannelRequestDocument
        {
            Data = new DataInCreateWriteOnlyChannelRequest
            {
                Attributes = new AttributesInCreateWriteOnlyChannelRequest
                {
                    Name = newChannel.Name,
                    IsAdultOnly = newChannel.IsAdultOnly
                },
                Relationships = new RelationshipsInCreateWriteOnlyChannelRequest
                {
                    VideoStream = new ToOneDataStreamInRequest
                    {
                        Data = new DataStreamIdentifierInRequest
                        {
                            Id = existingVideoStream.StringId!
                        }
                    },
                    AudioStreams = new ToManyDataStreamInRequest
                    {
                        Data =
                        [
                            new DataStreamIdentifierInRequest
                            {
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

        // Act
        WriteOnlyChannelPrimaryResponseDocument? response =
            await ApiResponse.TranslateAsync(async () => await apiClient.PostWriteOnlyChannelAsync(requestBody, queryString));

        response.ShouldNotBeNull();

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

    [Fact]
    public async Task Cannot_create_resource_for_missing_request_body()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        CreateWriteOnlyChannelRequestDocument requestBody = null!;

        // Act
        Func<Task> action = async () => _ = await apiClient.PostWriteOnlyChannelAsync(requestBody);

        // Assert
        await action.Should().ThrowExactlyAsync<ArgumentNullException>().WithParameterName("body");
    }

    [Fact]
    public async Task Cannot_create_resource_with_unknown_relationship_ID()
    {
        // Arrange
        WriteOnlyChannel newChannel = _fakers.WriteOnlyChannel.GenerateOne();

        string unknownVideoStreamId = Unknown.StringId.For<DataStream, long>();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        var requestBody = new CreateWriteOnlyChannelRequestDocument
        {
            Data = new DataInCreateWriteOnlyChannelRequest
            {
                Attributes = new AttributesInCreateWriteOnlyChannelRequest
                {
                    Name = newChannel.Name
                },
                Relationships = new RelationshipsInCreateWriteOnlyChannelRequest
                {
                    VideoStream = new ToOneDataStreamInRequest
                    {
                        Data = new DataStreamIdentifierInRequest
                        {
                            Id = unknownVideoStreamId
                        }
                    }
                }
            }
        };

        // Act
        Func<Task> action = async () => _ = await apiClient.PostWriteOnlyChannelAsync(requestBody);

        // Assert
        ApiException<ErrorResponseDocument> exception = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which;
        exception.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be("HTTP 404: A related resource does not exist.");
        exception.Result.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Result.Errors.ElementAt(0);
        error.Status.Should().Be("404");
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'dataStreams' with ID '{unknownVideoStreamId}' in relationship 'videoStream' does not exist.");
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
