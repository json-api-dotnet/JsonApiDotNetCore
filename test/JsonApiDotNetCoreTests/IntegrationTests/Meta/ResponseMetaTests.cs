using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta
{
    public sealed class ResponseMetaTests : IClassFixture<IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext> _testContext;

        public ResponseMetaTests(IntegrationTestContext<TestableStartup<MetaDbContext>, MetaDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<ProductFamiliesController>();
            testContext.UseController<SupportTicketsController>();

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddSingleton<IResponseMeta, SupportResponseMeta>();
            });

            var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeTotalResourceCount = false;
        }

        [Fact]
        public async Task Returns_top_level_meta()
        {
            // Arrange
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<SupportTicket>();
            });

            const string route = "/supportTickets";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/supportTickets"",
    ""first"": ""http://localhost/supportTickets""
  },
  ""data"": [],
  ""meta"": {
    ""license"": ""MIT"",
    ""projectUrl"": ""https://github.com/json-api-dotnet/JsonApiDotNetCore/"",
    ""versions"": [
      ""v4.0.0"",
      ""v3.1.0"",
      ""v2.5.2"",
      ""v1.3.1""
    ]
  }
}");
        }
    }
}
