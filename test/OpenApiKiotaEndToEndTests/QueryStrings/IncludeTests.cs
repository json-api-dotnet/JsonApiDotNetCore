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

public sealed class IncludeTests : IClassFixture<IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly QueryStringFakers _fakers = new();

    public IncludeTests(IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<NodesController>();
        testContext.UseController<NameValuePairsController>();
    }

    [Fact]
    public async Task Can_include_in_primary_resources()
    {
        // Arrange
        Node node = _fakers.Node.GenerateOne();
        node.Values = _fakers.NameValuePair.GenerateList(2);

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
            ["include"] = "values.owner"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodeCollectionResponseDocument? response = await apiClient.Nodes.GetAsync();

            // Assert
            response.Should().NotBeNull();
            response.Data.Should().HaveCount(1);
            response.Data.ElementAt(0).Id.Should().Be(node.StringId);

            response.Included.Should().HaveCount(2);
            response.Included.OfType<NameValuePairDataInResponse>().Should().ContainSingle(include => include.Id == node.Values.ElementAt(0).StringId);
            response.Included.OfType<NameValuePairDataInResponse>().Should().ContainSingle(include => include.Id == node.Values.ElementAt(1).StringId);
        }
    }

    [Fact]
    public async Task Can_include_in_primary_resource()
    {
        // Arrange
        Node node = _fakers.Node.GenerateOne();
        node.Values = _fakers.NameValuePair.GenerateList(1);
        node.Children = _fakers.Node.GenerateSet(2);

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
            ["include"] = "children.parent,values"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodePrimaryResponseDocument? response = await apiClient.Nodes[node.StringId!].GetAsync();

            // Assert
            response.Should().NotBeNull();
            response.Data.Should().NotBeNull();
            response.Data.Id.Should().Be(node.StringId);

            response.Included.Should().HaveCount(3);
            response.Included.OfType<NodeDataInResponse>().Should().ContainSingle(include => include.Id == node.Children.ElementAt(0).StringId);
            response.Included.OfType<NodeDataInResponse>().Should().ContainSingle(include => include.Id == node.Children.ElementAt(1).StringId);
            response.Included.OfType<NameValuePairDataInResponse>().Should().ContainSingle(include => include.Id == node.Values[0].StringId);
        }
    }

    [Fact]
    public async Task Can_include_in_secondary_resources()
    {
        // Arrange
        Node node = _fakers.Node.GenerateOne();
        node.Parent = _fakers.Node.GenerateOne();
        node.Values = _fakers.NameValuePair.GenerateList(2);

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
            ["include"] = "owner.parent,owner.values"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NameValuePairCollectionResponseDocument? response = await apiClient.Nodes[node.StringId!].Values.GetAsync();

            // Assert
            response.Should().NotBeNull();
            response.Data.Should().HaveCount(2);

            response.Included.Should().HaveCount(2);
            response.Included.OfType<NodeDataInResponse>().Should().ContainSingle(include => include.Id == node.StringId);
            response.Included.OfType<NodeDataInResponse>().Should().ContainSingle(include => include.Id == node.Parent.StringId);
        }
    }

    [Fact]
    public async Task Can_include_in_secondary_resource()
    {
        // Arrange
        Node node = _fakers.Node.GenerateOne();
        node.Parent = _fakers.Node.GenerateOne();
        node.Parent.Parent = _fakers.Node.GenerateOne();

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
            ["include"] = "parent.parent"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NullableNodeSecondaryResponseDocument? response = await apiClient.Nodes[node.StringId!].Parent.GetAsync();

            // Assert
            response.Should().NotBeNull();
            response.Data.Should().NotBeNull();
            response.Data.Id.Should().Be(node.Parent.StringId);

            response.Included.Should().HaveCount(1);

            response.Included.OfType<NodeDataInResponse>().Should().ContainSingle(nodeData => nodeData.Id == node.Parent.Parent.StringId).Subject.With(
                nodeData =>
                {
                    nodeData.Attributes.Should().NotBeNull();
                    nodeData.Attributes.Name.Should().Be(node.Parent.Parent.Name);
                });
        }
    }

    [Fact]
    public async Task Can_use_empty_include()
    {
        // Arrange
        Node node = _fakers.Node.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Nodes.Add(node);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["include"] = null
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodePrimaryResponseDocument? response = await apiClient.Nodes[node.StringId!].GetAsync();

            // Assert
            response.Should().NotBeNull();
            response.Data.Should().NotBeNull();
            response.Data.Id.Should().Be(node.StringId);

            response.Included.Should().BeEmpty();
        }
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
