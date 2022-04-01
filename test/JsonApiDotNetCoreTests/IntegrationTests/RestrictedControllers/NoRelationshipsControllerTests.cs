using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

public sealed class NoRelationshipsControllerTests : IClassFixture<IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
    private readonly RestrictionFakers _fakers = new();

    public NoRelationshipsControllerTests(IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ChairsController>();
    }

    [Fact]
    public async Task Can_get_resources()
    {
        // Arrange
        const string route = "/chairs";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Can_get_resource()
    {
        // Arrange
        Chair chair = _fakers.Chair.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Chairs.Add(chair);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/chairs/{chair.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Cannot_get_secondary_resources()
    {
        // Arrange
        Chair chair = _fakers.Chair.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Chairs.Add(chair);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/chairs/{chair.StringId}/pillows";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("The requested endpoint is not accessible.");
        error.Detail.Should().Be($"Endpoint '{route}' is not accessible for GET requests.");
    }

    [Fact]
    public async Task Cannot_get_secondary_resource()
    {
        // Arrange
        Chair chair = _fakers.Chair.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Chairs.Add(chair);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/chairs/{chair.StringId}/room";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("The requested endpoint is not accessible.");
        error.Detail.Should().Be($"Endpoint '{route}' is not accessible for GET requests.");
    }

    [Fact]
    public async Task Cannot_get_relationship()
    {
        // Arrange
        Chair chair = _fakers.Chair.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Chairs.Add(chair);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/chairs/{chair.StringId}/relationships/pillows";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("The requested endpoint is not accessible.");
        error.Detail.Should().Be($"Endpoint '{route}' is not accessible for GET requests.");
    }

    [Fact]
    public async Task Can_create_resource()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "chairs",
                attributes = new
                {
                }
            }
        };

        const string route = "/chairs";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Can_update_resource()
    {
        // Arrange
        Chair existingChair = _fakers.Chair.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Chairs.Add(existingChair);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "chairs",
                id = existingChair.StringId,
                attributes = new
                {
                }
            }
        };

        string route = $"/chairs/{existingChair.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Can_delete_resource()
    {
        // Arrange
        Chair existingChair = _fakers.Chair.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Chairs.Add(existingChair);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/chairs/{existingChair.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Cannot_update_relationship()
    {
        // Arrange
        Chair existingChair = _fakers.Chair.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Chairs.Add(existingChair);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/chairs/{existingChair.StringId}/relationships/room";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("The requested endpoint is not accessible.");
        error.Detail.Should().Be($"Endpoint '{route}' is not accessible for PATCH requests.");
    }

    [Fact]
    public async Task Cannot_add_to_ToMany_relationship()
    {
        // Arrange
        Chair existingChair = _fakers.Chair.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Chairs.Add(existingChair);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string route = $"/chairs/{existingChair.StringId}/relationships/pillows";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("The requested endpoint is not accessible.");
        error.Detail.Should().Be($"Endpoint '{route}' is not accessible for POST requests.");
    }

    [Fact]
    public async Task Cannot_remove_from_ToMany_relationship()
    {
        // Arrange
        Chair existingChair = _fakers.Chair.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Chairs.Add(existingChair);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string route = $"/chairs/{existingChair.StringId}/relationships/pillows";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("The requested endpoint is not accessible.");
        error.Detail.Should().Be($"Endpoint '{route}' is not accessible for DELETE requests.");
    }
}
