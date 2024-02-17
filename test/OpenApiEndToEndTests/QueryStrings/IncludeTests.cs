using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client;
using OpenApiEndToEndTests.QueryStrings.GeneratedCode;
using OpenApiTests;
using OpenApiTests.QueryStrings;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiEndToEndTests.QueryStrings;

public sealed class IncludeTests : IClassFixture<IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext> _testContext;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly QueryStringFakers _fakers = new();

    public IncludeTests(IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _testOutputHelper = testOutputHelper;

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

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new QueryStringsClient(httpClient, _testOutputHelper);

        var queryString = new Dictionary<string, string?>
        {
            ["include"] = "values.owner"
        };

        // Act
        ApiResponse<NodeCollectionResponseDocument> response = await apiClient.GetNodeCollectionAsync(queryString);

        // Assert
        response.Result.Data.ShouldHaveCount(1);
        response.Result.Data.ElementAt(0).Id.Should().Be(node.StringId);

        response.Result.Included.Should().HaveCount(2);
        response.Result.Included.Should().ContainSingle(include => include is NameValuePairDataInResponse && include.Id == node.Values.ElementAt(0).StringId);
        response.Result.Included.Should().ContainSingle(include => include is NameValuePairDataInResponse && include.Id == node.Values.ElementAt(1).StringId);
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

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new QueryStringsClient(httpClient, _testOutputHelper);

        var queryString = new Dictionary<string, string?>
        {
            ["include"] = "children.parent,values"
        };

        // Act
        ApiResponse<NodePrimaryResponseDocument> response = await apiClient.GetNodeAsync(node.StringId!, queryString);

        // Assert
        response.Result.Data.Id.Should().Be(node.StringId);

        response.Result.Included.Should().HaveCount(3);
        response.Result.Included.Should().ContainSingle(include => include is NodeDataInResponse && include.Id == node.Children.ElementAt(0).StringId);
        response.Result.Included.Should().ContainSingle(include => include is NodeDataInResponse && include.Id == node.Children.ElementAt(1).StringId);
        response.Result.Included.Should().ContainSingle(include => include is NameValuePairDataInResponse && include.Id == node.Values[0].StringId);
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

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new QueryStringsClient(httpClient, _testOutputHelper);

        var queryString = new Dictionary<string, string?>
        {
            ["include"] = "owner.parent,owner.values"
        };

        // Act
        ApiResponse<NameValuePairCollectionResponseDocument> response = await apiClient.GetNodeValuesAsync(node.StringId!, queryString);

        // Assert
        response.Result.Data.ShouldHaveCount(2);

        response.Result.Included.ShouldHaveCount(2);
        response.Result.Included.Should().ContainSingle(include => include is NodeDataInResponse && include.Id == node.StringId);
        response.Result.Included.Should().ContainSingle(include => include is NodeDataInResponse && include.Id == node.Parent.StringId);
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

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new QueryStringsClient(httpClient, _testOutputHelper);

        var queryString = new Dictionary<string, string?>
        {
            ["include"] = "parent.parent"
        };

        // Act
        ApiResponse<NullableNodeSecondaryResponseDocument> response = await apiClient.GetNodeParentAsync(node.StringId!, queryString);

        // Assert
        response.Result.Data.ShouldNotBeNull();
        response.Result.Data.Id.Should().Be(node.Parent.StringId);

        response.Result.Included.Should().HaveCount(1);

        NodeDataInResponse? include = response.Result.Included.ElementAt(0).Should().BeOfType<NodeDataInResponse>().Subject;
        include.Id.Should().Be(node.Parent.Parent.StringId);
        include.Attributes.Name.Should().Be(node.Parent.Parent.Name);
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

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new QueryStringsClient(httpClient, _testOutputHelper);

        var queryString = new Dictionary<string, string?>
        {
            ["include"] = null
        };

        // Act
        ApiResponse<NodePrimaryResponseDocument> response = await apiClient.GetNodeAsync(node.StringId!, queryString);

        // Assert
        response.Result.Data.Id.Should().Be(node.StringId);

        response.Result.Included.Should().BeEmpty();
    }
}
