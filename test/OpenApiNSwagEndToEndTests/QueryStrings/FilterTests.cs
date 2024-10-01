using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using OpenApiNSwagEndToEndTests.QueryStrings.GeneratedCode;
using OpenApiTests;
using OpenApiTests.QueryStrings;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.QueryStrings;

public sealed class FilterTests : IClassFixture<IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly QueryStringFakers _fakers = new();

    public FilterTests(IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<NodesController>();
    }

    [Fact]
    public async Task Can_filter_in_primary_resources()
    {
        // Arrange
        List<Node> nodes = _fakers.Node.GenerateList(2);
        nodes[0].Name = "John No Quote";
        nodes[1].Name = "Brian O'Quote";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Node>();
            dbContext.Nodes.AddRange(nodes);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new QueryStringsClient(httpClient);

        var queryString = new Dictionary<string, string?>
        {
            ["filter"] = "equals(name,'Brian O''Quote')"
        };

        // Act
        NodeCollectionResponseDocument response = await apiClient.GetNodeCollectionAsync(queryString);

        // Assert
        response.Data.ShouldHaveCount(1);
        response.Data.ElementAt(0).Id.Should().Be(nodes[1].StringId);

        response.Data.ElementAt(0).Attributes.ShouldNotBeNull().With(attributes =>
        {
            attributes.Name.Should().Be(nodes[1].Name);
            attributes.Comment.Should().Be(nodes[1].Comment);
        });

        response.Meta.ShouldNotBeNull();
        response.Meta.ShouldContainKey("total").With(total => total.Should().Be(1));
    }

    [Fact]
    public async Task Can_filter_in_secondary_resources()
    {
        // Arrange
        Node node = _fakers.Node.GenerateOne();
        node.Children = _fakers.Node.GenerateSet(2);
        node.Children.ElementAt(0).Comment = "Discount: $10";
        node.Children.ElementAt(1).Comment = "Discount: 5%";

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
            ["filter"] = "and(startsWith(comment,'Discount:'),contains(comment,'%'))"
        };

        // Act
        NodeCollectionResponseDocument response = await apiClient.GetNodeChildrenAsync(node.StringId!, queryString);

        // Assert
        response.Data.ShouldHaveCount(1);
        response.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(1).StringId);

        response.Data.ElementAt(0).Attributes.ShouldNotBeNull().With(attributes =>
        {
            attributes.Name.Should().Be(node.Children.ElementAt(1).Name);
            attributes.Comment.Should().Be(node.Children.ElementAt(1).Comment);
        });

        response.Meta.ShouldNotBeNull();
        response.Meta.ShouldContainKey("total").With(total => total.Should().Be(1));
    }

    [Fact]
    public async Task Can_filter_at_ToMany_relationship_endpoint()
    {
        // Arrange
        Node node = _fakers.Node.GenerateOne();
        node.Children = _fakers.Node.GenerateSet(2);
        node.Children.ElementAt(0).Children = _fakers.Node.GenerateSet(1);
        node.Children.ElementAt(1).Children = _fakers.Node.GenerateSet(2);

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
            ["filter"] = "greaterThan(count(children),'1')"
        };

        // Act
        NodeIdentifierCollectionResponseDocument response = await apiClient.GetNodeChildrenRelationshipAsync(node.StringId!, queryString);

        // Assert
        response.Data.ShouldHaveCount(1);
        response.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(1).StringId);
        response.Meta.ShouldNotBeNull();
        response.Meta.ShouldContainKey("total").With(total => total.Should().Be(1));
        response.Links.ShouldNotBeNull();
        response.Links.Describedby.Should().Be("/swagger/v1/swagger.json");
    }

    [Fact]
    public async Task Cannot_use_empty_filter()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new QueryStringsClient(httpClient);

        var queryString = new Dictionary<string, string?>
        {
            ["filter"] = null
        };

        // Act
        Func<Task> action = async () => _ = await apiClient.GetNodeAsync(Unknown.StringId.Int64, queryString);

        // Assert
        ApiException<ErrorResponseDocument> exception = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which;
        exception.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Message.Should().Be("HTTP 400: The query string is invalid.");
        exception.Result.Links.ShouldNotBeNull();
        exception.Result.Links.Describedby.Should().Be("/swagger/v1/swagger.json");
        exception.Result.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Result.Errors.ElementAt(0);
        error.Status.Should().Be("400");
        error.Title.Should().Be("Missing query string parameter value.");
        error.Detail.Should().Be("Missing value for 'filter' query string parameter.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
