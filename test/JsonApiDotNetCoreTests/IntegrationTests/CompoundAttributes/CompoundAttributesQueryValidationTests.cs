using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

public sealed class CompoundAttributesQueryValidationTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;
    private readonly CompoundAttributesFakers _fakers = new();

    public CompoundAttributesQueryValidationTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
        testContext.UseController<ContactNotesController>();
    }

    [Fact]
    public async Task Cannot_sort_on_compound_object()
    {
        // Arrange
        const string route = "/contacts?sort=content";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Contain("zero or more to-one relationships or compound non-collection attributes, followed by a scalar non-collection attribute");
    }

    [Fact]
    public async Task Cannot_sort_on_collection_of_compound_attributes()
    {
        // Arrange
        const string route = "/contacts?sort=content.phoneNumbers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Contain("zero or more to-one relationships or compound non-collection attributes, followed by a scalar non-collection attribute");
    }

    [Fact]
    public async Task Cannot_compare_compound_object_directly_to_scalar_constant()
    {
        // Arrange
        const string route = "/contacts?filter=equals(content,'test')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Contain("null expected");
    }

    [Fact]
    public async Task Cannot_navigate_directly_into_collection_without_has()
    {
        // Arrange
        const string route = "/contacts?filter=equals(content.phoneNumbers.number,'123')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");

        error.Detail.Should().Contain(
            "zero or more to-one relationships or compound non-collection attributes, followed by a to-one relationship or an attribute");
    }

    [Fact]
    public async Task Cannot_filter_non_collection_with_has()
    {
        // Arrange
        const string route = "/contacts?filter=has(content)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");

        error.Detail.Should().Contain(
            "zero or more to-one relationships or compound non-collection attributes, followed by a to-many relationship or a collection attribute");
    }

    [Fact]
    public async Task Cannot_use_count_on_non_collection()
    {
        // Arrange
        const string route = "/contacts?filter=equals(count(content),1)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");

        error.Detail.Should().Contain(
            "zero or more to-one relationships or compound non-collection attributes, followed by a to-many relationship or a collection attribute");
    }

    [Fact]
    public async Task Cannot_use_isType_inside_compound_attribute()
    {
        // Arrange
        const string route = "/contacts?filter=has(content.phoneNumbers,isType(null,contacts))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Contain("Function 'isType' can only be used on resource types");
    }

    [Fact]
    public async Task Cannot_use_underscore_on_compound_collection()
    {
        // Arrange
        const string route = "/contacts?filter=has(content.phoneNumbers,startsWith(_.countryPrefix,'%2B31'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be("Function or field name expected. Failed at position 37: has(content.phoneNumbers,startsWith(^_.countryPrefix,'+31'))");
    }
}
