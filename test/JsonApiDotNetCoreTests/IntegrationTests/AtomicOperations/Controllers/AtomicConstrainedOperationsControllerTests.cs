using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Controllers;

public sealed class AtomicConstrainedOperationsControllerTests
    : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicConstrainedOperationsControllerTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CreateMusicTrackOperationsController>();
    }

    [Fact]
    public async Task Can_create_resources_for_matching_resource_type()
    {
        // Arrange
        string newTitle1 = _fakers.MusicTrack.Generate().Title;
        string newTitle2 = _fakers.MusicTrack.Generate().Title;

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "musicTracks",
                        attributes = new
                        {
                            title = newTitle1
                        }
                    }
                },
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "musicTracks",
                        attributes = new
                        {
                            title = newTitle2
                        }
                    }
                }
            }
        };

        const string route = "/operations/musicTracks/create";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.ShouldHaveCount(2);
    }

    [Fact]
    public async Task Cannot_create_resource_for_mismatching_resource_type()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "performers",
                        attributes = new
                        {
                        }
                    }
                }
            }
        };

        const string route = "/operations/musicTracks/create";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Unsupported combination of operation code and resource type at this endpoint.");
        error.Detail.Should().Be("This endpoint can only be used to create resources of type 'musicTracks'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
    }

    [Fact]
    public async Task Cannot_update_resources_for_matching_resource_type()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.MusicTracks.Add(existingTrack);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "musicTracks",
                        id = existingTrack.StringId,
                        attributes = new
                        {
                        }
                    }
                }
            }
        };

        const string route = "/operations/musicTracks/create";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Unsupported combination of operation code and resource type at this endpoint.");
        error.Detail.Should().Be("This endpoint can only be used to create resources of type 'musicTracks'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
    }

    [Fact]
    public async Task Cannot_add_to_ToMany_relationship_for_matching_resource_type()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();
        Performer existingPerformer = _fakers.Performer.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingTrack, existingPerformer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    @ref = new
                    {
                        type = "musicTracks",
                        id = existingTrack.StringId,
                        relationship = "performers"
                    },
                    data = new[]
                    {
                        new
                        {
                            type = "performers",
                            id = existingPerformer.StringId
                        }
                    }
                }
            }
        };

        const string route = "/operations/musicTracks/create";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Unsupported combination of operation code and resource type at this endpoint.");
        error.Detail.Should().Be("This endpoint can only be used to create resources of type 'musicTracks'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
    }
}
