using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

public sealed class ScopeReadTests : IClassFixture<IntegrationTestContext<ScopesStartup<ScopesDbContext>, ScopesDbContext>>
{
    private const string ScopeHeaderName = "X-Auth-Scopes";
    private readonly IntegrationTestContext<ScopesStartup<ScopesDbContext>, ScopesDbContext> _testContext;
    private readonly ScopesFakers _fakers = new();

    public ScopeReadTests(IntegrationTestContext<ScopesStartup<ScopesDbContext>, ScopesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<MoviesController>();
        testContext.UseController<ActorsController>();
        testContext.UseController<GenresController>();
    }

    [Fact]
    public async Task Cannot_get_primary_resources_without_scopes()
    {
        // Arrange
        const string route = "/movies";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: read:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_get_primary_resources_with_incorrect_scopes()
    {
        // Arrange
        const string route = "/movies";

        Action<HttpRequestHeaders> setRequestHeaders = headers => headers.Add(ScopeHeaderName, "read:actors write:genres");

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: read:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Can_get_primary_resources_with_correct_scope()
    {
        // Arrange
        Movie movie = _fakers.Movie.GenerateOne();
        movie.Genre = _fakers.Genre.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Movie>();
            dbContext.Movies.Add(movie);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/movies";

        Action<HttpRequestHeaders> setRequestHeaders = headers => headers.Add(ScopeHeaderName, "read:movies");

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("movies");
        responseDocument.Data.ManyValue[0].Id.Should().Be(movie.StringId);
        responseDocument.Data.ManyValue[0].Attributes.ShouldNotBeEmpty();
        responseDocument.Data.ManyValue[0].Relationships.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Can_get_primary_resources_with_write_scope()
    {
        // Arrange
        Genre genre = _fakers.Genre.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Genre>();
            dbContext.Genres.Add(genre);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/genres";

        Action<HttpRequestHeaders> setRequestHeaders = headers => headers.Add(ScopeHeaderName, "write:genres");

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("genres");
        responseDocument.Data.ManyValue[0].Id.Should().Be(genre.StringId);
        responseDocument.Data.ManyValue[0].Attributes.ShouldNotBeEmpty();
        responseDocument.Data.ManyValue[0].Relationships.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Can_get_primary_resources_with_redundant_scopes()
    {
        // Arrange
        Actor actor = _fakers.Actor.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Actor>();
            dbContext.Actors.Add(actor);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/actors";

        Action<HttpRequestHeaders> setRequestHeaders = headers => headers.Add(ScopeHeaderName, "read:genres read:actors write:movies");

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("actors");
        responseDocument.Data.ManyValue[0].Id.Should().Be(actor.StringId);
        responseDocument.Data.ManyValue[0].Attributes.ShouldNotBeEmpty();
        responseDocument.Data.ManyValue[0].Relationships.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Cannot_get_primary_resource_without_scopes()
    {
        // Arrange
        const string route = "/actors/1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: read:actors.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_get_secondary_resource_without_scopes()
    {
        // Arrange
        const string route = "/movies/1/genre";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: read:genres read:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_get_secondary_resources_without_scopes()
    {
        // Arrange
        const string route = "/genres/1/movies";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: read:genres read:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_get_ToOne_relationship_without_scopes()
    {
        // Arrange
        const string route = "/movies/1/relationships/genre";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: read:genres read:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_get_ToMany_relationship_without_scopes()
    {
        // Arrange
        const string route = "/genres/1/relationships/movies";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: read:genres read:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_include_with_insufficient_scopes()
    {
        // Arrange
        const string route = "/movies?include=genre,cast";

        Action<HttpRequestHeaders> setRequestHeaders = headers => headers.Add(ScopeHeaderName, "read:movies");

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: read:actors read:genres read:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_filter_with_insufficient_scopes()
    {
        // Arrange
        const string route = "/movies?filter=and(has(cast),equals(genre.name,'some'))";

        Action<HttpRequestHeaders> setRequestHeaders = headers => headers.Add(ScopeHeaderName, "read:movies");

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: read:actors read:genres read:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }

    [Fact]
    public async Task Cannot_sort_with_insufficient_scopes()
    {
        // Arrange
        const string route = "/movies?sort=count(cast),genre.name";

        Action<HttpRequestHeaders> setRequestHeaders = headers => headers.Add(ScopeHeaderName, "read:movies");

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        error.Title.Should().Be("Insufficient permissions to perform this request.");
        error.Detail.Should().Be("Performing this request requires the following scopes: read:actors read:genres read:movies.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(ScopeHeaderName);
    }
}
