using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

/// <summary>
/// One request for each example in the user documentation. These are smoke tests for the query-string side.
/// </summary>
public sealed class CompoundAttributesDocumentationTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;
    private readonly CompoundAttributesFakers _fakers = new();

    public CompoundAttributesDocumentationTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
        testContext.UseController<ContactNotesController>();
    }

    [Fact]
    public async Task Filter_on_nested_attribute()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[0].Content.Name.LastName = "Smithson";
        contacts[1].Content.Name.LastName = "Johnson";

        await SeedAsync(contacts);

        const string route = "/contacts?filter=startsWith(content.name.lastName,'Smi')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[0].StringId);
    }

    [Fact]
    public async Task Filter_on_deep_nested_attribute()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[0].Content.Company!.Name = "Contoso";
        contacts[1].Content.Company!.Name = "Fabrikam";

        await SeedAsync(contacts);

        const string route = "/contacts?filter=equals(content.company.name,'Contoso')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[0].StringId);
    }

    [Fact]
    public async Task Filter_compound_collection_with_has()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[0].Content.PhoneNumbers[0].Label = "mobile";
        contacts[1].Content.PhoneNumbers.ForEach(phone => phone.Label = "home");

        await SeedAsync(contacts);

        const string route = "/contacts?filter=has(content.phoneNumbers,equals(label,'mobile'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[0].StringId);
    }

    [Fact]
    public async Task Filter_scalar_collection_with_has_and_underscore()
    {
        // Arrange
        List<AddressBook> addressBooks = _fakers.AddressBook.GenerateList(2);
        addressBooks[0].Categories = ["family", "work"];
        addressBooks[1].Categories = ["friends"];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.AddRange(addressBooks);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/addressBooks?filter=has(categories,equals(_,'family'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(addressBooks[0].StringId);
    }

    [Fact]
    public async Task Filter_by_count()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[0].Content.PhoneNumbers = [];

        await SeedAsync(contacts);

        const string route = "/contacts?filter=greaterThan(count(content.phoneNumbers),'0')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[1].StringId);
    }

    [Fact]
    public async Task Sort_by_nested_attribute()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(3);
        contacts[0].Content.Name.LastName = "Charlie";
        contacts[1].Content.Name.LastName = "Alpha";
        contacts[2].Content.Name.LastName = "Bravo";

        await SeedAsync(contacts);

        const string route = "/contacts?sort=content.name.lastName";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue.Select(resource => resource.Id).Should().ContainInOrder(contacts[1].StringId, contacts[2].StringId, contacts[0].StringId);
    }

    [Fact]
    public async Task Select_nested_attributes()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();

        await SeedAsync([contact]);

        const string route = "/contacts?fields[contacts]=content.name.lastName,content.company.name";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].GetAttribute("content").ShouldBeJson($$"""
            {
              "name": { "lastName": "{{contact.Content.Name.LastName}}" },
              "company": { "name": "{{contact.Content.Company!.Name.Replace("\"", "\\\"")}}" }
            }
            """);
    }

    [Fact]
    public async Task Filter_through_relationship()
    {
        // Arrange
        AddressBook addressBookSmith = _fakers.AddressBook.Generate();
        addressBookSmith.EmergencyContact!.Name.LastName = "Smith";

        AddressBook addressBookJones = _fakers.AddressBook.Generate();
        addressBookJones.EmergencyContact!.Name.LastName = "Jones";

        Contact contactSmith = _fakers.Contact.Generate();
        contactSmith.AddressBook = addressBookSmith;

        Contact contactJones = _fakers.Contact.Generate();
        contactJones.AddressBook = addressBookJones;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.Contacts.AddRange(contactSmith, contactJones);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=equals(addressBook.emergencyContact.name.lastName,'Smith')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contactSmith.StringId);
    }

    private async Task SeedAsync(List<Contact> contacts)
    {
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });
    }
}
