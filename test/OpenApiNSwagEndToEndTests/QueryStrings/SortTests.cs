using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using OpenApiNSwagEndToEndTests.QueryStrings.GeneratedCode;
using OpenApiTests;
using OpenApiTests.QueryStrings;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.QueryStrings;

public sealed class SortTests : IClassFixture<IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly QueryStringFakers _fakers = new();

    public SortTests(IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<NodesController>();
    }

    [Fact]
    public async Task Can_sort_in_primary_resources()
    {
        // Arrange
        List<Node> nodes = _fakers.Node.GenerateList(2);
        nodes[0].Name = "A";
        nodes[1].Name = "B";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Node>();
            dbContext.Nodes.AddRange(nodes);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new QueryStringsClient(httpClient);

        var queryString = new Dictionary<string, string?>
        {
            ["sort"] = "-name"
        };

        // Act
        NodeCollectionResponseDocument response = await apiClient.GetNodeCollectionAsync(queryString);

        // Assert
        response.Data.Should().HaveCount(2);
        response.Data.ElementAt(0).Id.Should().Be(nodes[1].StringId);
        response.Data.ElementAt(1).Id.Should().Be(nodes[0].StringId);
    }

    [Fact]
    public async Task Can_sort_in_secondary_resources()
    {
        // Arrange
        Node node = _fakers.Node.GenerateOne();
        node.Children = _fakers.Node.GenerateSet(2);
        node.Children.ElementAt(0).Name = "B";
        node.Children.ElementAt(1).Name = "A";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Node>();
            dbContext.Nodes.Add(node);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new QueryStringsClient(httpClient);

        var queryString = new Dictionary<string, string?>
        {
            ["sort"] = "name"
        };

        // Act
        NodeCollectionResponseDocument response = await apiClient.GetNodeChildrenAsync(node.StringId!, queryString);

        // Assert
        response.Data.Should().HaveCount(2);
        response.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(1).StringId);
        response.Data.ElementAt(1).Id.Should().Be(node.Children.ElementAt(0).StringId);
    }

    [Fact]
    public async Task Can_sort_at_ToMany_relationship_endpoint()
    {
        // Arrange
        Node node = _fakers.Node.GenerateOne();
        node.Children = _fakers.Node.GenerateSet(2);
        node.Children.ElementAt(0).Children = _fakers.Node.GenerateSet(1);
        node.Children.ElementAt(1).Children = _fakers.Node.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Node>();
            dbContext.Nodes.Add(node);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new QueryStringsClient(httpClient);

        var queryString = new Dictionary<string, string?>
        {
            ["sort"] = "count(children)"
        };

        // Act
        NodeIdentifierCollectionResponseDocument response = await apiClient.GetNodeChildrenRelationshipAsync(node.StringId!, queryString);

        // Assert
        response.Data.Should().HaveCount(2);
        response.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(0).StringId);
        response.Data.ElementAt(1).Id.Should().Be(node.Children.ElementAt(1).StringId);
    }

    [Fact]
    public async Task Cannot_use_empty_sort()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new QueryStringsClient(httpClient);

        var queryString = new Dictionary<string, string?>
        {
            ["sort"] = null
        };

        // Act
        Func<Task> action = async () => _ = await apiClient.GetNodeAsync(Unknown.StringId.Int64, queryString);

        // Assert
        ApiException<ErrorResponseDocument> exception = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which;
        exception.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Message.Should().Be("HTTP 400: The query string is invalid.");
        exception.Result.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Result.Errors.ElementAt(0);
        error.Status.Should().Be("400");
        error.Title.Should().Be("Missing query string parameter value.");
        error.Detail.Should().Be("Missing value for 'sort' query string parameter.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("sort");
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
