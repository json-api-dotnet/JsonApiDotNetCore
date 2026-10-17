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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("remarks");
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("content");

        string responseRaw = await httpResponse.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(responseRaw);
        JsonElement contentElement = jsonDocument.RootElement
            .GetProperty("data")
            .GetProperty("attributes")
            .GetProperty("content");

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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("remarks");
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("content");

        string responseRaw = await httpResponse.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(responseRaw);
        JsonElement contentElement = jsonDocument.RootElement
            .GetProperty("data")
            .GetProperty("attributes")
            .GetProperty("content");

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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("remarks");
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("content");

        string responseRaw = await httpResponse.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(responseRaw);
        JsonElement contentElement = jsonDocument.RootElement
            .GetProperty("data")
            .GetProperty("attributes")
            .GetProperty("content");

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

        string route = $"/addressBooks/{addressBook.StringId}?fields[addressBooks]=syncUrls";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("name");
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("syncUrls");
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
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("name");
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("favorites");

        string responseRaw = await httpResponse.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(responseRaw);
        JsonElement favoritesElement = jsonDocument.RootElement
            .GetProperty("data")
            .GetProperty("attributes")
            .GetProperty("favorites");

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
}
