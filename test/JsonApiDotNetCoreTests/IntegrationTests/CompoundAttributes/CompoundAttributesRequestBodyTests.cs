using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

public sealed class CompoundAttributesRequestBodyTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;
    private readonly CompoundAttributesFakers _fakers = new();

    public CompoundAttributesRequestBodyTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
        testContext.UseController<ContactNotesController>();

        var options = (JsonApiOptions)testContext.App.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownFieldsInRequestBody = false;
    }

    [Theory]
    [InlineData("\"text\"")]
    [InlineData("1")]
    [InlineData("true")]
    [InlineData("[]")]
    public async Task Cannot_send_compound_as_non_object(string contentJson)
    {
        // Arrange
        object requestBody = CompoundAttributesSamples.CreateContactPostBody(JsonNode.Parse(contentJson));

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        AssertSingleError(httpResponse, responseDocument, "Failed to deserialize request body: Incompatible attribute value found.",
            "/data/attributes/content");
    }

    [Fact]
    public async Task Cannot_send_nested_compound_as_non_object()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["name"] = new JsonArray();

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        AssertSingleError(httpResponse, responseDocument, "Failed to deserialize request body: Incompatible attribute value found.",
            "/data/attributes/content/name");
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("\"x\"")]
    public async Task Cannot_send_compound_collection_as_object_or_string(string phoneNumbersJson)
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["phoneNumbers"] = JsonNode.Parse(phoneNumbersJson);

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        AssertSingleError(httpResponse, responseDocument, "Failed to deserialize request body: Incompatible attribute value found.",
            "/data/attributes/content/phoneNumbers");
    }

    [Fact]
    public async Task Cannot_send_non_object_collection_element()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["phoneNumbers"] = new JsonArray(1);

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        AssertSingleError(httpResponse, responseDocument, "Failed to deserialize request body: Incompatible attribute value found.",
            "/data/attributes/content/phoneNumbers/0");
    }

    [Fact]
    public async Task Cannot_send_scalar_collection_as_non_array()
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
                    syncDeviceIds = "x"
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/addressBooks", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
        responseDocument.Errors[0].Source.Should().NotBeNull();
        responseDocument.Errors[0].Source!.Pointer.Should().Be("/data/attributes/syncDeviceIds");
    }

    [Theory]
    [InlineData("""{ "syncDeviceIds": [ "x" ] }""", "/data/attributes/syncDeviceIds")]
    [InlineData("""{ "emergencyContact": { "name": { "lastName": "Doe" }, "ratings": [ 1.5 ] } }""", "/data/attributes/emergencyContact/ratings")]
    [InlineData("""{ "emergencyContact": { "name": { "lastName": "Doe" }, "company": { "name": "X", "businessHours": [ "x" ] } } }""",
        "/data/attributes/emergencyContact/company/businessHours")]
    public async Task Cannot_send_wrong_scalar_collection_element(string attributesJson, string expectedPointer)
    {
        // Arrange
        JsonObject attributes = CompoundAttributesSamples.ParseObject(attributesJson);
        attributes["name"] = "Book";

        var requestBody = new
        {
            data = new
            {
                type = "addressBooks",
                attributes
            }
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/addressBooks", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
        responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Incompatible attribute value found.");
        responseDocument.Errors[0].Source.Should().NotBeNull();
        responseDocument.Errors[0].Source!.Pointer.Should().Be(expectedPointer);
    }

    [Theory]
    [InlineData("""{ "year": "1990", "month": 1, "day": 1 }""", "/data/attributes/content/birthDate/year")]
    [InlineData("""{ "month": 300, "day": 1 }""", "/data/attributes/content/birthDate/month")]
    public async Task Cannot_send_wrong_scalar_child_value(string birthDateJson, string expectedPointer)
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["birthDate"] = JsonNode.Parse(birthDateJson);

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
        responseDocument.Errors[0].Source.Should().NotBeNull();
        responseDocument.Errors[0].Source!.Pointer.Should().Be(expectedPointer);
    }

    [Fact]
    public async Task Cannot_send_null_for_non_nullable_value_type_child()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["importantDates"] = JsonNode.Parse("""[ { "value": null, "label": "x" } ]""");

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
        responseDocument.Errors[0].Source.Should().NotBeNull();
        responseDocument.Errors[0].Source!.Pointer.Should().Be("/data/attributes/content/importantDates/0/value");
    }

    [Fact]
    public async Task Cannot_send_array_for_scalar_child()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["phoneNumbers"] = JsonNode.Parse("""[ { "number": "1", "label": [ "a" ] } ]""");

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
        responseDocument.Errors[0].Source.Should().NotBeNull();
        responseDocument.Errors[0].Source!.Pointer.Should().Be("/data/attributes/content/phoneNumbers/0/label");
    }

    [Theory]
    [InlineData(false, "content", "ContactRoot", "/data/attributes/content/nope")]
    [InlineData(false, "name", "ContactName", "/data/attributes/content/name/nope")]
    [InlineData(false, "phoneNumbers", "ContactPhoneNumber", "/data/attributes/content/phoneNumbers/0/nope")]
    [InlineData(false, "company", "ContactCompany", "/data/attributes/content/company/nope")]
    [InlineData(true, "content", "ContactRoot", "/data/attributes/content/nope")]
    [InlineData(true, "name", "ContactName", "/data/attributes/content/name/nope")]
    [InlineData(true, "phoneNumbers", "ContactPhoneNumber", "/data/attributes/content/phoneNumbers/0/nope")]
    [InlineData(true, "company", "ContactCompany", "/data/attributes/content/company/nope")]
    public async Task Cannot_send_unknown_nested_member(bool isPatch, string depth, string compoundTypeName, string expectedPointer)
    {
        // Arrange
        JsonObject content = CreateContentWithUnknownMember(depth);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await SendContentAsync(isPatch, content);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unknown attribute found.");
        error.Detail.Should().Be($"Attribute 'nope' does not exist on compound type '{compoundTypeName}'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be(expectedPointer);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Can_send_null_element_in_scalar_collection_of_nullable_elements(bool isPatch)
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["ratings"] = new JsonArray(5, null);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await SendContentAsync(isPatch, content);

        // Assert
        if (isPatch)
        {
            httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
            return;
        }

        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        JsonNode? ratings = ((JsonObject)responseDocument.Data.SingleValue.GetAttribute("content")!)["ratings"];
        ratings.Should().BeOfType<JsonArray>().Which.Select(node => node?.GetValue<int>()).Should().Equal(5, null);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cannot_send_null_element_in_scalar_collection_of_non_nullable_elements(bool isPatch)
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["creditLimits"] = new JsonArray(1, null);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await SendContentAsync(isPatch, content);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
        responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Incompatible attribute value found.");
        responseDocument.Errors[0].Source.Should().NotBeNull();
        responseDocument.Errors[0].Source!.Pointer.Should().Be("/data/attributes/content/creditLimits");
    }

    [Theory(Skip = "EF Core 10: Cannot materialize a null element in a JSON complex collection (InvalidOperationException 'Invalid token type: Null').")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Can_send_null_element_in_compound_collection_of_reference_type(bool isPatch)
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["phoneNumbers"] = new JsonArray((JsonNode?)null);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await SendContentAsync(isPatch, content);

        // Assert
        httpResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        JsonNode? phoneNumbers = ((JsonObject)responseDocument.Data.SingleValue.GetAttribute("content")!)["phoneNumbers"];
        phoneNumbers.Should().BeOfType<JsonArray>().Which.Should().ContainSingle().Which.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_use_wrong_case_for_nested_member()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content["addresses"] = JsonNode.Parse("""[ { "isoCountryCode": "NL", "city": "Utrecht", "ZipCode": "1234" } ]""");

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.Title.Should().Be("Failed to deserialize request body: Unknown attribute found.");
        error.Detail.Should().Be("Attribute 'ZipCode' does not exist on compound type 'ContactAddress'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/content/addresses/0/ZipCode");
    }

    [Fact]
    public async Task Can_use_last_duplicate_key()
    {
        // Arrange
        using JsonDocument document = JsonDocument.Parse("""
            { "name": { "lastName": "First" }, "name": { "lastName": "Second" } }
            """);

        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                attributes = new
                {
                    content = document.RootElement.Clone()
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.GetAttribute("content", "name.lastName").ShouldBeJson("\"Second\"");
    }

    [Theory]
    [InlineData("""{ "name": null, "name": { "lastName": "Second" } }""")]
    [InlineData("""{ "name": 5, "name": { "lastName": "Second" } }""")]
    [InlineData("""{ "name": { "nope": 1 }, "name": { "lastName": "Second" } }""")]
    public async Task Can_use_last_duplicate_key_when_first_value_is_null_or_invalid(string contentJson)
    {
        // Arrange
        using JsonDocument document = JsonDocument.Parse(contentJson);

        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                attributes = new
                {
                    content = document.RootElement.Clone()
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.GetAttribute("content", "name.lastName").ShouldBeJson("\"Second\"");
    }

    [Fact]
    public async Task Cannot_use_last_duplicate_key_when_it_is_invalid()
    {
        // Arrange
        using JsonDocument document = JsonDocument.Parse("""
            { "name": { "lastName": "First" }, "name": 5 }
            """);

        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                attributes = new
                {
                    content = document.RootElement.Clone()
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        AssertSingleError(httpResponse, responseDocument, "Failed to deserialize request body: Incompatible attribute value found.",
            "/data/attributes/content/name");
    }

    [Fact]
    public async Task Cannot_send_blank_member_name()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        content[""] = "x";

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(false, false, "nickName", "Attribute value cannot be assigned when creating resource.",
        "The attribute 'nickName' on compound type 'ContactName' cannot be assigned to.")]
    [InlineData(true, false, "maidenName", "Attribute value cannot be assigned when updating resource.",
        "The attribute 'maidenName' on compound type 'ContactName' cannot be assigned to.")]
    [InlineData(false, false, "displayName", "Attribute is read-only.", "Attribute 'displayName' on compound type 'ContactName' is read-only.")]
    [InlineData(true, false, "displayName", "Attribute is read-only.", "Attribute 'displayName' on compound type 'ContactName' is read-only.")]
    [InlineData(false, true, "nickName", "Attribute value cannot be assigned when creating resource.",
        "The attribute 'nickName' on compound type 'ContactName' cannot be assigned to.")]
    [InlineData(true, true, "maidenName", "Attribute value cannot be assigned when updating resource.",
        "The attribute 'maidenName' on compound type 'ContactName' cannot be assigned to.")]
    [InlineData(false, true, "displayName", "Attribute is read-only.", "Attribute 'displayName' on compound type 'ContactName' is read-only.")]
    [InlineData(true, true, "displayName", "Attribute is read-only.", "Attribute 'displayName' on compound type 'ContactName' is read-only.")]
    public async Task Cannot_send_blocked_nested_member(bool isPatch, bool insideCollectionElement, string member, string expectedTitle, string expectedDetail)
    {
        // Arrange
        var name = new JsonObject
        {
            ["lastName"] = "Doe",
            [member] = "value"
        };

        HttpResponseMessage httpResponse;
        Document responseDocument;
        string expectedPointer;

        // Act
        if (insideCollectionElement)
        {
            (httpResponse, responseDocument) = await SendFavoritesAsync(isPatch, name);
            expectedPointer = $"/data/attributes/favorites/0/name/{member}";
        }
        else
        {
            JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
            content["name"] = name;

            (httpResponse, responseDocument) = await SendContentAsync(isPatch, content);
            expectedPointer = $"/data/attributes/content/name/{member}";
        }

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be($"Failed to deserialize request body: {expectedTitle}");
        error.Detail.Should().Be(expectedDetail);
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be(expectedPointer);
    }

    [Fact]
    public async Task Can_send_nested_member_that_is_blocked_only_in_the_other_operation()
    {
        // Arrange
        JsonObject postContent = CompoundAttributesSamples.CreateMinimalContent();
        ((JsonObject)postContent["name"]!)["maidenName"] = "Byron";

        JsonObject patchContent = CompoundAttributesSamples.CreateMinimalContent();
        ((JsonObject)patchContent["name"]!)["nickName"] = "Countess";

        // Act
        (HttpResponseMessage postResponse, Document _) = await SendContentAsync(false, postContent);
        (HttpResponseMessage patchResponse, Document _) = await SendContentAsync(true, patchContent);

        // Assert
        postResponse.ShouldHaveStatusCode(HttpStatusCode.Created);
        patchResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Cannot_echo_a_get_payload_that_contains_read_only_members()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(existingContact);
            await dbContext.SaveChangesAsync();
        });

        (HttpResponseMessage getResponse, Document getDocument) = await _testContext.ExecuteGetAsync<Document>($"/contacts/{existingContact.StringId}");

        getResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        getDocument.Data.SingleValue.Should().NotBeNull();

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(getDocument.Data.SingleValue.GetAttribute("content"));

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().NotBeEmpty();
        responseDocument.Errors[0].Source.Should().NotBeNull();
        responseDocument.Errors[0].Source!.Pointer.Should().StartWith("/data/attributes/content/");
    }

    private static JsonObject CreateContentWithUnknownMember(string depth)
    {
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();

        switch (depth)
        {
            case "content":
            {
                content["nope"] = "x";
                break;
            }
            case "name":
            {
                ((JsonObject)content["name"]!)["nope"] = "x";
                break;
            }
            case "phoneNumbers":
            {
                content["phoneNumbers"] = JsonNode.Parse("""[ { "number": "1", "nope": "x" } ]""");
                break;
            }
            default:
            {
                content["company"] = JsonNode.Parse("""{ "name": "Acme", "nope": "x" }""");
                break;
            }
        }

        return content;
    }

    private async Task<(HttpResponseMessage, Document)> SendContentAsync(bool isPatch, JsonNode content)
    {
        if (!isPatch)
        {
            return await _testContext.ExecutePostAsync<Document>("/contacts", CompoundAttributesSamples.CreateContactPostBody(content));
        }

        Contact existingContact = CompoundAttributesSamples.CreateContact();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(existingContact);
            await dbContext.SaveChangesAsync();
        });

        return await _testContext.ExecutePatchAsync<Document>($"/contacts/{existingContact.StringId}",
            CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, content));
    }

    private async Task<(HttpResponseMessage, Document)> SendFavoritesAsync(bool isPatch, JsonObject name)
    {
        var favorites = new JsonArray(new JsonObject
        {
            ["name"] = name
        });

        if (!isPatch)
        {
            var postBody = new
            {
                data = new
                {
                    type = "addressBooks",
                    attributes = new
                    {
                        name = "Book",
                        favorites
                    }
                }
            };

            return await _testContext.ExecutePostAsync<Document>("/addressBooks", postBody);
        }

        AddressBook existingAddressBook = _fakers.AddressBook.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(existingAddressBook);
            await dbContext.SaveChangesAsync();
        });

        var patchBody = new
        {
            data = new
            {
                type = "addressBooks",
                id = existingAddressBook.StringId,
                attributes = new
                {
                    favorites
                }
            }
        };

        return await _testContext.ExecutePatchAsync<Document>($"/addressBooks/{existingAddressBook.StringId}", patchBody);
    }

    private static void AssertSingleError(HttpResponseMessage httpResponse, Document responseDocument, string expectedTitle, string expectedPointer)
    {
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be(expectedTitle);
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be(expectedPointer);
    }
}
