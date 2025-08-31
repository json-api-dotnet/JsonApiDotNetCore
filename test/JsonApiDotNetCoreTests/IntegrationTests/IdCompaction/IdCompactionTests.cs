using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdCompaction;

public sealed class IdCompationTests : IClassFixture<IntegrationTestContext<IdCompactionStartup, IdCompactionDbContext>>
{
    private readonly IntegrationTestContext<IdCompactionStartup, IdCompactionDbContext> _testContext;
    private readonly IdCompationFakers _fakers = new();

    public IdCompationTests(IntegrationTestContext<IdCompactionStartup, IdCompactionDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<GrantsController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.UseRelativeLinks = true;
        options.ClientIdGeneration = ClientIdGenerationMode.Forbidden;
    }

    [Fact]
    public async Task Can_create_resource()
    {
        // Arrange
        Grant grant = _fakers.Grants.GenerateOne();

        var requestBody = new
        {
            data = new
            {
                type = "grants",
                attributes = new
                {
                    name = grant.Name
                }
            }
        };

        const string route = "/grants";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);
    }
}
