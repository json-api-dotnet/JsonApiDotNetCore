using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using OpenApiTests;
using OpenApiTests.RestrictedControllers;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiNSwagEndToEndTests.RestrictedControllers;

public sealed class MediaTypeTests : IClassFixture<IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
    private readonly RestrictionFakers _fakers = new();

    public MediaTypeTests(IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WriteOnlyChannelsController>();
    }

    [Fact]
    public async Task Can_create_resource_with_default_media_type()
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

        var requestBody = new
        {
            data = new
            {
                type = "writeOnlyChannels",
                attributes = new
                {
                    name = newChannel.Name,
                    isAdultOnly = newChannel.IsAdultOnly
                },
                relationships = new
                {
                    videoStream = new
                    {
                        data = new
                        {
                            type = "dataStreams",
                            id = existingVideoStream.StringId
                        }
                    },
                    audioStreams = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "dataStreams",
                                id = existingAudioStream.StringId
                            }
                        }
                    }
                }
            }
        };

        const string route = "/writeOnlyChannels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        httpResponse.Content.Headers.ContentType.Should().NotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().NotBeEmpty();
        responseDocument.Data.SingleValue.Relationships.Should().NotBeEmpty();

        long newChannelId = long.Parse(responseDocument.Data.SingleValue.Id.Should().NotBeNull().And.Subject);

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

            channelInDatabase.VideoStream.Should().NotBeNull();
            channelInDatabase.VideoStream.Id.Should().Be(existingVideoStream.Id);

            channelInDatabase.AudioStreams.Should().HaveCount(1);
            channelInDatabase.AudioStreams.ElementAt(0).Id.Should().Be(existingAudioStream.Id);
        });
    }
}
