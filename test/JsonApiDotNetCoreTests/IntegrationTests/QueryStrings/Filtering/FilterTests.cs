using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;

public sealed class FilterTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public FilterTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WebAccountsController>();
        testContext.UseController<CalendarsController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.EnableLegacyFilterNotation = false;
    }

    [Fact]
    public async Task Cannot_filter_in_unknown_scope()
    {
        // Arrange
        var parameterName = new MarkedText($"filter[^{Unknown.Relationship}]", '^');
        string route = $"/webAccounts?{parameterName.Text}=equals(title,null)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"Field '{Unknown.Relationship}' does not exist on resource type 'webAccounts'. {parameterName}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName.Text);
    }

    [Fact]
    public async Task Cannot_filter_in_unknown_nested_scope()
    {
        // Arrange
        var parameterName = new MarkedText($"filter[posts.^{Unknown.Relationship}]", '^');
        string route = $"/webAccounts?{parameterName.Text}=equals(title,null)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"Field '{Unknown.Relationship}' does not exist on resource type 'blogPosts'. {parameterName}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName.Text);
    }

    [Fact]
    public async Task Cannot_filter_on_attribute_with_blocked_capability()
    {
        // Arrange
        var parameterValue = new MarkedText("equals(^dateOfBirth,null)", '^');
        string route = $"/webAccounts?filter={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"Filtering on attribute 'dateOfBirth' is not allowed. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    [Fact]
    public async Task Cannot_filter_on_ToMany_relationship_with_blocked_capability()
    {
        // Arrange
        var parameterValue = new MarkedText("has(^appointments)", '^');
        string route = $"/calendars?filter={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"Filtering on relationship 'appointments' is not allowed. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    [Fact]
    public async Task Can_filter_on_ID()
    {
        // Arrange
        List<WebAccount> accounts = _fakers.WebAccount.GenerateList(2);

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(accounts[0].StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("userName").WhoseValue.Should().Be(accounts[0].UserName);
    }
}
