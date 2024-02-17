using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client;
using Microsoft.Net.Http.Headers;
using OpenApiEndToEndTests.QueryStrings.GeneratedCode;
using OpenApiTests;
using OpenApiTests.QueryStrings;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiEndToEndTests.QueryStrings;

public sealed class ETagTests : IClassFixture<IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext> _testContext;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly QueryStringFakers _fakers = new();

    public ETagTests(IntegrationTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _testOutputHelper = testOutputHelper;

        testContext.UseController<NodesController>();
    }

    [Fact]
    public async Task Read_Read_Write_Read_Scenario()
    {
        // Arrange
        Node node = _fakers.Node.Generate();
        node.Name = "Bob l'éponge";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Node>();
            dbContext.Nodes.Add(node);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new QueryStringsClient(httpClient, _testOutputHelper);

        // Act & Assert
        ApiResponse<NodePrimaryResponseDocument?> response1 = await ApiResponse.TranslateAsync(() => apiClient.GetNodeAsync(node.StringId!));
        response1.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response1.Headers.Should().ContainKey(HeaderNames.ETag);
        string response1Etag = response1.Headers[HeaderNames.ETag].First();

        ApiResponse<NodePrimaryResponseDocument?> response2 =
            await ApiResponse.TranslateAsync(() => apiClient.GetNodeAsync(node.StringId!, if_None_Match: response1Etag));

        response2.StatusCode.Should().Be((int)HttpStatusCode.NotModified);
        response2.Headers.Should().ContainKey(HeaderNames.ETag).WhoseValue.Should().Equal([response1Etag]);

        ApiResponse<NodePrimaryResponseDocument?> response3 = await ApiResponse.TranslateAsync(() => apiClient.PatchNodeAsync(node.StringId!,
            body: new NodePatchRequestDocument
            {
                Data = new NodeDataInPatchRequest
                {
                    Id = node.StringId!,
                    Attributes = new NodeAttributesInPatchRequest
                    {
                        Name = "Yann Le Gac"
                    }
                }
            }));

        response3.StatusCode.Should().Be((int)HttpStatusCode.NoContent);

        ApiResponse<NodePrimaryResponseDocument?> response4 = await ApiResponse.TranslateAsync(() => apiClient.GetNodeAsync(node.StringId!));
        response4.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response4.Headers.Should().ContainKey(HeaderNames.ETag).WhoseValue.Should().NotEqual([response1Etag]);
    }
}
