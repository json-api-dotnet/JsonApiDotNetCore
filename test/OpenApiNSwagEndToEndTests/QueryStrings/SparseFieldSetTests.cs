using FluentAssertions;
using OpenApiNSwagEndToEndTests.QueryStrings.GeneratedCode;
using OpenApiTests;
using OpenApiTests.QueryStrings;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.QueryStrings;

public sealed class SparseFieldSetTests : IClassFixture<IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly QueryStringFakers _fakers = new();

    public SparseFieldSetTests(IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<NodesController>();
    }

    [Fact]
    public async Task Can_select_attribute_in_primary_resources()
    {
        // Arrange
        Node node = _fakers.Node.GenerateOne();

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
            ["fields[nodes]"] = "name"
        };

        // Act
        NodeCollectionResponseDocument response = await apiClient.GetNodeCollectionAsync(queryString);

        // Assert
        response.Data.Should().HaveCount(1);
        response.Data.ElementAt(0).Id.Should().Be(node.StringId);

        response.Data.ElementAt(0).Attributes.RefShould().NotBeNull().And.Subject.With(attributes =>
        {
            attributes.Name.Should().Be(node.Name);
            attributes.Comment.Should().BeNull();
        });

        response.Data.ElementAt(0).Relationships.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_fields_in_primary_resource()
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
            ["fields[nodes]"] = "comment,parent"
        };

        // Act
        PrimaryNodeResponseDocument response = await apiClient.GetNodeAsync(node.StringId!, queryString);

        // Assert
        response.Data.Id.Should().Be(node.StringId);
        response.Data.Attributes.Should().NotBeNull();
        response.Data.Attributes.Name.Should().BeNull();
        response.Data.Attributes.Comment.Should().Be(node.Comment);
        response.Data.Relationships.Should().NotBeNull();
        response.Data.Relationships.Parent.Should().NotBeNull();
        response.Data.Relationships.Children.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_fields_in_secondary_resources()
    {
        // Arrange
        Node node = _fakers.Node.GenerateOne();
        node.Children = _fakers.Node.GenerateSet(1);

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
            ["fields[nodes]"] = "comment,children"
        };

        // Act
        NodeCollectionResponseDocument response = await apiClient.GetNodeChildrenAsync(node.StringId!, queryString);

        // Assert
        response.Data.Should().HaveCount(1);
        response.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(0).StringId);

        response.Data.ElementAt(0).Attributes.RefShould().NotBeNull().And.Subject.With(attributes =>
        {
            attributes.Name.Should().BeNull();
            attributes.Comment.Should().Be(node.Children.ElementAt(0).Comment);
        });

        response.Data.ElementAt(0).Relationships.RefShould().NotBeNull().And.Subject.With(relationships =>
        {
            relationships.Parent.Should().BeNull();
            relationships.Children.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task Can_select_fields_in_secondary_resource()
    {
        // Arrange
        Node node = _fakers.Node.GenerateOne();
        node.Parent = _fakers.Node.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Nodes.Add(node);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new QueryStringsClient(httpClient);

        var queryString = new Dictionary<string, string?>
        {
            ["fields[nodes]"] = "comment,children"
        };

        // Act
        NullableSecondaryNodeResponseDocument response = await apiClient.GetNodeParentAsync(node.StringId!, queryString);

        // Assert
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(node.Parent.StringId);
        response.Data.Attributes.Should().NotBeNull();
        response.Data.Attributes.Name.Should().BeNull();
        response.Data.Attributes.Comment.Should().Be(node.Parent.Comment);
        response.Data.Relationships.Should().NotBeNull();
        response.Data.Relationships.Parent.Should().BeNull();
        response.Data.Relationships.Children.Should().NotBeNull();
    }

    [Fact]
    public async Task Can_select_empty_fieldset()
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
            ["fields[nodes]"] = null
        };

        // Act
        PrimaryNodeResponseDocument response = await apiClient.GetNodeAsync(node.StringId!, queryString);

        // Assert
        response.Data.Id.Should().Be(node.StringId);
        response.Data.Attributes.Should().BeNull();
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
