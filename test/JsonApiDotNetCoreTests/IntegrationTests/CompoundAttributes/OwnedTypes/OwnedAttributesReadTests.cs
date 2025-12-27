using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.OwnedTypes;

public sealed class OwnedAttributesReadTests : IClassFixture<IntegrationTestContext<TestableStartup<OwnedAttributesDbContext>, OwnedAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<OwnedAttributesDbContext>, OwnedAttributesDbContext> _testContext;
    private readonly OwnedAttributesFakers _fakers = new();

    public OwnedAttributesReadTests(IntegrationTestContext<TestableStartup<OwnedAttributesDbContext>, OwnedAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.UseTrackingBehaviorHack = false;
    }

    [Fact]
    public async Task Can_get_primary_resource_by_ID_with_include()
    {
        // Arrange
        var jsonApiOptions = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        jsonApiOptions.UseTrackingBehaviorHack = true;

        AddressBook addressBook = _fakers.AddressBook.GenerateOne();
        addressBook.Contacts = _fakers.Contact.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddressBooks.Add(addressBook);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/addressBooks/{addressBook.StringId}?include=contacts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("addressBooks");
        responseDocument.Data.SingleValue.Id.Should().Be(addressBook.StringId);

        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("emergencyContact").WhoseValue.SerializeAs<ContactRoot>().Should()
            .BeEquivalentTo(addressBook.EmergencyContact, options => options.WithStrictOrdering());

        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("favorites").WhoseValue.SerializeAs<List<ContactRoot>>().Should()
            .BeEquivalentTo(addressBook.Favorites, options => options.WithStrictOrdering());

        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("syncUrls").WhoseValue.As<List<object>>().Should()
            .BeEquivalentTo(addressBook.SyncUrls, options => options.WithStrictOrdering());

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("contacts").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.ManyValue.Should().HaveCount(2);

            value.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "contacts" && resource.Id == addressBook.Contacts.ElementAt(0).StringId);
            value.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "contacts" && resource.Id == addressBook.Contacts.ElementAt(1).StringId);
        });

        responseDocument.Included.Should().HaveCount(2);

        responseDocument.Included.Should().ContainSingle(resource => resource.Type == "contacts" && resource.Id == addressBook.Contacts.ElementAt(0).StringId)
            .Which.With(resource =>
            {
                resource.Attributes.Should().ContainKey("content").WhoseValue.SerializeAs<ContactRoot>().Should()
                    .BeEquivalentTo(addressBook.Contacts.ElementAt(0).Content, options => options.WithStrictOrdering());
            });

        responseDocument.Included.Should().ContainSingle(resource => resource.Type == "contacts" && resource.Id == addressBook.Contacts.ElementAt(1).StringId)
            .Which.With(resource =>
            {
                resource.Attributes.Should().ContainKey("content").WhoseValue.SerializeAs<ContactRoot>().Should()
                    .BeEquivalentTo(addressBook.Contacts.ElementAt(1).Content, options => options.WithStrictOrdering());
            });
    }

    [Fact]
    public async Task Can_filter_equality_inside_compound_attribute_chain()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[0].Content.Name.LastName = "Smith";
        contacts[1].Content.Name.LastName = "Jefferson";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=startsWith(content.name.lastName,'Jeff')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].Type.Should().Be("contacts");
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[1].StringId);
    }

    [Fact]
    public async Task Can_filter_count_inside_primitive_collection_attribute()
    {
        // Arrange
        List<AddressBook> addressBooks = _fakers.AddressBook.GenerateList(3);
        addressBooks[0].SyncUrls = null;
        addressBooks[1].SyncUrls!.Clear();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.AddRange(addressBooks);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/addressBooks?filter=greaterThan(count(syncUrls),'0')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].Type.Should().Be("addressBooks");
        responseDocument.Data.ManyValue[0].Id.Should().Be(addressBooks[2].StringId);
    }

    [Fact]
    public async Task Can_filter_count_inside_compound_collection_attribute()
    {
        // Arrange
        List<AddressBook> addressBooks = _fakers.AddressBook.GenerateList(3);
        addressBooks[0].Favorites = null;
        addressBooks[1].Favorites!.Clear();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.AddRange(addressBooks);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/addressBooks?filter=greaterThan(count(favorites),'0')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].Type.Should().Be("addressBooks");
        responseDocument.Data.ManyValue[0].Id.Should().Be(addressBooks[2].StringId);
    }

    [Fact]
    public async Task Can_filter_count_inside_compound_collection_attribute_chain()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(3);
        // TODO: Uncommenting the line below crashes with a SQL error.
        contacts[0].Content.Addresses!.Clear();
        //contacts[0].Content.Addresses = null;
        contacts[1].Content.Addresses!.Clear();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=greaterThan(count(content.addresses),'0')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].Type.Should().Be("contacts");
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[2].StringId);
    }

    [Fact]
    public async Task Can_filter_has_inside_compound_collection_attribute_chain()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[1].Content.EmailAddresses.Clear();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=not(has(content.emailAddresses))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].Type.Should().Be("contacts");
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[1].StringId);
    }

    [Fact]
    public async Task Can_filter_nested_conditional_has_inside_compound_collection_attribute_chain()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[0].Content.EmailAddresses.Clear();

        contacts[1].Content.EmailAddresses.Add(LabeledValue<string>.Create("unknown@email.com"));

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=has(content.emailAddresses,equals(value,'unknown@email.com'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].Type.Should().Be("contacts");
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[1].StringId);
    }

    // TODO: Produce a good error message instead of crashing EF Core.
    // TODO: Add similar tests for filter equality, but allow a null check.
    [Fact]
    public async Task Cannot_sort_on_compound_attribute()
    {
        // Arrange
        const string route = "/contacts?sort=content.name";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        error.Title.Should().Be("An unhandled error occurred while processing this request.");
        error.Detail.Should().Contain("could not be translated");
        // TODO: error.Source.Should().NotBeNull();
        // TODO: error.Source.Parameter.Should().Be("sort");
    }

    [Fact]
    public async Task Can_sort_inside_compound_attribute_chain()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(3);
        contacts[0].Content.Company!.Name = "Contoso Corporation";
        contacts[1].Content.Company = null;
        contacts[2].Content.Company!.Name = "AdventureWorks";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?sort=content.company.name";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);

        responseDocument.Data.ManyValue[0].Type.Should().Be("contacts");
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[2].StringId);

        responseDocument.Data.ManyValue[1].Type.Should().Be("contacts");
        responseDocument.Data.ManyValue[1].Id.Should().Be(contacts[0].StringId);

        responseDocument.Data.ManyValue[2].Type.Should().Be("contacts");
        responseDocument.Data.ManyValue[2].Id.Should().Be(contacts[1].StringId);
    }

    [Fact(Skip = "TODO: Currently not supported.")]
    public async Task Can_select_inside_compound_attribute_chain()
    {
        // Arrange
        Contact contact = _fakers.Contact.GenerateOne();
        contact.Content.Company = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route =
            $"/contacts/{contact.StringId}?fields[contacts]=content.name,content.company.name,content.company.jobTitle,content.addresses,content.relations.label";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        // TODO: Add assertions.
    }
}
