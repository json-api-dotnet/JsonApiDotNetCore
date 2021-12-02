using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering
{
    public sealed class FilterTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
        private readonly QueryStringFakers _fakers = new();

        public FilterTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<WebAccountsController>();

            var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.EnableLegacyFilterNotation = false;
        }

        [Fact]
        public async Task Cannot_filter_in_unknown_scope()
        {
            // Arrange
            const string route = $"/webAccounts?filter[{Unknown.Relationship}]=equals(title,null)";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("The specified filter is invalid.");
            error.Detail.Should().Be($"Relationship '{Unknown.Relationship}' does not exist on resource type 'webAccounts'.");
            error.Source.ShouldNotBeNull();
            error.Source.Parameter.Should().Be($"filter[{Unknown.Relationship}]");
        }

        [Fact]
        public async Task Cannot_filter_in_unknown_nested_scope()
        {
            // Arrange
            const string route = $"/webAccounts?filter[posts.{Unknown.Relationship}]=equals(title,null)";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("The specified filter is invalid.");
            error.Detail.Should().Be($"Relationship '{Unknown.Relationship}' in 'posts.{Unknown.Relationship}' does not exist on resource type 'blogPosts'.");
            error.Source.ShouldNotBeNull();
            error.Source.Parameter.Should().Be($"filter[posts.{Unknown.Relationship}]");
        }

        [Fact]
        public async Task Cannot_filter_on_attribute_with_blocked_capability()
        {
            // Arrange
            const string route = "/webAccounts?filter=equals(dateOfBirth,null)";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Filtering on the requested attribute is not allowed.");
            error.Detail.Should().Be("Filtering on attribute 'dateOfBirth' is not allowed.");
            error.Source.ShouldNotBeNull();
            error.Source.Parameter.Should().Be("filter");
        }

        [Fact]
        public async Task Can_filter_on_ID()
        {
            // Arrange
            List<WebAccount> accounts = _fakers.WebAccount.Generate(2);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<WebAccount>();
                dbContext.Accounts.AddRange(accounts);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/webAccounts?filter=equals(id,'{accounts[0].StringId}')";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.ShouldHaveCount(1);
            responseDocument.Data.ManyValue[0].Id.Should().Be(accounts[0].StringId);
            responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("userName").With(value => value.Should().Be(accounts[0].UserName));
        }
    }
}
