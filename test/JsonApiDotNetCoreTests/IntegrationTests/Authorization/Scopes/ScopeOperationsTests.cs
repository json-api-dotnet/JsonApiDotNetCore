using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

public sealed class ScopeOperationsTests : IClassFixture<IntegrationTestContext<ScopesStartup<ScopesDbContext>, ScopesDbContext>>
{
    private const string ScopeHeaderName = "X-Auth-Scopes";
    private readonly IntegrationTestContext<ScopesStartup<ScopesDbContext>, ScopesDbContext> _testContext;
    private readonly ScopesFakers _fakers = new();

    public ScopeOperationsTests(IntegrationTestContext<ScopesStartup<ScopesDbContext>, ScopesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();
        testContext.UseController<MoviesController>();
        testContext.UseController<ActorsController>();
        testContext.UseController<GenresController>();
    }

    [Fact]
    public async Task Cannot_create_resources_without_scopes()
    {
        // Arrange
        Genre newGenre = _fakers.Genre.GenerateOne();
        Movie newMovie = _fakers.Movie.GenerateOne();

        const string genreLocalId = "genre-1";

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "genres",
                        lid = genreLocalId,
                        attributes = new
                        {
                            name = newGenre.Name
                        }
                    }
                },
                new
                {
                    op = "add",
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
                                    lid = genreLocalId
                                }
                            }
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

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
    public async Task Cannot_create_resource_with_read_scope()
    {
        // Arrange
        Genre newGenre = _fakers.Genre.GenerateOne();

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "genres",
                        attributes = new
                        {
                            name = newGenre.Name
                        }
                    }
                }
            }
        };

        const string route = "/operations";
        string contentType = JsonApiMediaType.AtomicOperations.ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(ScopeHeaderName, "read:genres");
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(JsonApiMediaType.AtomicOperations.ToString()));
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: write:genres.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_update_resources_without_scopes()
    {
        // Arrange
        string newTitle = _fakers.Movie.GenerateOne().Title;
        DateTime newBornAt = _fakers.Actor.GenerateOne().BornAt;

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "movies",
                        id = "1",
                        attributes = new
                        {
                            title = newTitle
                        }
                    }
                },
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "actors",
                        id = "1",
                        attributes = new
                        {
                            bornAt = newBornAt
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

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
    public async Task Cannot_update_resource_with_relationships_without_scopes()
    {
        // Arrange
        string newTitle = _fakers.Movie.GenerateOne().Title;

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
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
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

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
    public async Task Cannot_delete_resources_without_scopes()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "genres",
                        id = "1"
                    }
                },
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "actors",
                        id = "1"
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: write:actors write:genres.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_update_ToOne_relationship_without_scopes()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "movies",
                        id = "1",
                        relationship = "genre"
                    },
                    data = (object?)null
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

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
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "movies",
                        id = "1",
                        relationship = "cast"
                    },
                    data = Array.Empty<object>()
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

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
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    @ref = new
                    {
                        type = "movies",
                        id = "1",
                        relationship = "cast"
                    },
                    data = Array.Empty<object>()
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

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
            atomic__operations = new[]
            {
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "movies",
                        id = "1",
                        relationship = "cast"
                    },
                    data = Array.Empty<object>()
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

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
