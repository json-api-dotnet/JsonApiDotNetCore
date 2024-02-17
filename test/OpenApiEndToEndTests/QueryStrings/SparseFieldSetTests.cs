using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client;
using OpenApiEndToEndTests.QueryStrings.GeneratedCode;
using OpenApiTests;
using OpenApiTests.QueryStrings;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiEndToEndTests.QueryStrings;

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
        ApiResponse<NodeCollectionResponseDocument> response = await apiClient.GetNodeCollectionAsync(queryString);

        // Assert
        response.Result.Data.Should().HaveCount(1);
        response.Result.Data.ElementAt(0).Id.Should().Be(node.StringId);
        response.Result.Data.ElementAt(0).Attributes.Name.Should().Be(node.Name);
        response.Result.Data.ElementAt(0).Attributes.Comment.Should().BeNull();
        response.Result.Data.ElementAt(0).Relationships.Should().BeNull();
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
        ApiResponse<NodePrimaryResponseDocument> response = await apiClient.GetNodeAsync(node.StringId!, queryString);

        // Assert
        response.Result.Data.Id.Should().Be(node.StringId);
        response.Result.Data.Attributes.Name.Should().BeNull();
        response.Result.Data.Attributes.Comment.Should().Be(node.Comment);
        response.Result.Data.Relationships.Parent.Should().NotBeNull();
        response.Result.Data.Relationships.Children.Should().BeNull();
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
        ApiResponse<NodeCollectionResponseDocument> response = await apiClient.GetNodeChildrenAsync(node.StringId!, queryString);

        // Assert
        response.Result.Data.Should().HaveCount(1);
        response.Result.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(0).StringId);
        response.Result.Data.ElementAt(0).Attributes.Name.Should().BeNull();
        response.Result.Data.ElementAt(0).Attributes.Comment.Should().Be(node.Children.ElementAt(0).Comment);
        response.Result.Data.ElementAt(0).Relationships.Parent.Should().BeNull();
        response.Result.Data.ElementAt(0).Relationships.Children.Should().NotBeNull();
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
        ApiResponse<NullableNodeSecondaryResponseDocument> response = await apiClient.GetNodeParentAsync(node.StringId!, queryString);

        // Assert
        response.Result.Data.ShouldNotBeNull();
        response.Result.Data.Id.Should().Be(node.Parent.StringId);
        response.Result.Data.Attributes.Name.Should().BeNull();
        response.Result.Data.Attributes.Comment.Should().Be(node.Parent.Comment);
        response.Result.Data.Relationships.Parent.Should().BeNull();
        response.Result.Data.Relationships.Children.Should().NotBeNull();
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
        ApiResponse<NodePrimaryResponseDocument> response = await apiClient.GetNodeAsync(node.StringId!, queryString);

        // Assert
        response.Result.Data.Id.Should().Be(node.StringId);
        response.Result.Data.Attributes.Should().BeNull();
    }
}
