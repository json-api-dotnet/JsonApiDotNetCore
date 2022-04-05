using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

public sealed class WriteOnlyControllerTests : IClassFixture<IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
    private readonly RestrictionFakers _fakers = new();

    public WriteOnlyControllerTests(IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<TablesController>();
    }

    [Fact]
    public async Task Cannot_get_resources()
    {
        // Arrange
        const string route = "/tables?fields[tables]=legCount";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("The requested endpoint is not accessible.");
        error.Detail.Should().Be("Endpoint '/tables' is not accessible for GET requests.");
    }

    [Fact]
    public async Task Cannot_get_resource()
    {
        // Arrange
        Table table = _fakers.Table.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Tables.Add(table);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/tables/{table.StringId}";

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
    public async Task Cannot_get_secondary_resources()
    {
        // Arrange
        Table table = _fakers.Table.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Tables.Add(table);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/tables/{table.StringId}/chairs";

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
        Table table = _fakers.Table.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Tables.Add(table);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/tables/{table.StringId}/room";

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
        Table table = _fakers.Table.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Tables.Add(table);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/tables/{table.StringId}/relationships/chairs";

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
                type = "tables",
                attributes = new
                {
                }
            }
        };

        const string route = "/tables";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Can_update_resource()
    {
        // Arrange
        Table existingTable = _fakers.Table.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Tables.Add(existingTable);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "tables",
                id = existingTable.StringId,
                attributes = new
                {
                }
            }
        };

        string route = $"/tables/{existingTable.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Can_delete_resource()
    {
        // Arrange
        Table existingTable = _fakers.Table.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Tables.Add(existingTable);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/tables/{existingTable.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Can_update_relationship()
    {
        // Arrange
        Table existingTable = _fakers.Table.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Tables.Add(existingTable);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = (object?)null
        };

        string route = $"/tables/{existingTable.StringId}/relationships/room";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Can_add_to_ToMany_relationship()
    {
        // Arrange
        Table existingTable = _fakers.Table.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Tables.Add(existingTable);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string route = $"/tables/{existingTable.StringId}/relationships/chairs";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Can_remove_from_ToMany_relationship()
    {
        // Arrange
        Table existingTable = _fakers.Table.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Tables.Add(existingTable);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = Array.Empty<object>()
        };

        string route = $"/tables/{existingTable.StringId}/relationships/chairs";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
    }
}
