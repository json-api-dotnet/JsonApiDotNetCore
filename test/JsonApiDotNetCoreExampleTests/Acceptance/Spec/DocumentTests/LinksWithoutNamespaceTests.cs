using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec.DocumentTests
{
    public sealed class LinksWithoutNamespaceTests : IClassFixture<IntegrationTestContext<NoNamespaceStartup, AppDbContext>>
    {
        private readonly IntegrationTestContext<NoNamespaceStartup, AppDbContext> _testContext;

        public LinksWithoutNamespaceTests(IntegrationTestContext<NoNamespaceStartup, AppDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task GET_RelativeLinks_True_Without_Namespace_Returns_RelativeLinks()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.UseRelativeLinks = true;

            var person = new Person();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.People.Add(person);

                await dbContext.SaveChangesAsync();
            });

            var route = "/people/" + person.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be("/people/" + person.StringId);
        }

        [Fact]
        public async Task GET_RelativeLinks_False_Without_Namespace_Returns_AbsoluteLinks()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.UseRelativeLinks = false;

            var person = new Person();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.People.Add(person);

                await dbContext.SaveChangesAsync();
            });

            var route = "/people/" + person.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be("http://localhost/people/" + person.StringId);
        }
    }

    public sealed class NoNamespaceStartup : TestStartup
    {
        public NoNamespaceStartup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void ConfigureJsonApiOptions(JsonApiOptions options)
        {
            base.ConfigureJsonApiOptions(options);

            options.Namespace = null;
        }
    }
}
