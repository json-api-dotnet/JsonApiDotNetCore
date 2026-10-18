using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

public sealed class CompoundAttributesReadTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;
    private readonly CompoundAttributesFakers _fakers = new();

    public CompoundAttributesReadTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
        testContext.UseController<ContactNotesController>();
    }

    [Fact]
    public async Task Can_get_resource_by_ID_with_compound_attributes()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();

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
        responseDocument.Data.SingleValue.Id.Should().Be(contact.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("remarks").WhoseValue.Should().Be(contact.Remarks);

        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("content");
        object? contentValue = responseDocument.Data.SingleValue.Attributes!["content"];
        contentValue.Should().BeOfType<JsonElement>();

        var contentElement = (JsonElement)contentValue!;
        contentElement.GetProperty("name").GetProperty("lastName").GetString().Should().Be(contact.Content.Name.LastName);
        contentElement.GetProperty("name").GetProperty("displayName").GetString().Should().Be(contact.Content.Name.DisplayName);

        if (contact.Content.Company != null)
        {
            contentElement.GetProperty("company").GetProperty("name").GetString().Should().Be(contact.Content.Company.Name);
        }

        contentElement.GetProperty("phoneNumbers").GetArrayLength().Should().Be(contact.Content.PhoneNumbers.Count);

        contentElement.GetProperty("name").GetProperty("lastName").GetString().Should().Be(contact.Content.Name.LastName);
        contentElement.GetProperty("name").GetProperty("displayName").GetString().Should().Be(contact.Content.Name.DisplayName);
    }

    [Fact]
    public async Task Can_filter_by_nested_compound_attribute_equality()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(3);
        contacts[0].Content.Name.LastName = "Smith";
        contacts[1].Content.Name.LastName = "Jones";
        contacts[2].Content.Name.LastName = "Smith";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=equals(content.name.lastName,'Smith')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue.Select(resource => resource.Id).Should().BeEquivalentTo(contacts[0].StringId, contacts[2].StringId);
    }

    [Fact]
    public async Task Can_filter_by_nested_compound_attribute_text_matching()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[0].Content.Name.LastName = "Smithson";
        contacts[1].Content.Name.LastName = "Johnson";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=startsWith(content.name.lastName,'Smi')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[0].StringId);
    }

    [Fact]
    public async Task Can_filter_by_null_on_compound_attribute()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[0].Content.Company = null;
        contacts[1].Content.Company = _fakers.ContactCompany.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=equals(content.company,null)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[0].StringId);
    }

    [Fact]
    public async Task Can_filter_on_compound_collection_using_has()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);

        contacts[0].Content.PhoneNumbers =
        [
            new ContactPhoneNumber
            {
                Number = "111-111",
                Label = "work"
            },
            new ContactPhoneNumber
            {
                Number = "222-222",
                Label = "mobile"
            }
        ];

        contacts[1].Content.PhoneNumbers =
        [
            new ContactPhoneNumber
            {
                Number = "333-333",
                Label = "home"
            }
        ];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=has(content.phoneNumbers,equals(label,'mobile'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[0].StringId);
    }

    [Fact]
    public async Task Can_filter_on_primitive_collection_using_has_and_element_value()
    {
        // Arrange
        List<AddressBook> addressBooks = _fakers.AddressBook.GenerateList(2);

        addressBooks[0].Categories =
        [
            "https://example.com/sync",
            "https://backup.org"
        ];

        addressBooks[1].Categories = ["https://other.com/api"];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.AddRange(addressBooks);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/addressBooks?filter=has(categories,equals(_,'https://example.com/sync'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(addressBooks[0].StringId);
    }

    [Fact]
    public async Task Can_filter_on_primitive_collection_using_has_and_text_matching()
    {
        // Arrange
        List<AddressBook> addressBooks = _fakers.AddressBook.GenerateList(2);
        addressBooks[0].Categories = ["https://contoso.com/sync"];
        addressBooks[1].Categories = ["https://fabrikam.com/sync"];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.AddRange(addressBooks);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/addressBooks?filter=has(categories,contains(_,'contoso'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(addressBooks[0].StringId);
    }

    [Fact]
    public async Task Can_filter_by_count_on_compound_collection()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);

        contacts[0].Content.PhoneNumbers =
        [
            new ContactPhoneNumber
            {
                Number = "111-111"
            },
            new ContactPhoneNumber
            {
                Number = "222-222"
            },
            new ContactPhoneNumber
            {
                Number = "333-333"
            }
        ];

        contacts[1].Content.PhoneNumbers =
        [
            new ContactPhoneNumber
            {
                Number = "444-444"
            }
        ];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=greaterThan(count(content.phoneNumbers),'2')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[0].StringId);
    }

    [Fact]
    public async Task Can_filter_by_count_on_primitive_collection()
    {
        // Arrange
        List<AddressBook> addressBooks = _fakers.AddressBook.GenerateList(2);

        addressBooks[0].Categories =
        [
            "url1",
            "url2",
            "url3"
        ];

        addressBooks[1].Categories = ["url1"];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.AddRange(addressBooks);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/addressBooks?filter=greaterThan(count(categories),'1')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(addressBooks[0].StringId);
    }

    [Fact]
    public async Task Can_sort_by_nested_compound_attributes()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(3);
        contacts[0].Content.Name.LastName = "Smith";
        contacts[0].Content.Name.FirstName = "Zach";
        contacts[1].Content.Name.LastName = "Adams";
        contacts[1].Content.Name.FirstName = "John";
        contacts[2].Content.Name.LastName = "Smith";
        contacts[2].Content.Name.FirstName = "Alice";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?sort=content.name.lastName,content.name.firstName";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[1].StringId); // Adams
        responseDocument.Data.ManyValue[1].Id.Should().Be(contacts[2].StringId); // Smith, Alice
        responseDocument.Data.ManyValue[2].Id.Should().Be(contacts[0].StringId); // Smith, Zach
    }

    [Fact]
    public async Task Can_sort_descending_by_nested_compound_attribute()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[0].Content.Name.LastName = "Adams";
        contacts[1].Content.Name.LastName = "Smith";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?sort=-content.name.lastName";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[1].StringId); // Smith
        responseDocument.Data.ManyValue[1].Id.Should().Be(contacts[0].StringId); // Adams
    }

    [Fact]
    public async Task Can_filter_through_relationship_with_compound_attribute()
    {
        // Arrange
        AddressBook addressBook = _fakers.AddressBook.Generate();
        addressBook.EmergencyContact = _fakers.ContactRoot.Generate();
        addressBook.EmergencyContact.Name.LastName = "SpecialName";

        Contact contact = _fakers.Contact.Generate();
        contact.AddressBook = addressBook;

        Contact otherContact = _fakers.Contact.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.Contacts.AddRange(contact, otherContact);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=equals(addressBook.emergencyContact.name.lastName,'SpecialName')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contact.StringId);
    }

    [Fact]
    public async Task Can_filter_to_many_relationship_with_compound_collection_condition()
    {
        // Arrange
        AddressBook addressBook1 = _fakers.AddressBook.Generate();
        Contact contact1 = _fakers.Contact.Generate();

        contact1.Content.PhoneNumbers =
        [
            new ContactPhoneNumber
            {
                Number = "999",
                Label = "hotline"
            }
        ];

        addressBook1.Contacts.Add(contact1);

        AddressBook addressBook2 = _fakers.AddressBook.Generate();
        Contact contact2 = _fakers.Contact.Generate();

        contact2.Content.PhoneNumbers =
        [
            new ContactPhoneNumber
            {
                Number = "111",
                Label = "home"
            }
        ];

        addressBook2.Contacts.Add(contact2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.AddRange(addressBook1, addressBook2);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/addressBooks?filter=has(contacts,has(content.phoneNumbers,equals(label,'hotline')))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(addressBook1.StringId);
    }

    [Fact]
    public async Task Can_filter_categories_using_startsWith()
    {
        // Arrange
        List<AddressBook> addressBooks = _fakers.AddressBook.GenerateList(2);
        addressBooks[0].Categories = ["Personal", "Work"];
        addressBooks[1].Categories = ["Business", "Archive"];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.AddRange(addressBooks);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/addressBooks?filter=has(categories,startsWith(_,'Pers'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(addressBooks[0].StringId);
    }

    [Fact]
    public async Task Can_filter_categories_using_contains()
    {
        // Arrange
        List<AddressBook> addressBooks = _fakers.AddressBook.GenerateList(2);
        addressBooks[0].Categories = ["Personal", "Networking"];
        addressBooks[1].Categories = ["Family"];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.AddRange(addressBooks);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/addressBooks?filter=has(categories,contains(_,'ork'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(addressBooks[0].StringId);
    }

    [Fact]
    public async Task Can_filter_syncDeviceIds_by_has_guid_equality()
    {
        // Arrange
        List<AddressBook> addressBooks = _fakers.AddressBook.GenerateList(2);
        Guid targetGuid = Guid.NewGuid();
        addressBooks[0].SyncDeviceIds = [targetGuid, Guid.NewGuid()];
        addressBooks[1].SyncDeviceIds = [Guid.NewGuid()];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.AddRange(addressBooks);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/addressBooks?filter=has(syncDeviceIds,equals(_,'{targetGuid}'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(addressBooks[0].StringId);
    }

    [Fact]
    public async Task Can_filter_syncDeviceIds_by_count()
    {
        // Arrange
        List<AddressBook> addressBooks = _fakers.AddressBook.GenerateList(2);
        addressBooks[0].SyncDeviceIds = [Guid.NewGuid(), Guid.NewGuid()];
        addressBooks[1].SyncDeviceIds = [];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.AddRange(addressBooks);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/addressBooks?filter=greaterThan(count(syncDeviceIds),'0')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(addressBooks[0].StringId);
    }

    [Fact]
    public async Task Can_sort_by_scalar_collection_count()
    {
        // Arrange
        List<AddressBook> addressBooks = _fakers.AddressBook.GenerateList(3);
        addressBooks[0].SyncDeviceIds = [Guid.NewGuid()];
        addressBooks[1].SyncDeviceIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        addressBooks[2].SyncDeviceIds = [Guid.NewGuid(), Guid.NewGuid()];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.AddRange(addressBooks);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/addressBooks?sort=count(syncDeviceIds)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be(addressBooks[0].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(addressBooks[2].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(addressBooks[1].StringId);
    }

    [Fact]
    public async Task Can_sort_by_descending_scalar_collection_count()
    {
        // Arrange
        List<AddressBook> addressBooks = _fakers.AddressBook.GenerateList(3);
        addressBooks[0].SyncDeviceIds = [Guid.NewGuid()];
        addressBooks[1].SyncDeviceIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        addressBooks[2].SyncDeviceIds = [Guid.NewGuid(), Guid.NewGuid()];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.AddRange(addressBooks);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/addressBooks?sort=-count(syncDeviceIds)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be(addressBooks[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(addressBooks[2].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(addressBooks[0].StringId);
    }

    [Fact]
    public async Task Can_filter_businessHours_in_company()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[0].Content.Company = new ContactCompany
        {
            Name = "Company A",
            BusinessHours = [TimeSpan.FromHours(9), TimeSpan.FromHours(17)]
        };
        contacts[1].Content.Company = new ContactCompany
        {
            Name = "Company B",
            BusinessHours = [TimeSpan.FromHours(8), TimeSpan.FromHours(16)]
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=has(content.company.businessHours,equals(_,'09:00:00'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[0].StringId);
    }

    [Fact]
    public async Task Can_filter_ratings_with_integer_element()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[0].Content.Ratings = [5, null, 4];
        contacts[1].Content.Ratings = [1, 2, 3];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=has(content.ratings,equals(_,'5'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[0].StringId);
    }

    [Fact]
    public async Task Can_filter_importantDates_by_value()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[0].Content.ImportantDates =
        [
            new LabeledValue<DateOnly>
            {
                Value = new DateOnly(2020, 5, 15),
                Label = "anniversary"
            }
        ];
        contacts[1].Content.ImportantDates =
        [
            new LabeledValue<DateOnly>
            {
                Value = new DateOnly(2018, 1, 1),
                Label = "hired"
            }
        ];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=has(content.importantDates,equals(value,'2020-05-15'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[0].StringId);
    }

    [Fact]
    public async Task Can_filter_importantDates_by_label()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[0].Content.ImportantDates =
        [
            new LabeledValue<DateOnly>
            {
                Value = new DateOnly(2020, 5, 15),
                Label = "anniversary"
            }
        ];
        contacts[1].Content.ImportantDates =
        [
            new LabeledValue<DateOnly>
            {
                Value = new DateOnly(2018, 1, 1),
                Label = "hired"
            }
        ];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=has(content.importantDates,equals(label,'anniversary'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[0].StringId);
    }

    [Fact]
    public async Task Can_sort_by_compound_collection_count()
    {
        // Arrange
        List<AddressBook> addressBooks = _fakers.AddressBook.GenerateList(3);
        addressBooks[0].Favorites = _fakers.ContactRoot.GenerateList(1);
        addressBooks[1].Favorites = _fakers.ContactRoot.GenerateList(3);
        addressBooks[2].Favorites = _fakers.ContactRoot.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.AddRange(addressBooks);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/addressBooks?sort=count(favorites)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be(addressBooks[0].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(addressBooks[2].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(addressBooks[1].StringId);
    }

    [Fact]
    public async Task Can_sort_by_descending_compound_collection_count()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(3);
        contacts[0].Content.PhoneNumbers = _fakers.ContactPhoneNumber.GenerateList(1);
        contacts[1].Content.PhoneNumbers = _fakers.ContactPhoneNumber.GenerateList(3);
        contacts[2].Content.PhoneNumbers = _fakers.ContactPhoneNumber.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?sort=-count(content.phoneNumbers)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(contacts[2].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(contacts[0].StringId);
    }

    [Fact]
    public async Task Can_filter_using_predicate_less_has_on_compound_collection()
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[0].Content.PhoneNumbers = _fakers.ContactPhoneNumber.GenerateList(2);
        contacts[1].Content.PhoneNumbers = [];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/contacts?filter=has(content.phoneNumbers)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(contacts[0].StringId);
    }

    [Fact]
    public async Task Can_filter_using_predicate_less_has_on_scalar_collection()
    {
        // Arrange
        List<AddressBook> addressBooks = _fakers.AddressBook.GenerateList(2);
        addressBooks[0].Categories = ["CategoryA"];
        addressBooks[1].Categories = [];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.AddRange(addressBooks);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/addressBooks?filter=has(categories)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(addressBooks[0].StringId);
    }

    [Fact]
    public async Task Restricted_SecurityPin_is_omitted_without_sparse_fieldset()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();
        contact.Content.SecurityPin = "SecretPin123";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseRaw) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseRaw.Should().NotContain("SecretPin123");
        responseRaw.Should().NotContain("securityPin");
    }

    [Fact]
    public async Task Restricted_SecurityPin_is_omitted_with_whole_compound_sparse_fieldset()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();
        contact.Content.SecurityPin = "SecretPin456";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}?fields[contacts]=content";

        // Act
        (HttpResponseMessage httpResponse, string responseRaw) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        responseRaw.Should().NotContain("SecretPin456");
        responseRaw.Should().NotContain("securityPin");
    }
}
