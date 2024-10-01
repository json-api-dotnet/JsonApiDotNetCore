using FluentAssertions;
using OpenApiNSwagEndToEndTests.QueryStrings.GeneratedCode;
using OpenApiTests;
using OpenApiTests.QueryStrings;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.QueryStrings;

public sealed class IncludeTests : IClassFixture<IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly QueryStringFakers _fakers = new();

    public IncludeTests(IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

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

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new QueryStringsClient(httpClient);

        var queryString = new Dictionary<string, string?>
        {
            ["include"] = "values.owner"
        };

        // Act
        NodeCollectionResponseDocument response = await apiClient.GetNodeCollectionAsync(queryString);

        // Assert
        response.Data.ShouldHaveCount(1);
        response.Data.ElementAt(0).Id.Should().Be(node.StringId);

        response.Included.ShouldHaveCount(2);

        response.Included.Should().ContainSingle(include =>
            include is NameValuePairDataInResponse && ((NameValuePairDataInResponse)include).Id == node.Values.ElementAt(0).StringId);

        response.Included.Should().ContainSingle(include =>
            include is NameValuePairDataInResponse && ((NameValuePairDataInResponse)include).Id == node.Values.ElementAt(1).StringId);
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

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new QueryStringsClient(httpClient);

        var queryString = new Dictionary<string, string?>
        {
            ["include"] = "children.parent,values"
        };

        // Act
        NodePrimaryResponseDocument response = await apiClient.GetNodeAsync(node.StringId!, queryString);

        // Assert
        response.Data.Id.Should().Be(node.StringId);

        response.Included.ShouldHaveCount(3);

        response.Included.Should().ContainSingle(include =>
            include is NodeDataInResponse && ((NodeDataInResponse)include).Id == node.Children.ElementAt(0).StringId);

        response.Included.Should().ContainSingle(include =>
            include is NodeDataInResponse && ((NodeDataInResponse)include).Id == node.Children.ElementAt(1).StringId);

        response.Included.Should().ContainSingle(include =>
            include is NameValuePairDataInResponse && ((NameValuePairDataInResponse)include).Id == node.Values[0].StringId);
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

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new QueryStringsClient(httpClient);

        var queryString = new Dictionary<string, string?>
        {
            ["include"] = "owner.parent,owner.values"
        };

        // Act
        NameValuePairCollectionResponseDocument response = await apiClient.GetNodeValuesAsync(node.StringId!, queryString);

        // Assert
        response.Data.ShouldHaveCount(2);

        response.Included.ShouldHaveCount(2);
        response.Included.Should().ContainSingle(include => include is NodeDataInResponse && ((NodeDataInResponse)include).Id == node.StringId);
        response.Included.Should().ContainSingle(include => include is NodeDataInResponse && ((NodeDataInResponse)include).Id == node.Parent.StringId);
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

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new QueryStringsClient(httpClient);

        var queryString = new Dictionary<string, string?>
        {
            ["include"] = "parent.parent"
        };

        // Act
        NullableNodeSecondaryResponseDocument response = await apiClient.GetNodeParentAsync(node.StringId!, queryString);

        // Assert
        response.Data.ShouldNotBeNull();
        response.Data.Id.Should().Be(node.Parent.StringId);

        response.Included.ShouldHaveCount(1);

        NodeDataInResponse? include = response.Included.ElementAt(0).Should().BeOfType<NodeDataInResponse>().Subject;
        include.Id.Should().Be(node.Parent.Parent.StringId);
        include.Attributes.ShouldNotBeNull();
        include.Attributes.Name.Should().Be(node.Parent.Parent.Name);
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

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new QueryStringsClient(httpClient);

        var queryString = new Dictionary<string, string?>
        {
            ["include"] = null
        };

        // Act
        NodePrimaryResponseDocument response = await apiClient.GetNodeAsync(node.StringId!, queryString);

        // Assert
        response.Data.Id.Should().Be(node.StringId);

        response.Included.Should().BeEmpty();
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
