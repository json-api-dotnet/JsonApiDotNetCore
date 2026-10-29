using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

public sealed class CompoundAttributesSparseFieldSetTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;
    private readonly CompoundAttributesFakers _fakers = new();

    public CompoundAttributesSparseFieldSetTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
        testContext.UseController<ContactNotesController>();
    }

    [Fact]
    public async Task Can_select_leaf_property_in_compound_attribute()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}?fields[contacts]=content.name.lastName";

        // Act
        (HttpResponseMessage httpResponse, JsonElement responseElement) = await _testContext.ExecuteGetAsync<JsonElement>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        JsonElement attributes = responseElement.GetProperty("data").GetProperty("attributes");
        attributes.TryGetProperty("remarks", out _).Should().BeFalse();

        JsonElement contentElement = attributes.GetProperty("content");
        contentElement.TryGetProperty("name", out JsonElement nameElement).Should().BeTrue();
        nameElement.GetProperty("lastName").GetString().Should().Be(contact.Content.Name.LastName);
        nameElement.TryGetProperty("firstName", out _).Should().BeFalse();
        nameElement.TryGetProperty("displayName", out _).Should().BeFalse();
        contentElement.TryGetProperty("company", out _).Should().BeFalse();
        contentElement.TryGetProperty("phoneNumbers", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Can_select_leaf_property_in_compound_attribute_along_with_top_level_attribute()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}?fields[contacts]=content.name.lastName,remarks";

        // Act
        (HttpResponseMessage httpResponse, JsonElement responseElement) = await _testContext.ExecuteGetAsync<JsonElement>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        JsonElement attributes = responseElement.GetProperty("data").GetProperty("attributes");
        attributes.TryGetProperty("remarks", out _).Should().BeTrue();

        JsonElement contentElement = attributes.GetProperty("content");
        contentElement.TryGetProperty("name", out JsonElement nameElement).Should().BeTrue();
        nameElement.GetProperty("lastName").GetString().Should().Be(contact.Content.Name.LastName);
        nameElement.TryGetProperty("firstName", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Can_select_whole_compound_attribute()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}?fields[contacts]=content";

        // Act
        (HttpResponseMessage httpResponse, JsonElement responseElement) = await _testContext.ExecuteGetAsync<JsonElement>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        JsonElement attributes = responseElement.GetProperty("data").GetProperty("attributes");
        attributes.TryGetProperty("remarks", out _).Should().BeFalse();

        JsonElement contentElement = attributes.GetProperty("content");
        contentElement.TryGetProperty("name", out JsonElement nameElement).Should().BeTrue();
        nameElement.GetProperty("lastName").GetString().Should().Be(contact.Content.Name.LastName);
        nameElement.GetProperty("firstName").GetString().Should().Be(contact.Content.Name.FirstName);
        nameElement.GetProperty("displayName").GetString().Should().Be(contact.Content.Name.DisplayName);
    }

    [Fact]
    public async Task Can_select_top_level_attribute_and_omit_compound_attribute()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}?fields[contacts]=remarks";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("remarks");
        responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("content");
    }

    [Fact]
    public async Task Can_select_primitive_collection()
    {
        // Arrange
        AddressBook addressBook = _fakers.AddressBook.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(addressBook);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/addressBooks/{addressBook.StringId}?fields[addressBooks]=categories";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("name");
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("categories");
    }

    [Fact]
    public async Task Can_select_collection_of_compound_attributes()
    {
        // Arrange
        AddressBook addressBook = _fakers.AddressBook.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(addressBook);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/addressBooks/{addressBook.StringId}?fields[addressBooks]=favorites";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("name");
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("favorites");
    }

    [Fact]
    public async Task Can_select_nested_properties_inside_collection_of_compound_attributes()
    {
        // Arrange
        AddressBook addressBook = _fakers.AddressBook.Generate();
        addressBook.Favorites = _fakers.ContactRoot.Generate(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(addressBook);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/addressBooks/{addressBook.StringId}?fields[addressBooks]=favorites.name.lastName";

        // Act
        (HttpResponseMessage httpResponse, JsonElement responseElement) = await _testContext.ExecuteGetAsync<JsonElement>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        JsonElement attributes = responseElement.GetProperty("data").GetProperty("attributes");
        attributes.TryGetProperty("name", out _).Should().BeFalse();

        JsonElement favoritesElement = attributes.GetProperty("favorites");
        favoritesElement.GetArrayLength().Should().Be(2);

        for (int i = 0; i < 2; i++)
        {
            JsonElement favorite = favoritesElement[i];
            favorite.TryGetProperty("name", out JsonElement nameElement).Should().BeTrue();
            nameElement.GetProperty("lastName").GetString().Should().Be(addressBook.Favorites[i].Name.LastName);
            nameElement.TryGetProperty("firstName", out _).Should().BeFalse();
            favorite.TryGetProperty("company", out _).Should().BeFalse();
        }
    }

    [Fact]
    public async Task Can_select_multiple_sibling_properties_in_sparse_fieldsets()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}?fields[contacts]=content.name.firstName,content.name.lastName";

        // Act
        (HttpResponseMessage httpResponse, JsonElement responseElement) = await _testContext.ExecuteGetAsync<JsonElement>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        JsonElement content = responseElement.GetProperty("data").GetProperty("attributes").GetProperty("content");
        content.TryGetProperty("name", out JsonElement nameElement).Should().BeTrue();
        nameElement.GetProperty("firstName").GetString().Should().Be(contact.Content.Name.FirstName);
        nameElement.GetProperty("lastName").GetString().Should().Be(contact.Content.Name.LastName);
        nameElement.TryGetProperty("middleName", out _).Should().BeFalse();
        content.TryGetProperty("company", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Can_select_properties_across_different_compound_branches()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();

        contact.Content.Company = new ContactCompany
        {
            Name = "Acme Corp",
            Department = "Engineering"
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}?fields[contacts]=content.name.lastName,content.company.name";

        // Act
        (HttpResponseMessage httpResponse, JsonElement responseElement) = await _testContext.ExecuteGetAsync<JsonElement>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        JsonElement content = responseElement.GetProperty("data").GetProperty("attributes").GetProperty("content");
        content.TryGetProperty("name", out JsonElement nameElement).Should().BeTrue();
        nameElement.GetProperty("lastName").GetString().Should().Be(contact.Content.Name.LastName);
        nameElement.TryGetProperty("firstName", out _).Should().BeFalse();

        content.TryGetProperty("company", out JsonElement companyElement).Should().BeTrue();
        companyElement.GetProperty("name").GetString().Should().Be(contact.Content.Company!.Name);
        companyElement.TryGetProperty("department", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Can_combine_sparse_fieldsets_with_relationship_includes()
    {
        // Arrange
        AddressBook addressBook = _fakers.AddressBook.Generate();
        Contact contact = _fakers.Contact.Generate();
        contact.AddressBook = addressBook;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(addressBook);
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}?include=addressBook&fields[contacts]=content.name.lastName,addressBook&fields[addressBooks]=name";

        // Act
        (HttpResponseMessage httpResponse, JsonElement responseElement) = await _testContext.ExecuteGetAsync<JsonElement>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        JsonElement included = responseElement.GetProperty("included");
        included.GetArrayLength().Should().Be(1);

        JsonElement includedAttributes = included[0].GetProperty("attributes");
        includedAttributes.TryGetProperty("name", out _).Should().BeTrue();
        includedAttributes.TryGetProperty("categories", out _).Should().BeFalse();

        JsonElement content = responseElement.GetProperty("data").GetProperty("attributes").GetProperty("content");
        content.TryGetProperty("name", out JsonElement nameElement).Should().BeTrue();
        nameElement.GetProperty("lastName").GetString().Should().Be(contact.Content.Name.LastName);
        nameElement.TryGetProperty("firstName", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Selecting_only_relationships_implicitly_materializes_compound_properties()
    {
        // Arrange
        AddressBook addressBook = _fakers.AddressBook.Generate();
        Contact contact = _fakers.Contact.Generate();
        contact.AddressBook = addressBook;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(addressBook);
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}?fields[contacts]=addressBook";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("addressBook");
        responseDocument.Data.SingleValue.Attributes.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(contact.Id);
            contactInDatabase.Content.Should().NotBeNull();
            contactInDatabase.Content.Name.Should().NotBeNull();
            contactInDatabase.Content.Name.LastName.Should().Be(contact.Content.Name.LastName);
        });
    }

    [Fact]
    public async Task View_blocked_nested_attribute_is_pruned_when_retrieving_compound_attribute()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();
        contact.Content.Name.SecretAlias = "Classified";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}?fields[contacts]=content";

        // Act
        (HttpResponseMessage httpResponse, JsonElement responseElement) = await _testContext.ExecuteGetAsync<JsonElement>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        JsonElement name = responseElement.GetProperty("data").GetProperty("attributes").GetProperty("content").GetProperty("name");
        name.TryGetProperty("lastName", out _).Should().BeTrue();
        name.TryGetProperty("secretAlias", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Cannot_select_view_blocked_nested_attribute()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        var parameterValue = new MarkedText("content.name.^secretAlias", '^');
        string route = $"/contacts/{contact.StringId}?fields[contacts]={parameterValue.Text}";

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
    public async Task Can_select_leaf_property_in_compound_attribute_at_secondary_endpoint()
    {
        // Arrange
        AddressBook addressBook = _fakers.AddressBook.Generate();
        Contact contact = _fakers.Contact.Generate();
        contact.AddressBook = addressBook;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(addressBook);
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/addressBooks/{addressBook.StringId}/contacts?fields[contacts]=content.name.lastName";

        // Act
        (HttpResponseMessage httpResponse, JsonElement responseElement) = await _testContext.ExecuteGetAsync<JsonElement>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        JsonElement data = responseElement.GetProperty("data");
        data.GetArrayLength().Should().Be(1);

        JsonElement attributes = data[0].GetProperty("attributes");
        attributes.TryGetProperty("remarks", out _).Should().BeFalse();

        JsonElement content = attributes.GetProperty("content");
        content.TryGetProperty("name", out JsonElement nameElement).Should().BeTrue();
        nameElement.GetProperty("lastName").GetString().Should().Be(contact.Content.Name.LastName);
        nameElement.TryGetProperty("firstName", out _).Should().BeFalse();
        content.TryGetProperty("company", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Can_get_relationship_linkage_for_resource_with_compound_attribute()
    {
        // Arrange
        AddressBook addressBook = _fakers.AddressBook.Generate();
        Contact contact = _fakers.Contact.Generate();
        contact.AddressBook = addressBook;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(addressBook);
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/addressBooks/{addressBook.StringId}/relationships/contacts";

        // Act
        (HttpResponseMessage httpResponse, JsonElement responseElement) = await _testContext.ExecuteGetAsync<JsonElement>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        JsonElement data = responseElement.GetProperty("data");
        data.GetArrayLength().Should().Be(1);

        JsonElement identifier = data[0];
        identifier.GetProperty("type").GetString().Should().Be("contacts");
        identifier.GetProperty("id").GetString().Should().Be(contact.StringId);
        identifier.TryGetProperty("attributes", out _).Should().BeFalse();
        identifier.TryGetProperty("relationships", out _).Should().BeFalse();
    }
}
