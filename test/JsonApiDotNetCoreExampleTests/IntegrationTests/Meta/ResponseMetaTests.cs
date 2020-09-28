using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Meta
{
    public sealed class ResponseMetaTests : IClassFixture<IntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly IntegrationTestContext<Startup, AppDbContext> _testContext;

        public ResponseMetaTests(IntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddSingleton<IResponseMeta, TestResponseMeta>();
            });

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeTotalResourceCount = false;
        }

        [Fact]
        public async Task Registered_IResponseMeta_Adds_TopLevel_Meta()
        {
            // Arrange
            await _testContext.RunOnDatabaseAsync(async dbContext => { await dbContext.ClearTableAsync<Person>(); });

            var route = "/api/v1/people";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<JToken>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            var expected = @"{
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
    ""self"": ""http://localhost/api/v1/people""
  },
  ""data"": []
}";

            responseDocument.ToString().NormalizeLineEndings().Should().Be(expected.NormalizeLineEndings());
        }
    }

    public sealed class TestResponseMeta : IResponseMeta
    {
        public IReadOnlyDictionary<string, object> GetMeta()
        {
            return new Dictionary<string, object>
            {
                ["license"] = "MIT",
                ["projectUrl"] = "https://github.com/json-api-dotnet/JsonApiDotNetCore/",
                ["versions"] = new[]
                {
                    "v4.0.0",
                    "v3.1.0",
                    "v2.5.2",
                    "v1.3.1"
                }
            };
        }
    }
}
