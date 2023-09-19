using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

public sealed class ScopeWriteTests : IClassFixture<IntegrationTestContext<ScopesStartup<ScopesDbContext>, ScopesDbContext>>
{
    private const string ScopeHeaderName = "X-Auth-Scopes";
    private readonly IntegrationTestContext<ScopesStartup<ScopesDbContext>, ScopesDbContext> _testContext;
    private readonly ScopesFakers _fakers = new();

    public ScopeWriteTests(IntegrationTestContext<ScopesStartup<ScopesDbContext>, ScopesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<MoviesController>();
    }

    [Fact]
    public async Task Cannot_create_resource_without_scopes()
    {
        // Arrange
        Movie newMovie = _fakers.Movie.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "movies",
                attributes = new
                {
                    title = newMovie.Title,
                    releaseYear = newMovie.ReleaseYear,
                    durationInSeconds = newMovie.DurationInSeconds
                }
            }
        };

        const string route = "/movies";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: write:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_create_resource_with_relationships_without_scopes()
    {
        // Arrange
        Movie newMovie = _fakers.Movie.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "movies",
                attributes = new
                {
                    title = newMovie.Title,
                    releaseYear = newMovie.ReleaseYear,
                    durationInSeconds = newMovie.DurationInSeconds
                },
                relationships = new
                {
                    genre = new
                    {
                        data = new
                        {
                            type = "genres",
                            id = "1"
                        }
                    },
                    cast = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "actors",
                                id = "1"
                            }
                        }
                    }
                }
            }
        };

        const string route = "/movies";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: write:actors write:genres write:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_create_resource_with_relationships_with_read_scopes()
    {
        // Arrange
        Movie newMovie = _fakers.Movie.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "movies",
                attributes = new
                {
                    title = newMovie.Title,
                    releaseYear = newMovie.ReleaseYear,
                    durationInSeconds = newMovie.DurationInSeconds
                },
                relationships = new
                {
                    genre = new
                    {
                        data = new
                        {
                            type = "genres",
                            id = "1"
                        }
                    },
                    cast = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "actors",
                                id = "1"
                            }
                        }
                    }
                }
            }
        };

        const string route = "/movies";

        Action<HttpRequestHeaders> setRequestHeaders = headers => headers.Add(ScopeHeaderName, "read:movies read:genres read:actors");

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, setRequestHeaders: setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: write:actors write:genres write:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_update_resource_without_scopes()
    {
        // Arrange
        string newTitle = _fakers.Movie.Generate().Title;

        var requestBody = new
        {
            data = new
            {
                type = "movies",
                id = "1",
                attributes = new
                {
                    title = newTitle
                }
            }
        };

        const string route = "/movies/1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: write:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_update_resource_with_relationships_without_scopes()
    {
        // Arrange
        string newTitle = _fakers.Movie.Generate().Title;

        var requestBody = new
        {
            data = new
            {
                type = "movies",
                id = "1",
                attributes = new
                {
                    title = newTitle
                },
                relationships = new
                {
                    genre = new
                    {
                        data = new
                        {
                            type = "genres",
                            id = "1"
                        }
                    },
                    cast = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "actors",
                                id = "1"
                            }
                        }
                    }
                }
            }
        };

        const string route = "/movies/1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: write:actors write:genres write:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_delete_resource_without_scopes()
    {
        // Arrange
        const string route = "/movies/1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: write:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_update_ToOne_relationship_without_scopes()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "genres",
                id = "1"
            }
        };

        const string route = "/movies/1/relationships/genre";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: write:genres write:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_update_ToMany_relationship_without_scopes()
    {
        // Arrange
        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "actors",
                    id = "1"
                }
            }
        };

        const string route = "/movies/1/relationships/cast";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: write:actors write:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_add_to_ToMany_relationship_without_scopes()
    {
        // Arrange
        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "actors",
                    id = "1"
                }
            }
        };

        const string route = "/movies/1/relationships/cast";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: write:actors write:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_remove_from_ToMany_relationship_without_scopes()
    {
        // Arrange
        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "actors",
                    id = "1"
                }
            }
        };

        const string route = "/movies/1/relationships/cast";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: write:actors write:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }
}
