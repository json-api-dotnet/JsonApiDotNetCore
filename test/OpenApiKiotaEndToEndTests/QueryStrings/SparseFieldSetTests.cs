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

public sealed class SparseFieldSetTests : IClassFixture<IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly QueryStringFakers _fakers = new();

    public SparseFieldSetTests(IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["fields[nodes]"] = "name"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodeCollectionResponseDocument? response = await apiClient.Nodes.GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldHaveCount(1);
            response.Data.ElementAt(0).Id.Should().Be(node.StringId);

            response.Data.ElementAt(0).Attributes.ShouldNotBeNull().With(attributes =>
            {
                attributes.Name.Should().Be(node.Name);
                attributes.Comment.Should().BeNull();
            });

            response.Data.ElementAt(0).Relationships.Should().BeNull();
        }
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["fields[nodes]"] = "comment,parent"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodePrimaryResponseDocument? response = await apiClient.Nodes[node.StringId].GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Id.Should().Be(node.StringId);
            response.Data.Attributes.ShouldNotBeNull();
            response.Data.Attributes.Name.Should().BeNull();
            response.Data.Attributes.Comment.Should().Be(node.Comment);
            response.Data.Relationships.ShouldNotBeNull();
            response.Data.Relationships.Parent.ShouldNotBeNull();
            response.Data.Relationships.Children.Should().BeNull();
        }
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["fields[nodes]"] = "comment,children"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodeCollectionResponseDocument? response = await apiClient.Nodes[node.StringId].Children.GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldHaveCount(1);
            response.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(0).StringId);

            response.Data.ElementAt(0).Attributes.ShouldNotBeNull().With(attributes =>
            {
                attributes.Name.Should().BeNull();
                attributes.Comment.Should().Be(node.Children.ElementAt(0).Comment);
            });

            response.Data.ElementAt(0).Relationships.ShouldNotBeNull().With(relationships =>
            {
                relationships.Parent.Should().BeNull();
                relationships.Children.ShouldNotBeNull();
            });
        }
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["fields[nodes]"] = "comment,children"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NullableNodeSecondaryResponseDocument? response = await apiClient.Nodes[node.StringId].Parent.GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Id.Should().Be(node.Parent.StringId);
            response.Data.Attributes.ShouldNotBeNull();
            response.Data.Attributes.Name.Should().BeNull();
            response.Data.Attributes.Comment.Should().Be(node.Parent.Comment);
            response.Data.Relationships.ShouldNotBeNull();
            response.Data.Relationships.Parent.Should().BeNull();
            response.Data.Relationships.Children.ShouldNotBeNull();
        }
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["fields[nodes]"] = null
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodePrimaryResponseDocument? response = await apiClient.Nodes[node.StringId].GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Id.Should().Be(node.StringId);
            response.Data.Attributes.Should().BeNull();
        }
    }
}
