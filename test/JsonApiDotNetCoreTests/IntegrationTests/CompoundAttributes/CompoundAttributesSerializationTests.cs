using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

/// <summary>
/// Tests that depend on serializer or error options, so each test uses its own application instance.
/// </summary>
public sealed class CompoundAttributesSerializationTests : IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>
{
    private readonly CompoundAttributesFakers _fakers = new();

    public CompoundAttributesSerializationTests()
    {
        UseController<AddressBooksController>();
        UseController<ContactsController>();
        UseController<ContactNotesController>();
    }

    [Theory]
    [InlineData(JsonIgnoreCondition.Never)]
    [InlineData(JsonIgnoreCondition.WhenWritingNull)]
    [InlineData(JsonIgnoreCondition.WhenWritingDefault)]
    public async Task Can_apply_ignore_condition_to_members_of_compounds(JsonIgnoreCondition condition)
    {
        // Arrange
        var options = (JsonApiOptions)App.Services.GetRequiredService<IJsonApiOptions>();
        options.SerializerOptions.DefaultIgnoreCondition = condition;

        Contact contact = CompoundAttributesSamples.CreateContact();
        contact.Content.PhoneNumbers[0].CountryPrefix = null;
        contact.Content.PhoneNumbers[0].Kind = null;
        contact.Content.PhoneNumbers[1].Kind = PhoneKind.Mobile;
        contact.Content.Company!.SetRegistrationNumber(null);
        contact.Content.Company.Departments = [];
        contact.Content.Relations = null;
        contact.Content.EmailAddresses = [];

        contact.Content.ImportantDates =
        [
            new LabeledValue<DateOnly>
            {
                Value = default,
                Label = "unknown"
            }
        ];

        contact.LastKnownLocation = new GeoPoint
        {
            Latitude = 0,
            Longitude = 5
        };

        await RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        bool writesNull = condition == JsonIgnoreCondition.Never;
        bool writesDefault = condition != JsonIgnoreCondition.WhenWritingDefault;

        ResourceObject resource = responseDocument.Data.SingleValue;

        // Elements of a compound collection.
        HasMember(resource.GetAttribute("content", "phoneNumbers.0"), "countryPrefix").Should().Be(writesNull);
        HasMember(resource.GetAttribute("content", "phoneNumbers.0"), "kind").Should().Be(writesNull);

        // The default value of a nullable member is null, so a non-null enum value is always written.
        HasMember(resource.GetAttribute("content", "phoneNumbers.1"), "kind").Should().BeTrue();

        // The default value of a non-nullable member in a generic element is written only when defaults are written.
        HasMember(resource.GetAttribute("content", "importantDates.0"), "value").Should().Be(writesDefault);
        HasMember(resource.GetAttribute("content", "importantDates.0"), "label").Should().BeTrue();

        // Three levels deep: attribute, compound, nested member.
        HasMember(resource.GetAttribute("content", "company"), "registrationNumber").Should().Be(writesNull);

        // An empty collection is neither null nor default.
        resource.GetAttribute("content", "company.departments").ShouldBeJson("[]");
        resource.GetAttribute("content", "emailAddresses").ShouldBeJson("[]");

        // A null collection is written only when nulls are written.
        HasMember(resource.GetAttribute("content"), "relations").Should().Be(writesNull);

        // Members of a nullable struct.
        JsonNode? location = resource.GetAttribute("lastKnownLocation");
        HasMember(location, "latitude").Should().Be(writesDefault);
        HasMember(location, "longitude").Should().BeTrue();
    }

    [Theory]
    [InlineData(JsonIgnoreCondition.Never)]
    [InlineData(JsonIgnoreCondition.WhenWritingNull)]
    [InlineData(JsonIgnoreCondition.WhenWritingDefault)]
    public async Task Can_apply_ignore_condition_to_four_levels_of_compounds_in_collection(JsonIgnoreCondition condition)
    {
        // Arrange
        var options = (JsonApiOptions)App.Services.GetRequiredService<IJsonApiOptions>();
        options.SerializerOptions.DefaultIgnoreCondition = condition;

        AddressBook addressBook = _fakers.AddressBook.Generate();
        addressBook.Favorites = [CompoundAttributesSamples.CreateContact().Content];
        addressBook.Favorites[0].Company!.Department = null;

        await RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(addressBook);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/addressBooks/{addressBook.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        // attribute (favorites) -> element -> company -> department
        JsonNode? company = responseDocument.Data.SingleValue.GetAttribute("favorites", "0.company");
        HasMember(company, "department").Should().Be(condition == JsonIgnoreCondition.Never);
        HasMember(company, "name").Should().BeTrue();
    }

    [Fact]
    public async Task Can_return_same_ETag_for_identical_compound()
    {
        // Arrange
        Contact contact = CompoundAttributesSamples.CreateContact();

        await RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}";

        // Act
        (HttpResponseMessage firstResponse, Document _) = await ExecuteGetAsync<Document>(route);
        (HttpResponseMessage secondResponse, Document _) = await ExecuteGetAsync<Document>(route);

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(contact.StringId!, CompoundAttributesSamples.CreateIdenticalPatchContent());
        (HttpResponseMessage patchResponse, string _) = await ExecutePatchAsync<string>(route, requestBody);

        (HttpResponseMessage afterIdenticalPatchResponse, Document _) = await ExecuteGetAsync<Document>(route);

        JsonObject changedContent = CompoundAttributesSamples.CreateIdenticalPatchContent();
        changedContent["audit"]!["createdBy"] = "someone-else";
        requestBody = CompoundAttributesSamples.CreateContactPatchBody(contact.StringId!, changedContent);
        (HttpResponseMessage _, string _) = await ExecutePatchAsync<string>(route, requestBody);

        (HttpResponseMessage afterChangedPatchResponse, Document _) = await ExecuteGetAsync<Document>(route);

        // Assert
        firstResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        patchResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        firstResponse.Headers.ETag.Should().NotBeNull();
        secondResponse.Headers.ETag.Should().Be(firstResponse.Headers.ETag);
        afterIdenticalPatchResponse.Headers.ETag.Should().Be(firstResponse.Headers.ETag);
        afterChangedPatchResponse.Headers.ETag.Should().NotBe(firstResponse.Headers.ETag);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Can_include_or_hide_request_body_in_compound_conversion_error(bool includeRequestBody)
    {
        // Arrange
        var options = (JsonApiOptions)App.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeRequestBodyInErrors = includeRequestBody;

        const string requestBody = """
            { "data": { "type": "contacts", "attributes": { "content": { "name": 5 } } } }
            """;

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.Title.Should().Be("Failed to deserialize request body: Incompatible attribute value found.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/content/name");

        if (includeRequestBody)
        {
            error.Meta.Should().ContainRequestBody(requestBody);
        }
        else
        {
            error.Meta.Should().NotContainKey("requestBody");
        }
    }

    [Fact]
    public async Task Can_hide_request_body_in_unknown_nested_member_error()
    {
        // Arrange
        var options = (JsonApiOptions)App.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeRequestBodyInErrors = false;
        options.AllowUnknownFieldsInRequestBody = false;

        const string requestBody = """
            { "data": { "type": "contacts", "attributes": { "content": { "phoneNumbers": [ { "nope": 1 } ] } } } }
            """;

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.Detail.Should().Be("Attribute 'nope' does not exist on compound type 'ContactPhoneNumber'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/content/phoneNumbers/0/nope");
        error.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Can_escape_member_name_in_error_pointer()
    {
        // Arrange
        var options = (JsonApiOptions)App.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownFieldsInRequestBody = false;

        const string requestBody = """
            { "data": { "type": "contacts", "attributes": { "content": { "phoneNumbers": [ { "a~b/c": 1 } ] } } } }
            """;

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.Detail.Should().Be("Attribute 'a~b/c' does not exist on compound type 'ContactPhoneNumber'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/content/phoneNumbers/0/a~0b~1c");
    }

    [Fact]
    public async Task Can_round_trip_private_and_init_only_setters()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateIdenticalPatchContent();
        content["company"]!["registrationNumber"] = "REG-9";
        content["addresses"]![0]!["isoCountryCode"] = "BE";
        ((JsonObject)content["name"]!).Remove("nickName");

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage postResponse, Document postDocument) = await ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        postResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        postDocument.Data.SingleValue.Should().NotBeNull();
        postDocument.Data.SingleValue.GetAttribute("content", "company.registrationNumber").ShouldBeJson("\"REG-9\"");
        postDocument.Data.SingleValue.GetAttribute("content", "addresses.0.isoCountryCode").ShouldBeJson("\"BE\"");

        string contactId = postDocument.Data.SingleValue.Id!;

        // Act
        (HttpResponseMessage getResponse, Document getDocument) = await ExecuteGetAsync<Document>($"/contacts/{contactId}");

        // Assert
        getResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        getDocument.Data.SingleValue.Should().NotBeNull();
        getDocument.Data.SingleValue.GetAttribute("content", "company.registrationNumber").ShouldBeJson("\"REG-9\"");
        getDocument.Data.SingleValue.GetAttribute("content", "addresses.0.isoCountryCode").ShouldBeJson("\"BE\"");

        // Act
        (HttpResponseMessage sparseResponse, Document sparseDocument) =
            await ExecuteGetAsync<Document>($"/contacts/{contactId}?fields[contacts]=content.company.registrationNumber,content.addresses.isoCountryCode");

        // Assert
        sparseResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        sparseDocument.Data.SingleValue.Should().NotBeNull();

        sparseDocument.Data.SingleValue.GetAttribute("content").ShouldBeJson("""
            { "company": { "registrationNumber": "REG-9" }, "addresses": [ { "isoCountryCode": "BE" } ] }
            """);
    }

    [Fact]
    public async Task Can_get_private_and_init_only_setters_through_include_with_sparse_fieldset()
    {
        // Arrange
        AddressBook addressBook = _fakers.AddressBook.Generate();
        Contact contact = CompoundAttributesSamples.CreateContact();
        contact.Content.Company!.SetRegistrationNumber("REG-7");
        addressBook.Contacts.Add(contact);

        await RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            await dbContext.ClearTableAsync<Contact>();
            dbContext.AddressBooks.Add(addressBook);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/addressBooks/{addressBook.StringId}?include=contacts" +
            "&fields[contacts]=content.company.registrationNumber,content.addresses.isoCountryCode,content.addresses.city";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(1);

        responseDocument.Included[0].GetAttribute("content").ShouldBeJson("""
            { "company": { "registrationNumber": "REG-7" }, "addresses": [ { "isoCountryCode": "NL", "city": "Utrecht" } ] }
            """);
    }

    [Fact]
    public async Task Can_get_private_and_init_only_setters_at_secondary_endpoint_with_sparse_fieldset()
    {
        // Arrange
        AddressBook addressBook = _fakers.AddressBook.Generate();
        Contact contact = CompoundAttributesSamples.CreateContact();
        contact.Content.Company!.SetRegistrationNumber("REG-8");
        addressBook.Contacts.Add(contact);

        await RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            await dbContext.ClearTableAsync<Contact>();
            dbContext.AddressBooks.Add(addressBook);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/addressBooks/{addressBook.StringId}/contacts?fields[contacts]=content.company.registrationNumber,content.addresses.isoCountryCode";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].GetAttribute("content").ShouldBeJson("""
            { "company": { "registrationNumber": "REG-8" }, "addresses": [ { "isoCountryCode": "NL" } ] }
            """);
    }

    private static bool HasMember(JsonNode? node, string name)
    {
        node.Should().BeOfType<JsonObject>();
        return ((JsonObject)node).ContainsKey(name);
    }
}
