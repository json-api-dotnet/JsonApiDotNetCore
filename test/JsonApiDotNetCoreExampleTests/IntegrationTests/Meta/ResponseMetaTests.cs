using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Meta
{
    public sealed class ResponseMetaTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<SupportDbContext>, SupportDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<SupportDbContext>, SupportDbContext> _testContext;

        public ResponseMetaTests(ExampleIntegrationTestContext<TestableStartup<SupportDbContext>, SupportDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddSingleton<IResponseMeta, SupportResponseMeta>();
            });

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
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

            var route = "/supportTickets";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().BeJson(@"{
  ""meta"": {
    ""license"": ""MIT"",
    ""projectUrl"": ""https://github.com/json-api-dotnet/JsonApiDotNetCore/"",
    ""versions"": [
      ""v4.0.0"",
      ""v3.1.0"",
      ""v2.5.2"",
      ""v1.3.1""
    ]
  },
  ""links"": {
    ""self"": ""http://localhost/supportTickets"",
    ""first"": ""http://localhost/supportTickets""
  },
  ""data"": []
}");
        }
    }
}
