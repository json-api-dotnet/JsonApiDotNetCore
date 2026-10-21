using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

public sealed class CompoundAttributesValidationTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;
    private readonly CompoundAttributesFakers _fakers = new();

    public CompoundAttributesValidationTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
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
            "zero or more to-one relationships or compound non-collection attributes, followed by a to-one relationship or a non-collection attribute");
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
        error.Detail.Should().Contain("Function 'isType' cannot be used on compound types");
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

    [Fact]
    public async Task Cannot_create_contact_with_missing_required_compound_property()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                attributes = new
                {
                    content = new
                    {
                        name = new
                        {
                            firstName = "John",
                            lastName = "Doe"
                        },
                        birthDate = new
                        {
                            year = 1990,
                            day = 15
                        }
                    }
                }
            }
        };

        const string route = "/contacts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/content/birthDate/month");
    }

    [Fact]
    public async Task Cannot_create_contact_with_out_of_range_compound_property()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                attributes = new
                {
                    content = new
                    {
                        name = new
                        {
                            firstName = "John",
                            lastName = "Doe"
                        },
                        birthDate = new
                        {
                            year = 1990,
                            month = 15,
                            day = 15
                        }
                    }
                }
            }
        };

        const string route = "/contacts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/content/birthDate/month");
    }

    [Fact]
    public async Task Cannot_create_contact_with_empty_required_collection_element_property()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                attributes = new
                {
                    content = new
                    {
                        name = new
                        {
                            firstName = "John",
                            lastName = "Doe"
                        },
                        phoneNumbers = new[]
                        {
                            new
                            {
                                number = string.Empty,
                                label = "work"
                            }
                        }
                    }
                }
            }
        };

        const string route = "/contacts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/content/phoneNumbers/0/number");
    }

    [Fact]
    public async Task Cannot_create_contact_with_blocked_AllowCreate_compound_property()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                attributes = new
                {
                    content = new
                    {
                        name = new
                        {
                            firstName = "John",
                            lastName = "Doe",
                            nickName = "Johnny"
                        }
                    }
                }
            }
        };

        const string route = "/contacts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Attribute value cannot be assigned when creating resource.");
        error.Detail.Should().Be("The attribute 'nickName' on compound type 'ContactName' cannot be assigned to.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/content/name/nickName");
    }

    [Fact]
    public async Task Cannot_update_contact_with_blocked_AllowChange_compound_property()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                id = contact.StringId,
                attributes = new
                {
                    content = new
                    {
                        name = new
                        {
                            lastName = contact.Content.Name.LastName,
                            maidenName = "NewMaidenName"
                        }
                    }
                }
            }
        };

        string route = $"/contacts/{contact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Attribute value cannot be assigned when updating resource.");
        error.Detail.Should().Be("The attribute 'maidenName' on compound type 'ContactName' cannot be assigned to.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/content/name/maidenName");
    }

    [Fact]
    public async Task Cannot_set_read_only_compound_property_on_create()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                attributes = new
                {
                    content = new
                    {
                        name = new
                        {
                            lastName = "Doe",
                            displayName = "John Doe"
                        }
                    }
                }
            }
        };

        const string route = "/contacts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Attribute is read-only.");
        error.Detail.Should().Be("Attribute 'displayName' on compound type 'ContactName' is read-only.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/content/name/displayName");
    }

    [Fact]
    public async Task Cannot_set_read_only_compound_property_on_update()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                id = contact.StringId,
                attributes = new
                {
                    content = new
                    {
                        name = new
                        {
                            lastName = contact.Content.Name.LastName,
                            displayName = "Changed Display Name"
                        }
                    }
                }
            }
        };

        string route = $"/contacts/{contact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Attribute is read-only.");
        error.Detail.Should().Be("Attribute 'displayName' on compound type 'ContactName' is read-only.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/content/name/displayName");
    }

    [Fact]
    public async Task Cannot_send_unknown_compound_property_when_disallowed()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                attributes = new
                {
                    content = new
                    {
                        name = new
                        {
                            lastName = "Doe",
                            extraProp = "Unexpected"
                        }
                    }
                }
            }
        };

        const string route = "/contacts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unknown attribute found.");
        error.Detail.Should().Be("Attribute 'extraProp' does not exist on compound type 'ContactName'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/content/name/extraProp");
    }
}
