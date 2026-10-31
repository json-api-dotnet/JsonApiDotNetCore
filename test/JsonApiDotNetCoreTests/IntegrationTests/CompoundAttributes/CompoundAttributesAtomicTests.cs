using System.Net;
using System.Text.Json.Nodes;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

public sealed class CompoundAttributesAtomicTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;
    private readonly CompoundAttributesFakers _fakers = new();

    public CompoundAttributesAtomicTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
        testContext.UseController<ContactNotesController>();
        testContext.UseController<OperationsController>();

        var options = (JsonApiOptions)testContext.App.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownFieldsInRequestBody = false;
    }

    [Theory]
    [InlineData("add", "\"text\"")]
    [InlineData("update", "\"text\"")]
    [InlineData("add", "1")]
    [InlineData("update", "1")]
    [InlineData("add", "[]")]
    [InlineData("update", "[]")]
    public async Task Cannot_send_compound_as_wrong_json_kind(string op, string contentJson)
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        object requestBody = CreateOperationsBody(CreateOperation(op, existingContact, JsonNode.Parse(contentJson)));

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>("/operations", requestBody);

        // Assert
        AssertSingleError(httpResponse, responseDocument, HttpStatusCode.UnprocessableEntity,
            "Failed to deserialize request body: Incompatible attribute value found.", null, "/atomic:operations/0/data/attributes/content");
    }

    [Theory]
    [InlineData("add", "nickName", "Failed to deserialize request body: Attribute value cannot be assigned when creating resource.",
        "The attribute 'nickName' on compound type 'ContactName' cannot be assigned to.")]
    [InlineData("update", "maidenName", "Failed to deserialize request body: Attribute value cannot be assigned when updating resource.",
        "The attribute 'maidenName' on compound type 'ContactName' cannot be assigned to.")]
    [InlineData("add", "displayName", "Failed to deserialize request body: Attribute is read-only.",
        "Attribute 'displayName' on compound type 'ContactName' is read-only.")]
    [InlineData("update", "displayName", "Failed to deserialize request body: Attribute is read-only.",
        "Attribute 'displayName' on compound type 'ContactName' is read-only.")]
    public async Task Cannot_send_blocked_nested_member(string op, string member, string expectedTitle, string expectedDetail)
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        ((JsonObject)content["name"]!)[member] = "value";

        object requestBody = CreateOperationsBody(CreateOperation(op, existingContact, content));

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>("/operations", requestBody);

        // Assert
        AssertSingleError(httpResponse, responseDocument, HttpStatusCode.UnprocessableEntity, expectedTitle, expectedDetail,
            $"/atomic:operations/0/data/attributes/content/name/{member}");
    }

    [Fact]
    public async Task Can_send_nested_member_that_is_blocked_only_in_the_other_operation_kind()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        JsonObject addContent = CompoundAttributesSamples.CreateMinimalContent();
        ((JsonObject)addContent["name"]!)["maidenName"] = "Byron";

        JsonObject updateContent = CompoundAttributesSamples.CreateMinimalContent();
        ((JsonObject)updateContent["name"]!)["nickName"] = "Countess";

        object requestBody =
            CreateOperationsBody(CreateOperation("add", existingContact, addContent), CreateOperation("update", existingContact, updateContent));

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>("/operations", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.Should().HaveCount(2);
    }

    [Fact]
    public async Task Can_set_nullable_compound_to_null_on_update()
    {
        // Arrange
        AddressBook existingAddressBook = _fakers.AddressBook.Generate();
        await SeedAsync(existingAddressBook);

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "addressBooks",
                        id = existingAddressBook.StringId,
                        attributes = new
                        {
                            emergencyContact = (object?)null
                        }
                    }
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>("/operations", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            AddressBook addressBookInDatabase = await dbContext.AddressBooks.FirstWithIdAsync(existingAddressBook.Id);

            addressBookInDatabase.EmergencyContact.Should().BeNull();
        });
    }

    [Fact]
    public async Task Cannot_set_non_nullable_compound_to_null_on_update()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        object requestBody = CreateOperationsBody(CreateOperation("update", existingContact, null));

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>("/operations", requestBody);

        // Assert
        AssertSingleError(httpResponse, responseDocument, HttpStatusCode.UnprocessableEntity, "Input validation failed.", "The Content field is required.",
            "/atomic:operations/0/data/attributes/content");
    }

    [Fact]
    public async Task Cannot_update_with_invalid_element_through_IValidatableObject_in_compound_collection()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        JsonObject content = CompoundAttributesSamples.CreateIdenticalPatchContent();

        content["addresses"] = JsonNode.Parse("""
            [
              { "isoCountryCode": "NL", "city": "Utrecht" },
              { "isoCountryCode": "GR", "city": "Atlantis" }
            ]
            """);

        object requestBody = CreateOperationsBody(CreateOperation("update", existingContact, content));

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>("/operations", requestBody);

        // Assert
        AssertSingleError(httpResponse, responseDocument, HttpStatusCode.UnprocessableEntity, "Input validation failed.", "The city does not exist.",
            "/atomic:operations/0/data/attributes/content/addresses/1/city");
    }

    [Fact]
    public async Task Cannot_update_with_too_many_elements_in_scalar_collection_inside_compound()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        JsonObject content = CompoundAttributesSamples.CreateIdenticalPatchContent();
        content["company"]!["departments"] = new JsonArray("1", "2", "3", "4");

        object requestBody = CreateOperationsBody(CreateOperation("update", existingContact, content));

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>("/operations", requestBody);

        // Assert
        AssertSingleError(httpResponse, responseDocument, HttpStatusCode.UnprocessableEntity, "Input validation failed.",
            "The field Departments must be a string or array type with a maximum length of '3'.",
            "/atomic:operations/0/data/attributes/content/company/departments");
    }

    [Fact]
    public async Task Can_roll_back_compound_changes_of_earlier_operations_when_later_operation_fails()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        JsonObject newContent = CompoundAttributesSamples.CreateMinimalContent();

        JsonObject changedContent = CompoundAttributesSamples.CreateMinimalContent();
        ((JsonObject)changedContent["name"]!)["lastName"] = "Changed";

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "contacts",
                        lid = "new-contact",
                        attributes = new
                        {
                            content = newContent
                        }
                    }
                },
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "contacts",
                        id = existingContact.StringId,
                        attributes = new
                        {
                            content = changedContent
                        }
                    }
                },
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "contacts",
                        id = Unknown.StringId.Int64,
                        attributes = new
                        {
                            content = changedContent
                        }
                    }
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>("/operations", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);
        ErrorObject error = responseDocument.Errors[0];
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations/2");

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<Contact> contactsInDatabase = await dbContext.Contacts.ToListAsync();

            contactsInDatabase.Should().HaveCount(1);
            contactsInDatabase[0].Id.Should().Be(existingContact.Id);
            contactsInDatabase[0].Content.Name.LastName.Should().Be("Lovelace");
        });
    }

    private static object CreateOperationsBody(params object[] operations)
    {
        return new
        {
            atomic__operations = operations
        };
    }

    private static object CreateOperation(string op, Contact existingContact, JsonNode? content)
    {
        if (op == "add")
        {
            return new
            {
                op,
                data = new
                {
                    type = "contacts",
                    attributes = new
                    {
                        content
                    }
                }
            };
        }

        return new
        {
            op,
            data = new
            {
                type = "contacts",
                id = existingContact.StringId,
                attributes = new
                {
                    content
                }
            }
        };
    }

    private async Task SeedAsync(Contact contact)
    {
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });
    }

    private async Task SeedAsync(AddressBook addressBook)
    {
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(addressBook);
            await dbContext.SaveChangesAsync();
        });
    }

    private static void AssertSingleError(HttpResponseMessage httpResponse, Document responseDocument, HttpStatusCode expectedStatusCode, string expectedTitle,
        string? expectedDetail, string expectedPointer)
    {
        httpResponse.ShouldHaveStatusCode(expectedStatusCode);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(expectedStatusCode);
        error.Title.Should().Be(expectedTitle);

        if (expectedDetail != null)
        {
            error.Detail.Should().Be(expectedDetail);
        }

        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be(expectedPointer);
    }
}
