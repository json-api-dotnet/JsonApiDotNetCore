using System.Net;
using FluentAssertions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode;
using OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.QueryStrings;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.QueryStrings;

public sealed class SortTests : IClassFixture<IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly QueryStringFakers _fakers = new();

    public SortTests(IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<NodesController>();
    }

    [Fact]
    public async Task Can_sort_in_primary_resources()
    {
        // Arrange
        List<Node> nodes = _fakers.Node.Generate(2);
        nodes[0].Name = "A";
        nodes[1].Name = "B";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Node>();
            dbContext.Nodes.AddRange(nodes);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["sort"] = "-name"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodeCollectionResponseDocument? response = await apiClient.Nodes.GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldHaveCount(2);
            response.Data.ElementAt(0).Id.Should().Be(nodes[1].StringId);
            response.Data.ElementAt(1).Id.Should().Be(nodes[0].StringId);
        }
    }

    [Fact]
    public async Task Can_sort_in_secondary_resources()
    {
        // Arrange
        Node node = _fakers.Node.Generate();
        node.Children = _fakers.Node.Generate(2).ToHashSet();
        node.Children.ElementAt(0).Name = "B";
        node.Children.ElementAt(1).Name = "A";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Node>();
            dbContext.Nodes.Add(node);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["sort"] = "name"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodeCollectionResponseDocument? response = await apiClient.Nodes[node.StringId].Children.GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldHaveCount(2);
            response.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(1).StringId);
            response.Data.ElementAt(1).Id.Should().Be(node.Children.ElementAt(0).StringId);
        }
    }

    [Fact]
    public async Task Can_sort_at_ToMany_relationship_endpoint()
    {
        // Arrange
        Node node = _fakers.Node.Generate();
        node.Children = _fakers.Node.Generate(2).ToHashSet();
        node.Children.ElementAt(0).Children = _fakers.Node.Generate(1).ToHashSet();
        node.Children.ElementAt(1).Children = _fakers.Node.Generate(2).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Node>();
            dbContext.Nodes.Add(node);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["sort"] = "count(children)"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodeIdentifierCollectionResponseDocument? response = await apiClient.Nodes[node.StringId].Relationships.Children.GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldHaveCount(2);
            response.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(0).StringId);
            response.Data.ElementAt(1).Id.Should().Be(node.Children.ElementAt(1).StringId);
        }
    }

    [Fact]
    public async Task Cannot_use_empty_sort()
    {
        // Arrange
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["sort"] = null
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            Func<Task> action = async () => _ = await apiClient.Nodes[Unknown.StringId.Int64].GetAsync();

            // Assert
            ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
            exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            exception.Errors.ShouldHaveCount(1);

            ErrorObject error = exception.Errors.ElementAt(0);
            error.Status.Should().Be("400");
            error.Title.Should().Be("Missing query string parameter value.");
            error.Detail.Should().Be("Missing value for 'sort' query string parameter.");
            error.Source.ShouldNotBeNull();
            error.Source.Parameter.Should().Be("sort");
        }
    }
}
