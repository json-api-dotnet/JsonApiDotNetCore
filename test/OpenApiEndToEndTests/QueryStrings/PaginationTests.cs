using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.Exceptions;
using OpenApiEndToEndTests.QueryStrings.GeneratedCode;
using OpenApiTests;
using OpenApiTests.QueryStrings;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiEndToEndTests.QueryStrings;

public sealed class PaginationTests : IClassFixture<IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext> _testContext;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly QueryStringFakers _fakers = new();

    public PaginationTests(IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _testOutputHelper = testOutputHelper;

        testContext.UseController<NodesController>();
    }

    [Fact]
    public async Task Can_paginate_in_primary_resources()
    {
        // Arrange
        List<Node> nodes = _fakers.Node.Generate(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Node>();
            dbContext.Nodes.AddRange(nodes);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new QueryStringsClient(httpClient, _testOutputHelper);

        var queryString = new Dictionary<string, string?>
        {
            ["page[size]"] = "1",
            ["page[number]"] = "2"
        };

        // Act
        NodeCollectionResponseDocument response = await apiClient.GetNodeCollectionAsync(queryString);

        // Assert
        response.Data.Should().HaveCount(1);
        response.Data.ElementAt(0).Id.Should().Be(nodes[1].StringId);
        response.Meta.ShouldNotBeNull();
        response.Meta.ShouldContainKey("total").With(total => total.Should().Be(3));
    }

    [Fact]
    public async Task Can_paginate_in_secondary_resources()
    {
        // Arrange
        Node node = _fakers.Node.Generate();
        node.Children = _fakers.Node.Generate(3).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Node>();
            dbContext.Nodes.Add(node);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new QueryStringsClient(httpClient, _testOutputHelper);

        var queryString = new Dictionary<string, string?>
        {
            ["page[size]"] = "2",
            ["page[number]"] = "1"
        };

        // Act
        NodeCollectionResponseDocument response = await apiClient.GetNodeChildrenAsync(node.StringId!, queryString);

        // Assert
        response.Data.Should().HaveCount(2);
        response.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(0).StringId);
        response.Data.ElementAt(1).Id.Should().Be(node.Children.ElementAt(1).StringId);
        response.Meta.ShouldNotBeNull();
        response.Meta.ShouldContainKey("total").With(total => total.Should().Be(3));
    }

    [Fact]
    public async Task Can_paginate_at_ToMany_relationship_endpoint()
    {
        // Arrange
        Node node = _fakers.Node.Generate();
        node.Children = _fakers.Node.Generate(3).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Node>();
            dbContext.Nodes.Add(node);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new QueryStringsClient(httpClient, _testOutputHelper);

        var queryString = new Dictionary<string, string?>
        {
            ["page[size]"] = "2",
            ["page[number]"] = "2"
        };

        // Act
        NodeIdentifierCollectionResponseDocument response = await apiClient.GetNodeChildrenRelationshipAsync(node.StringId!, queryString);

        // Assert
        response.Data.Should().HaveCount(1);
        response.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(2).StringId);
        response.Meta.ShouldNotBeNull();
        response.Meta.ShouldContainKey("total").With(total => total.Should().Be(3));
    }

    [Fact]
    public async Task Cannot_use_empty_page_size()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new QueryStringsClient(httpClient, _testOutputHelper);

        var queryString = new Dictionary<string, string?>
        {
            ["page[size]"] = null
        };

        // Act
        Func<Task> action = async () => _ = await apiClient.GetNodeAsync(Unknown.StringId.Int64, queryString);

        // Assert
        ApiException exception = (await action.Should().ThrowExactlyAsync<ApiException>()).Which;
        exception.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Message.Should().StartWith("The query string is invalid.");
        exception.Message.Should().Contain("Missing value for 'page[size]' query string parameter.");
    }

    [Fact]
    public async Task Cannot_use_empty_page_number()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new QueryStringsClient(httpClient, _testOutputHelper);

        var queryString = new Dictionary<string, string?>
        {
            ["page[number]"] = null
        };

        // Act
        Func<Task> action = async () => _ = await apiClient.GetNodeAsync(Unknown.StringId.Int64, queryString);

        // Assert
        ApiException exception = (await action.Should().ThrowExactlyAsync<ApiException>()).Which;
        exception.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Message.Should().StartWith("The query string is invalid.");
        exception.Message.Should().Contain("Missing value for 'page[number]' query string parameter.");
    }
}
