using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.QueryStrings;

public sealed class AtomicQueryStringTests : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicQueryStringTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();
        testContext.UseController<MusicTracksController>();

        testContext.ConfigureServicesAfterStartup(services =>
        {
            services.AddSingleton<ISystemClock, FrozenSystemClock>();
            services.AddResourceDefinition<MusicTrackReleaseDefinition>();
        });
    }

    [Fact]
    public async Task Cannot_include_at_operations_endpoint()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "recordCompanies",
                        attributes = new
                        {
                        }
                    }
                }
            }
        };

        const string route = "/operations?include=recordCompanies";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
        error.Detail.Should().Be("The parameter 'include' cannot be used at this endpoint.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("include");
    }

    [Fact]
    public async Task Cannot_filter_at_operations_endpoint()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "recordCompanies",
                        attributes = new
                        {
                        }
                    }
                }
            }
        };

        const string route = "/operations?filter=equals(id,'1')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
        error.Detail.Should().Be("The parameter 'filter' cannot be used at this endpoint.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    [Fact]
    public async Task Cannot_sort_at_operations_endpoint()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "recordCompanies",
                        attributes = new
                        {
                        }
                    }
                }
            }
        };

        const string route = "/operations?sort=-id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
        error.Detail.Should().Be("The parameter 'sort' cannot be used at this endpoint.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("sort");
    }

    [Fact]
    public async Task Cannot_use_pagination_number_at_operations_endpoint()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "recordCompanies",
                        attributes = new
                        {
                        }
                    }
                }
            }
        };

        const string route = "/operations?page[number]=1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
        error.Detail.Should().Be("The parameter 'page[number]' cannot be used at this endpoint.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("page[number]");
    }

    [Fact]
    public async Task Cannot_use_pagination_size_at_operations_endpoint()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "recordCompanies",
                        attributes = new
                        {
                        }
                    }
                }
            }
        };

        const string route = "/operations?page[size]=1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
        error.Detail.Should().Be("The parameter 'page[size]' cannot be used at this endpoint.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("page[size]");
    }

    [Fact]
    public async Task Cannot_use_sparse_fieldset_at_operations_endpoint()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "recordCompanies",
                        attributes = new
                        {
                        }
                    }
                }
            }
        };

        const string route = "/operations?fields[recordCompanies]=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
        error.Detail.Should().Be("The parameter 'fields[recordCompanies]' cannot be used at this endpoint.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("fields[recordCompanies]");
    }

    [Fact]
    public async Task Can_use_Queryable_handler_at_resource_endpoint()
    {
        // Arrange
        var clock = _testContext.Factory.Services.GetRequiredService<ISystemClock>();

        List<MusicTrack> musicTracks = _fakers.MusicTrack.Generate(3);
        musicTracks[0].ReleasedAt = clock.UtcNow.AddMonths(5);
        musicTracks[1].ReleasedAt = clock.UtcNow.AddMonths(-5);
        musicTracks[2].ReleasedAt = clock.UtcNow.AddMonths(-1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<MusicTrack>();
            dbContext.MusicTracks.AddRange(musicTracks);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/musicTracks?isRecentlyReleased=true";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(musicTracks[2].StringId);
    }

    [Fact]
    public async Task Cannot_use_Queryable_handler_at_operations_endpoint()
    {
        // Arrange
        string newTrackTitle = _fakers.MusicTrack.Generate().Title;

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "musicTracks",
                        attributes = new
                        {
                            title = newTrackTitle
                        }
                    }
                }
            }
        };

        const string route = "/operations?isRecentlyReleased=true";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Unknown query string parameter.");

        error.Detail.Should().Be("Query string parameter 'isRecentlyReleased' is unknown. " +
            "Set 'AllowUnknownQueryStringParameters' to 'true' in options to ignore unknown parameters.");

        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("isRecentlyReleased");
    }
}
