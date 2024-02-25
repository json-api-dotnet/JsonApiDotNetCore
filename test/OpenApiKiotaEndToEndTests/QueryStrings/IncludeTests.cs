using FluentAssertions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiNSwagEndToEndTests.QueryStrings.GeneratedCode;
using OpenApiNSwagEndToEndTests.QueryStrings.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.QueryStrings;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.QueryStrings;

public sealed class IncludeTests : IClassFixture<IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly QueryStringFakers _fakers = new();

    public IncludeTests(IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext> testContext, ITestOutputHelper testOutputHelper)
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
        Node node = _fakers.Node.Generate();
        node.Values = _fakers.NameValuePair.Generate(2);

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
            response.ShouldNotBeNull();
            response.Data.ShouldHaveCount(1);
            response.Data.ElementAt(0).Id.Should().Be(node.StringId);

            response.Included.ShouldHaveCount(2);
            response.Included.Should().ContainSingle(include => include is NameValuePairDataInResponse && include.Id == node.Values.ElementAt(0).StringId);
            response.Included.Should().ContainSingle(include => include is NameValuePairDataInResponse && include.Id == node.Values.ElementAt(1).StringId);
        }
    }

    [Fact]
    public async Task Can_include_in_primary_resource()
    {
        // Arrange
        Node node = _fakers.Node.Generate();
        node.Values = _fakers.NameValuePair.Generate(1);
        node.Children = _fakers.Node.Generate(2).ToHashSet();

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
            NodePrimaryResponseDocument? response = await apiClient.Nodes[node.StringId].GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Id.Should().Be(node.StringId);

            response.Included.ShouldHaveCount(3);
            response.Included.Should().ContainSingle(include => include is NodeDataInResponse && include.Id == node.Children.ElementAt(0).StringId);
            response.Included.Should().ContainSingle(include => include is NodeDataInResponse && include.Id == node.Children.ElementAt(1).StringId);
            response.Included.Should().ContainSingle(include => include is NameValuePairDataInResponse && include.Id == node.Values[0].StringId);
        }
    }

    [Fact]
    public async Task Can_include_in_secondary_resources()
    {
        // Arrange
        Node node = _fakers.Node.Generate();
        node.Parent = _fakers.Node.Generate();
        node.Values = _fakers.NameValuePair.Generate(2);

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
            NameValuePairCollectionResponseDocument? response = await apiClient.Nodes[node.StringId].Values.GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldHaveCount(2);

            response.Included.ShouldHaveCount(2);
            response.Included.Should().ContainSingle(include => include is NodeDataInResponse && include.Id == node.StringId);
            response.Included.Should().ContainSingle(include => include is NodeDataInResponse && include.Id == node.Parent.StringId);
        }
    }

    [Fact]
    public async Task Can_include_in_secondary_resource()
    {
        // Arrange
        Node node = _fakers.Node.Generate();
        node.Parent = _fakers.Node.Generate();
        node.Parent.Parent = _fakers.Node.Generate();

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
            NullableNodeSecondaryResponseDocument? response = await apiClient.Nodes[node.StringId].Parent.GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Id.Should().Be(node.Parent.StringId);

            response.Included.ShouldHaveCount(1);

            NodeDataInResponse? include = response.Included.ElementAt(0).Should().BeOfType<NodeDataInResponse>().Subject;
            include.Id.Should().Be(node.Parent.Parent.StringId);
            include.Attributes.ShouldNotBeNull();
            include.Attributes.Name.Should().Be(node.Parent.Parent.Name);
        }
    }

    [Fact]
    public async Task Can_use_empty_include()
    {
        // Arrange
        Node node = _fakers.Node.Generate();

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
            NodePrimaryResponseDocument? response = await apiClient.Nodes[node.StringId].GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Id.Should().Be(node.StringId);

            response.Included.Should().BeEmpty();
        }
    }
}
