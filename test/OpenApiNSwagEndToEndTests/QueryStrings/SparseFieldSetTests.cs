using FluentAssertions;
using OpenApiNSwagEndToEndTests.QueryStrings.GeneratedCode;
using OpenApiTests;
using OpenApiTests.QueryStrings;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.QueryStrings;

public sealed class SparseFieldSetTests : IClassFixture<IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext> _testContext;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly QueryStringFakers _fakers = new();

    public SparseFieldSetTests(IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _testOutputHelper = testOutputHelper;

        testContext.UseController<NodesController>();
    }

    [Fact]
    public async Task Can_select_attribute_in_primary_resources()
    {
        // Arrange
        Node node = _fakers.Node.Generate();

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
            ["fields[nodes]"] = "name"
        };

        // Act
        NodeCollectionResponseDocument response = await apiClient.GetNodeCollectionAsync(queryString, null);

        // Assert
        response.Data.Should().HaveCount(1);
        response.Data.ElementAt(0).Id.Should().Be(node.StringId);
        response.Data.ElementAt(0).Attributes.Name.Should().Be(node.Name);
        response.Data.ElementAt(0).Attributes.Comment.Should().BeNull();
        response.Data.ElementAt(0).Relationships.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_fields_in_primary_resource()
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
            ["fields[nodes]"] = "comment,parent"
        };

        // Act
        NodePrimaryResponseDocument response = await apiClient.GetNodeAsync(node.StringId!, queryString, null);

        // Assert
        response.Data.Id.Should().Be(node.StringId);
        response.Data.Attributes.Name.Should().BeNull();
        response.Data.Attributes.Comment.Should().Be(node.Comment);
        response.Data.Relationships.Parent.Should().NotBeNull();
        response.Data.Relationships.Children.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_fields_in_secondary_resources()
    {
        // Arrange
        Node node = _fakers.Node.Generate();
        node.Children = _fakers.Node.Generate(1).ToHashSet();

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
            ["fields[nodes]"] = "comment,children"
        };

        // Act
        NodeCollectionResponseDocument response = await apiClient.GetNodeChildrenAsync(node.StringId!, queryString, null);

        // Assert
        response.Data.Should().HaveCount(1);
        response.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(0).StringId);
        response.Data.ElementAt(0).Attributes.Name.Should().BeNull();
        response.Data.ElementAt(0).Attributes.Comment.Should().Be(node.Children.ElementAt(0).Comment);
        response.Data.ElementAt(0).Relationships.Parent.Should().BeNull();
        response.Data.ElementAt(0).Relationships.Children.Should().NotBeNull();
    }

    [Fact]
    public async Task Can_select_fields_in_secondary_resource()
    {
        // Arrange
        Node node = _fakers.Node.Generate();
        node.Parent = _fakers.Node.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Nodes.Add(node);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new QueryStringsClient(httpClient, _testOutputHelper);

        var queryString = new Dictionary<string, string?>
        {
            ["fields[nodes]"] = "comment,children"
        };

        // Act
        NullableNodeSecondaryResponseDocument response = await apiClient.GetNodeParentAsync(node.StringId!, queryString, null);

        // Assert
        response.Data.ShouldNotBeNull();
        response.Data.Id.Should().Be(node.Parent.StringId);
        response.Data.Attributes.Name.Should().BeNull();
        response.Data.Attributes.Comment.Should().Be(node.Parent.Comment);
        response.Data.Relationships.Parent.Should().BeNull();
        response.Data.Relationships.Children.Should().NotBeNull();
    }

    [Fact]
    public async Task Can_select_empty_fieldset()
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
            ["fields[nodes]"] = null
        };

        // Act
        NodePrimaryResponseDocument response = await apiClient.GetNodeAsync(node.StringId!, queryString, null);

        // Assert
        response.Data.Id.Should().Be(node.StringId);
        response.Data.Attributes.Should().BeNull();
    }
}
