using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings.Filtering
{
    public sealed class FilterTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
        private readonly QueryStringFakers _fakers = new QueryStringFakers();

        public FilterTests(ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.EnableLegacyFilterNotation = false;
        }

        [Fact]
        public async Task Cannot_filter_in_unknown_scope()
        {
            // Arrange
            var route = "/webAccounts?filter[doesNotExist]=equals(title,null)";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified filter is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Relationship 'doesNotExist' does not exist on resource 'webAccounts'.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("filter[doesNotExist]");
        }

        [Fact]
        public async Task Cannot_filter_in_unknown_nested_scope()
        {
            // Arrange
            var route = "/webAccounts?filter[posts.doesNotExist]=equals(title,null)";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified filter is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Relationship 'doesNotExist' in 'posts.doesNotExist' does not exist on resource 'blogPosts'.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("filter[posts.doesNotExist]");
        }

        [Fact]
        public async Task Cannot_filter_on_attribute_with_blocked_capability()
        {
            // Arrange
            var route = "/webAccounts?filter=equals(dateOfBirth,null)";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Filtering on the requested attribute is not allowed.");
            responseDocument.Errors[0].Detail.Should().Be("Filtering on attribute 'dateOfBirth' is not allowed.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("filter");
        }

        [Fact]
        public async Task Can_filter_on_ID()
        {
            // Arrange
            var accounts = _fakers.WebAccount.Generate(2);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<WebAccount>();
                dbContext.Accounts.AddRange(accounts);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/webAccounts?filter=equals(id,'{accounts[0].StringId}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(accounts[0].StringId);
            responseDocument.ManyData[0].Attributes["userName"].Should().Be(accounts[0].UserName);
        }
    }
}
