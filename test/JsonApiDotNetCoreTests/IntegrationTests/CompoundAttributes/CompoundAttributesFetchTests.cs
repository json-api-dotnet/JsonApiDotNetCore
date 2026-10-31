using System.Net;
using System.Text.Json.Nodes;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

public sealed class CompoundAttributesFetchTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;
    private readonly CompoundAttributesFakers _fakers = new();

    public CompoundAttributesFetchTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
        testContext.UseController<ContactNotesController>();
    }

    [Fact]
    public async Task Can_get_resource_with_all_shapes_populated()
    {
        // Arrange
        Contact contact = CompoundAttributesSamples.CreateContact();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        JsonNode? content = responseDocument.Data.SingleValue.GetAttribute("content");
        content.ShouldBeJson(CompoundAttributesSamples.ExpectedContentJson);

        // Declared member order is preserved.
        content!.ToJsonString().Should().Be(JsonNode.Parse(CompoundAttributesSamples.ExpectedContentJson)!.ToJsonString());
    }

    [Fact]
    public async Task Renders_null_for_null_compound()
    {
        // Arrange
        AddressBook addressBook = _fakers.AddressBook.Generate();
        addressBook.EmergencyContact = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(addressBook);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/addressBooks/{addressBook.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("emergencyContact");
        responseDocument.Data.SingleValue.GetAttribute("emergencyContact").Should().BeNull();
    }

    [Fact]
    public async Task Renders_null_and_empty_for_collections_distinctly()
    {
        // Arrange
        AddressBook addressBook = _fakers.AddressBook.Generate();
        addressBook.Categories = null;
        addressBook.Favorites = [];

        Contact contact = _fakers.Contact.Generate();
        contact.Content.PhoneNumbers = [];
        contact.Content.Relations = null;
        contact.Content.Ratings = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            await dbContext.ClearTableAsync<Contact>();
            dbContext.AddressBooks.Add(addressBook);
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        // Act
        (HttpResponseMessage httpResponse1, Document responseDocument1) = await _testContext.ExecuteGetAsync<Document>($"/addressBooks/{addressBook.StringId}");
        (HttpResponseMessage httpResponse2, Document responseDocument2) = await _testContext.ExecuteGetAsync<Document>($"/contacts/{contact.StringId}");

        // Assert
        httpResponse1.ShouldHaveStatusCode(HttpStatusCode.OK);
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument1.Data.SingleValue.Should().NotBeNull();
        responseDocument1.Data.SingleValue.GetAttribute("categories").Should().BeNull();
        responseDocument1.Data.SingleValue.GetAttribute("favorites").ShouldBeJson("[]");

        responseDocument2.Data.SingleValue.Should().NotBeNull();
        responseDocument2.Data.SingleValue.GetAttribute("content", "phoneNumbers").ShouldBeJson("[]");
        responseDocument2.Data.SingleValue.GetAttribute("content", "relations").Should().BeNull();
        responseDocument2.Data.SingleValue.GetAttribute("content", "ratings").Should().BeNull();
    }

    [Fact]
    public async Task Renders_null_nested_compound_inside_non_null_compound()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();
        contact.Content.Company = null;
        contact.Content.BirthDate = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        JsonNode? content = responseDocument.Data.SingleValue.GetAttribute("content");
        content.Should().BeOfType<JsonObject>();

        var contentObject = (JsonObject)content!;
        contentObject.Should().ContainKey("company").WhoseValue.Should().BeNull();
        contentObject.Should().ContainKey("birthDate").WhoseValue.Should().BeNull();
        contentObject.Should().ContainKey("name").WhoseValue.Should().NotBeNull();
    }

    [Fact(Skip =
        "KNOWN-ISSUE NEW-ef-null-element: EF Core cannot materialize a null element in a JSON complex collection (InvalidOperationException 'Invalid token type: Null').")]
    public async Task Renders_null_element_in_compound_collection()
    {
        // Arrange
        AddressBook addressBook = _fakers.AddressBook.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(addressBook);
            await dbContext.SaveChangesAsync();

            // Entity Framework Core refuses to save null elements, so we write the JSON directly.
            await dbContext.Database.ExecuteSqlRawAsync("""
                UPDATE "AddressBooks" SET "Favorites" = jsonb_build_array("Favorites" -> 0, 'null'::jsonb) WHERE "Id" = {0}
                """, addressBook.Id);
        });

        string route = $"/addressBooks/{addressBook.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        JsonNode? favorites = responseDocument.Data.SingleValue.GetAttribute("favorites");
        favorites.Should().BeOfType<JsonArray>();

        var favoritesArray = (JsonArray)favorites!;
        favoritesArray.Should().HaveCount(2);
        favoritesArray[0].Should().NotBeNull();
        favoritesArray[1].Should().BeNull();
    }

    [Fact]
    public async Task Omits_view_blocked_nested_members_at_every_depth()
    {
        // Arrange
        Contact contact = CompoundAttributesSamples.CreateContact();

        AddressBook addressBook = _fakers.AddressBook.Generate();
        addressBook.Favorites[0].SecurityPin = "9999";
        addressBook.Favorites[0].Name.SecretAlias = "alias";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.Contacts.Add(contact);
            dbContext.AddressBooks.Add(addressBook);
            await dbContext.SaveChangesAsync();
        });

        string[] routes =
        [
            $"/contacts/{contact.StringId}",
            $"/contacts/{contact.StringId}?fields[contacts]=content",
            $"/contacts/{contact.StringId}?fields[contacts]=content.name",
            $"/addressBooks/{addressBook.StringId}",
            $"/addressBooks/{addressBook.StringId}?fields[addressBooks]=favorites"
        ];

        foreach (string route in routes)
        {
            // Act
            (HttpResponseMessage httpResponse, string responseText) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

            responseText.Should().NotContain("securityPin", route);
            responseText.Should().NotContain("secretAlias", route);
            responseText.Should().NotContain("secret\"", route);
            responseText.Should().NotContain("9999", route);
            responseText.Should().NotContain("alias\"", route);
        }
    }

    [Fact]
    public async Task Cannot_select_view_blocked_nested_member()
    {
        // Arrange
        var parameterValue = new MarkedText("content.name.^secretAlias", '^');
        string route = $"/contacts?fields[contacts]={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified fieldset is invalid.");
        error.Detail.Should().Be($"Retrieving attribute 'secretAlias' is not allowed. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("fields[contacts]");
    }

    [Fact]
    public async Task Uses_public_names_for_nested_members()
    {
        // Arrange
        Contact contact = CompoundAttributesSamples.CreateContact();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        JsonNode? address = responseDocument.Data.SingleValue.GetAttribute("content", "addresses.0");
        address.Should().BeOfType<JsonObject>();

        var addressObject = (JsonObject)address!;
        addressObject.Should().ContainKey("zipCode");
        addressObject.Should().NotContainKey("postalCode");
        addressObject.Should().NotContainKey("line2");

        // The nested key is written verbatim, not rewritten by DictionaryKeyPolicy.
        addressObject.Should().ContainKey("Address-Line2");
    }

    [Fact]
    public async Task Renders_generic_compound_types()
    {
        // Arrange
        Contact contact = CompoundAttributesSamples.CreateContact();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        responseDocument.Data.SingleValue.GetAttribute("content", "emailAddresses").ShouldBeJson("""
            [ { "value": "ada@example.com", "label": "primary" } ]
            """);

        responseDocument.Data.SingleValue.GetAttribute("content", "importantDates").ShouldBeJson("""
            [ { "value": "1843-07-10", "label": "notes" } ]
            """);
    }

    [Theory]
    [InlineData("ratings", """[ 5, null, 3 ]""")]
    [InlineData("company.businessHours", """[ "09:00:00", "17:00:00" ]""")]
    [InlineData("importantDates", """[ { "value": "1843-07-10", "label": "notes" } ]""")]
    [InlineData("preferredContactMethods", """[ 1, 0 ]""")]
    [InlineData("creditLimits", """[ 100.5, 2000 ]""")]
    [InlineData("anniversaries", """[ "2020-01-02T00:00:00Z" ]""")]
    public async Task Renders_scalar_collection_element_types(string path, string expectedJson)
    {
        // Arrange
        Contact contact = CompoundAttributesSamples.CreateContact();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.GetAttribute("content", path).ShouldBeJson(expectedJson);
    }

    [Fact]
    public async Task Renders_guid_scalar_collection()
    {
        // Arrange
        AddressBook addressBook = _fakers.AddressBook.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(addressBook);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/addressBooks/{addressBook.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        var expected = new JsonArray(addressBook.SyncDeviceIds!.Select(id => JsonValue.Create(id.ToString())).ToArray<JsonNode?>());
        responseDocument.Data.SingleValue.GetAttribute("syncDeviceIds").ShouldBeJson(expected);
    }

    [Fact]
    public async Task Renders_calculated_read_only_member()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();
        contact.Content.Name.FirstName = "Ada";
        contact.Content.Name.MiddleName = null;
        contact.Content.Name.LastName = "Lovelace";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.GetAttribute("content", "name.displayName").ShouldBeJson("\"Ada Lovelace\"");
    }

    [Fact]
    public async Task Can_get_collection_of_resources()
    {
        // Arrange
        Contact contactFull = CompoundAttributesSamples.CreateContact();

        Contact contactMinimal = _fakers.Contact.Generate();
        contactMinimal.Content.Company = null;
        contactMinimal.Content.BirthDate = null;
        contactMinimal.Content.PhoneNumbers = [];
        contactMinimal.Content.Relations = null;

        Contact contactMedium = _fakers.Contact.Generate();
        contactMedium.Content.Company!.Departments = [];
        contactMedium.Content.Ratings = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contactFull, contactMinimal, contactMedium);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);

        ResourceObject[] resources = responseDocument.Data.ManyValue.ToArray();
        resources.Select(resource => resource.Id).Should().ContainInOrder(contactFull.StringId, contactMinimal.StringId, contactMedium.StringId);

        resources[0].GetAttribute("content").ShouldBeJson(CompoundAttributesSamples.ExpectedContentJson);

        resources[1].GetAttribute("content", "name.lastName").ShouldBeJson($"\"{contactMinimal.Content.Name.LastName}\"");
        resources[1].GetAttribute("content", "company").Should().BeNull();
        resources[1].GetAttribute("content", "birthDate").Should().BeNull();
        resources[1].GetAttribute("content", "phoneNumbers").ShouldBeJson("[]");
        resources[1].GetAttribute("content", "relations").Should().BeNull();

        resources[2].GetAttribute("content", "name.lastName").ShouldBeJson($"\"{contactMedium.Content.Name.LastName}\"");
        resources[2].GetAttribute("content", "company.departments").ShouldBeJson("[]");
        resources[2].GetAttribute("content", "ratings").Should().BeNull();
        resources[2].GetAttribute("content", "phoneNumbers").Should().BeOfType<JsonArray>().Subject.Should().HaveCount(2);
    }

    [Fact]
    public async Task Fetches_all_scalar_properties_when_fieldset_selects_read_only_nested_member()
    {
        // Arrange
        Contact contact = CompoundAttributesSamples.CreateContact();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}?fields[contacts]=content.name.displayName";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);

        responseDocument.Data.SingleValue.GetAttribute("content").ShouldBeJson("""
            { "name": { "displayName": "Ada M Lovelace" } }
            """);
    }

    [Fact]
    public async Task Renders_nullable_struct_compound()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();

        contact.LastKnownLocation = new GeoPoint
        {
            Latitude = 52.09,
            Longitude = 5.12
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        responseDocument.Data.SingleValue.GetAttribute("lastKnownLocation").ShouldBeJson("""
            { "latitude": 52.09, "longitude": 5.12 }
            """);
    }

    [Fact]
    public async Task Renders_same_shape_after_write()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateIdenticalPatchContent();
        ((JsonObject)content["name"]!).Remove("nickName");

        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                attributes = new
                {
                    content
                }
            }
        };

        // Act
        (HttpResponseMessage postResponse, Document postDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        postResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        postDocument.Data.SingleValue.Should().NotBeNull();
        string newContactId = postDocument.Data.SingleValue.Id!;

        // Act
        (HttpResponseMessage getResponse, Document getDocument) = await _testContext.ExecuteGetAsync<Document>($"/contacts/{newContactId}");

        // Assert
        getResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        getDocument.Data.SingleValue.Should().NotBeNull();
        postDocument.Data.SingleValue.GetAttribute("content")!.ToJsonString().Should().Be(getDocument.Data.SingleValue.GetAttribute("content")!.ToJsonString());
    }
}
