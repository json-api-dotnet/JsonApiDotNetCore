using System.Net;
using System.Text.Json.Nodes;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

public sealed class CompoundAttributesModelStateTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;

    public CompoundAttributesModelStateTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
        testContext.UseController<ContactNotesController>();
    }

    [Fact]
    public async Task Cannot_create_with_missing_required_member()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["birthDate"] = JsonNode.Parse("""{ "year": 1990, "day": 15 }""");

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await PostAsync(content);

        // Assert
        AssertSingleModelError(httpResponse, responseDocument, "The Month field is required.", "/data/attributes/content/birthDate/month");
    }

    [Fact]
    public async Task Cannot_create_with_missing_implicitly_required_member()
    {
        // Arrange
        var content = new JsonObject
        {
            ["name"] = new JsonObject
            {
                ["firstName"] = "John"
            }
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await PostAsync(content);

        // Assert
        AssertSingleModelError(httpResponse, responseDocument, "The LastName field is required.", "/data/attributes/content/name/lastName");
    }

    [Fact]
    public async Task Cannot_create_with_out_of_range_member()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["birthDate"] = JsonNode.Parse("""{ "year": 1700, "month": 1, "day": 15 }""");

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await PostAsync(content);

        // Assert
        AssertSingleModelError(httpResponse, responseDocument, "The field Year must be between 1800 and 2500.", "/data/attributes/content/birthDate/year");
    }

    [Fact]
    public async Task Cannot_create_with_invalid_collection_element()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["phoneNumbers"] = JsonNode.Parse("""[ { "number": "1" }, { "number": "" } ]""");

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await PostAsync(content);

        // Assert
        AssertSingleModelError(httpResponse, responseDocument, "The Number field is required.", "/data/attributes/content/phoneNumbers/1/number");
    }

    [Fact]
    public async Task PATCH_validates_the_whole_replaced_compound()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        // The client "only wants to change the phone numbers", but because the whole compound is replaced, all nested rules apply.
        var content = new JsonObject
        {
            ["phoneNumbers"] = JsonNode.Parse("""[ { "number": "1" } ]""")
        };

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePatchAsync<Document>($"/contacts/{existingContact.StringId}", requestBody);

        // Assert
        AssertSingleModelError(httpResponse, responseDocument, "The Name field is required.", "/data/attributes/content/name");
    }

    [Fact]
    public async Task PATCH_does_not_validate_untargeted_compound()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        existingContact.Content.PhoneNumbers[0].Number = string.Empty;
        await SeedAsync(existingContact);

        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                id = existingContact.StringId,
                attributes = new
                {
                    remarks = "Changed"
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) =
            await _testContext.ExecutePatchAsync<string>($"/contacts/{existingContact.StringId}", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Cannot_set_non_nullable_compound_to_null()
    {
        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await PostAsync(null);

        // Assert
        AssertSingleModelError(httpResponse, responseDocument, "The Content field is required.", "/data/attributes/content");
    }

    [Fact]
    public async Task Cannot_set_non_nullable_nested_compound_to_null()
    {
        // Arrange
        var content = new JsonObject
        {
            ["name"] = null
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await PostAsync(content);

        // Assert
        AssertSingleModelError(httpResponse, responseDocument, "The Name field is required.", "/data/attributes/content/name");
    }

    [Fact]
    public async Task Cannot_set_non_nullable_nested_collection_to_null()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["phoneNumbers"] = null;

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await PostAsync(content);

        // Assert
        AssertSingleModelError(httpResponse, responseDocument, "The PhoneNumbers field is required.", "/data/attributes/content/phoneNumbers");
    }

    [Fact]
    public async Task Reports_multiple_errors_with_stable_order_and_limit()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["birthDate"] = JsonNode.Parse("""{ "year": 1700 }""");
        content["phoneNumbers"] = JsonNode.Parse("""[ { "number": "" } ]""");

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await PostAsync(content);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        // The test startup sets MaxModelValidationErrors to 3. MVC then reports a "too many errors" entry (first, without pointer) plus the first two validation errors in a deterministic order.
        responseDocument.Errors.Should().HaveCount(3);

        responseDocument.Errors.Select(error => error.Source?.Pointer).Should().Equal(null, "/data/attributes/content/birthDate/year",
            "/data/attributes/content/phoneNumbers/0/number");
    }

    [Fact]
    public async Task Can_create_valid_resource()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreatePostContent();

        // Act
        (HttpResponseMessage httpResponse, Document _) = await PostAsync(content);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Cannot_create_with_nonexistent_day_in_month()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["birthDate"] = JsonNode.Parse("""{ "year": 2023, "month": 2, "day": 30 }""");

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await PostAsync(content);

        // Assert
        AssertSingleModelError(httpResponse, responseDocument, "The day does not exist in the specified month.", "/data/attributes/content/birthDate/day");
    }

    [Fact]
    public async Task Cannot_create_with_too_long_nested_string()
    {
        // Arrange
        var content = new JsonObject
        {
            ["name"] = new JsonObject
            {
                ["firstName"] = new string('x', 21),
                ["lastName"] = "Doe"
            }
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await PostAsync(content);

        // Assert
        AssertSingleModelError(httpResponse, responseDocument, "The field FirstName must be a string or array type with a maximum length of '20'.",
            "/data/attributes/content/name/firstName");
    }

    [Fact]
    public async Task Cannot_create_with_too_many_scalar_collection_elements()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "addressBooks",
                attributes = new
                {
                    name = "Book",
                    categories = new[]
                    {
                        "1",
                        "2",
                        "3",
                        "4",
                        "5",
                        "6"
                    }
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/addressBooks", requestBody);

        // Assert
        AssertSingleModelError(httpResponse, responseDocument, "The field Categories must be a string or array type with a maximum length of '5'.",
            "/data/attributes/categories");
    }

    [Fact]
    public async Task Validates_struct_compound()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                attributes = new
                {
                    content = CompoundAttributesSamples.CreateMinimalContent(),
                    lastKnownLocation = new
                    {
                        latitude = 1.0
                    }
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse, Document _) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);
    }

    private async Task<(HttpResponseMessage, Document)> PostAsync(JsonNode? content)
    {
        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);
        return await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);
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

    private static void AssertSingleModelError(HttpResponseMessage httpResponse, Document responseDocument, string expectedDetail, string expectedPointer)
    {
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be(expectedDetail);
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be(expectedPointer);
    }
}
