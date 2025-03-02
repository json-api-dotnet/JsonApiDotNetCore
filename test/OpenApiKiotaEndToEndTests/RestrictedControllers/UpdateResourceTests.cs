using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode;
using OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.RestrictedControllers;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.RestrictedControllers;

public sealed class UpdateResourceTests : IClassFixture<IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly RestrictionFakers _fakers = new();

    public UpdateResourceTests(IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<WriteOnlyChannelsController>();

        testContext.ConfigureServices(services => services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>)));
    }

    [Fact]
    public async Task Can_update_resource_with_includes_and_fieldsets()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.GenerateOne();
        existingChannel.VideoStream = _fakers.DataStream.GenerateOne();
        existingChannel.UltraHighDefinitionVideoStream = _fakers.DataStream.GenerateOne();
        existingChannel.AudioStreams = _fakers.DataStream.GenerateSet(2);

        DataStream existingVideoStream = _fakers.DataStream.GenerateOne();
        string? newChannelName = _fakers.WriteOnlyChannel.GenerateOne().Name;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            dbContext.DataStreams.Add(existingVideoStream);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        var requestBody = new UpdateWriteOnlyChannelRequestDocument
        {
            Data = new DataInUpdateWriteOnlyChannelRequest
            {
                Type = WriteOnlyChannelResourceType.WriteOnlyChannels,
                Id = existingChannel.StringId!,
                Attributes = new AttributesInUpdateWriteOnlyChannelRequest
                {
                    Name = newChannelName
                },
                Relationships = new RelationshipsInUpdateWriteOnlyChannelRequest
                {
                    VideoStream = new ToOneDataStreamInRequest
                    {
                        Data = new DataStreamIdentifierInRequest
                        {
                            Type = DataStreamResourceType.DataStreams,
                            Id = existingVideoStream.StringId!
                        }
                    },
                    UltraHighDefinitionVideoStream = new NullableToOneDataStreamInRequest
                    {
                        Data = null
                    },
                    AudioStreams = new ToManyDataStreamInRequest
                    {
                        Data = []
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
            WriteOnlyChannelPrimaryResponseDocument? response = await apiClient.WriteOnlyChannels[existingChannel.StringId!].PatchAsync(requestBody);

            response.ShouldNotBeNull();

            response.Data.ShouldNotBeNull();
            response.Data.Id.Should().Be(existingChannel.StringId);
            response.Data.Attributes.ShouldNotBeNull();
            response.Data.Attributes.Name.Should().Be(newChannelName);
            response.Data.Attributes.IsCommercial.Should().Be(existingChannel.IsCommercial);
            response.Data.Attributes.IsAdultOnly.Should().BeNull();
            response.Data.Relationships.ShouldNotBeNull();
            response.Data.Relationships.VideoStream.ShouldNotBeNull();
            response.Data.Relationships.VideoStream.Data.ShouldNotBeNull();
            response.Data.Relationships.VideoStream.Data.Id.Should().Be(existingVideoStream.StringId);
            response.Data.Relationships.UltraHighDefinitionVideoStream.Should().BeNull();
            response.Data.Relationships.AudioStreams.ShouldNotBeNull();
            response.Data.Relationships.AudioStreams.Data.Should().BeEmpty();

            response.Included.Should().HaveCount(1);
            response.Included.OfType<DataStreamDataInResponse>().Should().ContainSingle(streamData => streamData.Id == existingVideoStream.StringId);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                // @formatter:wrap_chained_method_calls chop_always
                // @formatter:keep_existing_linebreaks true

                WriteOnlyChannel channelInDatabase = await dbContext.WriteOnlyChannels
                    .Include(channel => channel.VideoStream)
                    .Include(channel => channel.UltraHighDefinitionVideoStream)
                    .Include(channel => channel.AudioStreams)
                    .FirstWithIdAsync(existingChannel.Id);

                // @formatter:keep_existing_linebreaks restore
                // @formatter:wrap_chained_method_calls restore

                channelInDatabase.Name.Should().Be(newChannelName);
                channelInDatabase.IsCommercial.Should().Be(existingChannel.IsCommercial);
                channelInDatabase.IsAdultOnly.Should().Be(existingChannel.IsAdultOnly);

                channelInDatabase.VideoStream.ShouldNotBeNull();
                channelInDatabase.VideoStream.Id.Should().Be(existingVideoStream.Id);

                channelInDatabase.UltraHighDefinitionVideoStream.Should().BeNull();

                channelInDatabase.AudioStreams.Should().BeEmpty();
            });
        }
    }

    [Fact]
    public async Task Can_update_resource_without_attributes_or_relationships()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.GenerateOne();
        existingChannel.VideoStream = _fakers.DataStream.GenerateOne();
        existingChannel.UltraHighDefinitionVideoStream = _fakers.DataStream.GenerateOne();
        existingChannel.AudioStreams = _fakers.DataStream.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        var requestBody = new UpdateWriteOnlyChannelRequestDocument
        {
            Data = new DataInUpdateWriteOnlyChannelRequest
            {
                Type = WriteOnlyChannelResourceType.WriteOnlyChannels,
                Id = existingChannel.StringId!,
                Attributes = new AttributesInUpdateWriteOnlyChannelRequest(),
                Relationships = new RelationshipsInUpdateWriteOnlyChannelRequest()
            }
        };

        // Act
        WriteOnlyChannelPrimaryResponseDocument? response = await apiClient.WriteOnlyChannels[existingChannel.StringId!].PatchAsync(requestBody);

        response.ShouldNotBeNull();

        response.Data.ShouldNotBeNull();
        response.Data.Id.Should().Be(existingChannel.StringId);
        response.Data.Attributes.ShouldNotBeNull();
        response.Data.Attributes.Name.Should().Be(existingChannel.Name);
        response.Data.Attributes.IsCommercial.Should().Be(existingChannel.IsCommercial);
        response.Data.Attributes.IsAdultOnly.Should().Be(existingChannel.IsAdultOnly);
        response.Data.Relationships.ShouldNotBeNull();
        response.Data.Relationships.VideoStream.ShouldNotBeNull();
        response.Data.Relationships.VideoStream.Data.Should().BeNull();
        response.Data.Relationships.UltraHighDefinitionVideoStream.ShouldNotBeNull();
        response.Data.Relationships.UltraHighDefinitionVideoStream.Data.Should().BeNull();
        response.Data.Relationships.AudioStreams.ShouldNotBeNull();
        response.Data.Relationships.AudioStreams.Data.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            WriteOnlyChannel channelInDatabase = await dbContext.WriteOnlyChannels
                .Include(channel => channel.VideoStream)
                .Include(channel => channel.UltraHighDefinitionVideoStream)
                .Include(channel => channel.AudioStreams)
                .FirstWithIdAsync(existingChannel.Id);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            channelInDatabase.Name.Should().Be(existingChannel.Name);
            channelInDatabase.IsCommercial.Should().Be(existingChannel.IsCommercial);
            channelInDatabase.IsAdultOnly.Should().Be(existingChannel.IsAdultOnly);

            channelInDatabase.VideoStream.ShouldNotBeNull();
            channelInDatabase.VideoStream.Id.Should().Be(existingChannel.VideoStream.Id);

            channelInDatabase.UltraHighDefinitionVideoStream.ShouldNotBeNull();
            channelInDatabase.UltraHighDefinitionVideoStream.Id.Should().Be(existingChannel.UltraHighDefinitionVideoStream.Id);

            channelInDatabase.AudioStreams.Should().HaveCount(2);
        });
    }

    [Fact]
    public async Task Cannot_update_resource_for_missing_request_body()
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

        UpdateWriteOnlyChannelRequestDocument requestBody = null!;

        // Act
        Func<Task> action = async () => _ = await apiClient.WriteOnlyChannels[existingChannel.StringId!].PatchAsync(requestBody);

        // Assert
        await action.Should().ThrowExactlyAsync<ArgumentNullException>().WithParameterName("body");
    }

    [Fact]
    public async Task Cannot_update_resource_with_unknown_relationship_IDs()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.GenerateOne();
        existingChannel.VideoStream = _fakers.DataStream.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        string unknownDataStreamId = Unknown.StringId.For<DataStream, long>();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        var requestBody = new UpdateWriteOnlyChannelRequestDocument
        {
            Data = new DataInUpdateWriteOnlyChannelRequest
            {
                Type = WriteOnlyChannelResourceType.WriteOnlyChannels,
                Id = existingChannel.StringId!,
                Relationships = new RelationshipsInUpdateWriteOnlyChannelRequest
                {
                    VideoStream = new ToOneDataStreamInRequest
                    {
                        Data = new DataStreamIdentifierInRequest
                        {
                            Type = DataStreamResourceType.DataStreams,
                            Id = unknownDataStreamId
                        }
                    },
                    AudioStreams = new ToManyDataStreamInRequest
                    {
                        Data =
                        [
                            new DataStreamIdentifierInRequest
                            {
                                Type = DataStreamResourceType.DataStreams,
                                Id = unknownDataStreamId
                            }
                        ]
                    }
                }
            }
        };

        // Act
        Func<Task> action = async () => _ = await apiClient.WriteOnlyChannels[existingChannel.StringId!].PatchAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(2);

        ErrorObject error1 = exception.Errors.ElementAt(0);
        error1.Status.Should().Be("404");
        error1.Title.Should().Be("A related resource does not exist.");
        error1.Detail.Should().Be($"Related resource of type 'dataStreams' with ID '{unknownDataStreamId}' in relationship 'audioStreams' does not exist.");

        ErrorObject error2 = exception.Errors.ElementAt(1);
        error2.Status.Should().Be("404");
        error2.Title.Should().Be("A related resource does not exist.");
        error2.Detail.Should().Be($"Related resource of type 'dataStreams' with ID '{unknownDataStreamId}' in relationship 'videoStream' does not exist.");
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
