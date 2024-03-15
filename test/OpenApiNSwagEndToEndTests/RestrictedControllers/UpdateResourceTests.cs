using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenApiNSwagEndToEndTests.RestrictedControllers.GeneratedCode;
using OpenApiTests;
using OpenApiTests.RestrictedControllers;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.RestrictedControllers;

public sealed class UpdateResourceTests : IClassFixture<IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly RestrictionFakers _fakers = new();

    public UpdateResourceTests(IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<WriteOnlyChannelsController>();

        testContext.ConfigureServices(services => services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>)));
    }

    [Fact]
    public async Task Can_update_resource_with_includes_and_fieldsets()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.Generate();
        existingChannel.VideoStream = _fakers.DataStream.Generate();
        existingChannel.UltraHighDefinitionVideoStream = _fakers.DataStream.Generate();
        existingChannel.AudioStreams = _fakers.DataStream.Generate(2).ToHashSet();

        DataStream existingVideoStream = _fakers.DataStream.Generate();
        string? newChannelName = _fakers.WriteOnlyChannel.Generate().Name;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            dbContext.DataStreams.Add(existingVideoStream);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        var requestBody = new WriteOnlyChannelPatchRequestDocument
        {
            Data = new WriteOnlyChannelDataInPatchRequest
            {
                Id = existingChannel.StringId!,
                Attributes = new WriteOnlyChannelAttributesInPatchRequest
                {
                    Name = newChannelName
                },
                Relationships = new WriteOnlyChannelRelationshipsInPatchRequest
                {
                    VideoStream = new ToOneDataStreamInRequest
                    {
                        Data = new DataStreamIdentifier
                        {
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

        // Act
        WriteOnlyChannelPrimaryResponseDocument? response =
            await ApiResponse.TranslateAsync(() => apiClient.PatchWriteOnlyChannelAsync(existingChannel.StringId!, queryString, requestBody));

        response.ShouldNotBeNull();

        response.Data.Id.Should().Be(existingChannel.StringId);
        response.Data.Attributes.Name.Should().Be(newChannelName);
        response.Data.Attributes.IsCommercial.Should().Be(existingChannel.IsCommercial);
        response.Data.Attributes.IsAdultOnly.Should().BeNull();
        response.Data.Relationships.VideoStream.Data.Id.Should().Be(existingVideoStream.StringId);
        response.Data.Relationships.UltraHighDefinitionVideoStream.Should().BeNull();
        response.Data.Relationships.AudioStreams.Data.Should().BeEmpty();

        response.Included.ShouldHaveCount(1);

        DataStreamDataInResponse? videoStream = response.Included.ElementAt(0).Should().BeOfType<DataStreamDataInResponse>().Which;
        videoStream.Id.Should().Be(existingVideoStream.StringId);

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

    [Fact]
    public async Task Can_update_resource_without_attributes_or_relationships()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.Generate();
        existingChannel.VideoStream = _fakers.DataStream.Generate();
        existingChannel.UltraHighDefinitionVideoStream = _fakers.DataStream.Generate();
        existingChannel.AudioStreams = _fakers.DataStream.Generate(2).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        var requestBody = new WriteOnlyChannelPatchRequestDocument
        {
            Data = new WriteOnlyChannelDataInPatchRequest
            {
                Id = existingChannel.StringId!,
                Attributes = new WriteOnlyChannelAttributesInPatchRequest(),
                Relationships = new WriteOnlyChannelRelationshipsInPatchRequest()
            }
        };

        // Act
        WriteOnlyChannelPrimaryResponseDocument? response =
            await ApiResponse.TranslateAsync(() => apiClient.PatchWriteOnlyChannelAsync(existingChannel.StringId!, null, requestBody));

        response.ShouldNotBeNull();

        response.Data.Id.Should().Be(existingChannel.StringId);
        response.Data.Attributes.Name.Should().Be(existingChannel.Name);
        response.Data.Attributes.IsCommercial.Should().Be(existingChannel.IsCommercial);
        response.Data.Attributes.IsAdultOnly.Should().Be(existingChannel.IsAdultOnly);
        response.Data.Relationships.VideoStream.Data.Should().BeNull();
        response.Data.Relationships.UltraHighDefinitionVideoStream.Data.Should().BeNull();
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
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.Generate();
        existingChannel.VideoStream = _fakers.DataStream.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        WriteOnlyChannelPatchRequestDocument requestBody = null!;

        // Act
        Func<Task> action = async () => _ = await apiClient.PatchWriteOnlyChannelAsync(existingChannel.StringId!, null, requestBody);

        // Assert
        await action.Should().ThrowExactlyAsync<ArgumentNullException>().WithParameterName("body");
    }

    [Fact]
    public async Task Cannot_update_resource_with_unknown_relationship_IDs()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.Generate();
        existingChannel.VideoStream = _fakers.DataStream.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        string unknownDataStreamId = Unknown.StringId.For<DataStream, long>();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new RestrictedControllersClient(httpClient);

        var requestBody = new WriteOnlyChannelPatchRequestDocument
        {
            Data = new WriteOnlyChannelDataInPatchRequest
            {
                Id = existingChannel.StringId!,
                Relationships = new WriteOnlyChannelRelationshipsInPatchRequest
                {
                    VideoStream = new ToOneDataStreamInRequest
                    {
                        Data = new DataStreamIdentifier
                        {
                            Id = unknownDataStreamId
                        }
                    },
                    AudioStreams = new ToManyDataStreamInRequest
                    {
                        Data =
                        [
                            new DataStreamIdentifier
                            {
                                Id = unknownDataStreamId
                            }
                        ]
                    }
                }
            }
        };

        // Act
        Func<Task> action = async () => _ = await apiClient.PatchWriteOnlyChannelAsync(existingChannel.StringId!, null, requestBody);

        // Assert
        ApiException<ErrorResponseDocument> exception = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which;
        exception.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be("HTTP 404: The writeOnlyChannel or a related resource does not exist.");
        exception.Result.Errors.ShouldHaveCount(2);

        ErrorObject error1 = exception.Result.Errors.ElementAt(0);
        error1.Status.Should().Be("404");
        error1.Title.Should().Be("A related resource does not exist.");
        error1.Detail.Should().Be($"Related resource of type 'dataStreams' with ID '{unknownDataStreamId}' in relationship 'videoStream' does not exist.");

        ErrorObject error2 = exception.Result.Errors.ElementAt(1);
        error2.Status.Should().Be("404");
        error2.Title.Should().Be("A related resource does not exist.");
        error2.Detail.Should().Be($"Related resource of type 'dataStreams' with ID '{unknownDataStreamId}' in relationship 'audioStreams' does not exist.");
    }
}
