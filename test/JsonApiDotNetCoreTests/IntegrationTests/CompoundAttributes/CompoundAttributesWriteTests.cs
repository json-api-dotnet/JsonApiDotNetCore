using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

public sealed class CompoundAttributesWriteTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;
    private readonly CompoundAttributesFakers _fakers = new();

    public CompoundAttributesWriteTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
        testContext.UseController<ContactNotesController>();
    }

    [Fact]
    public async Task Can_create_resource_with_compound_attributes()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                attributes = new
                {
                    content = new
                    {
                        name = new
                        {
                            firstName = "John",
                            lastName = "Doe",
                            displayName = "John Doe"
                        },
                        company = new
                        {
                            name = "Acme Corp",
                            department = "Engineering"
                        },
                        phoneNumbers = new[]
                        {
                            new
                            {
                                number = "555-1234",
                                label = "work"
                            },
                            new
                            {
                                number = "555-5678",
                                label = "mobile"
                            }
                        }
                    },
                    remarks = "New contact note"
                }
            }
        };

        const string route = "/contacts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        string newContactId = responseDocument.Data.SingleValue.Id!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(long.Parse(newContactId));

            contactInDatabase.Remarks.Should().Be("New contact note");
            contactInDatabase.Content.Should().NotBeNull();
            contactInDatabase.Content.Name.FirstName.Should().Be("John");
            contactInDatabase.Content.Name.LastName.Should().Be("Doe");
            contactInDatabase.Content.Name.DisplayName.Should().Be("John Doe");
            contactInDatabase.Content.Company.Should().NotBeNull();
            contactInDatabase.Content.Company!.Name.Should().Be("Acme Corp");
            contactInDatabase.Content.Company.Department.Should().Be("Engineering");
            contactInDatabase.Content.PhoneNumbers.Should().HaveCount(2);
            contactInDatabase.Content.PhoneNumbers[0].Number.Should().Be("555-1234");
            contactInDatabase.Content.PhoneNumbers[1].Number.Should().Be("555-5678");
        });
    }

    [Fact]
    public async Task Can_create_resource_with_primitive_collection()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "addressBooks",
                attributes = new
                {
                    name = "Work Address Book",
                    syncUrls = new[]
                    {
                        "https://example.com/sync",
                        "https://backup.example.com"
                    }
                }
            }
        };

        const string route = "/addressBooks";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        string newAddressBookId = responseDocument.Data.SingleValue.Id!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            AddressBook addressBookInDatabase = await dbContext.AddressBooks.FirstWithIdAsync(long.Parse(newAddressBookId));

            addressBookInDatabase.Name.Should().Be("Work Address Book");
            addressBookInDatabase.SyncUrls.Should().NotBeNull();
            addressBookInDatabase.SyncUrls.Should().BeEquivalentTo("https://example.com/sync", "https://backup.example.com");
        });
    }

    [Fact]
    public async Task Can_patch_resource_replacing_compound_attribute()
    {
        // Arrange
        Contact existingContact = _fakers.Contact.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(existingContact);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                id = existingContact.StringId,
                attributes = new
                {
                    content = new
                    {
                        name = new
                        {
                            firstName = "Jane",
                            lastName = "Smith",
                            displayName = "Jane Smith"
                        },
                        phoneNumbers = new[]
                        {
                            new
                            {
                                number = "999-0000",
                                label = "direct"
                            }
                        }
                    }
                }
            }
        };

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.Content.Name.FirstName.Should().Be("Jane");
            contactInDatabase.Content.Name.LastName.Should().Be("Smith");
            contactInDatabase.Content.Name.DisplayName.Should().Be("Jane Smith");
            contactInDatabase.Content.Company.Should().BeNull();
            contactInDatabase.Content.PhoneNumbers.Should().HaveCount(1);
            contactInDatabase.Content.PhoneNumbers[0].Number.Should().Be("999-0000");
        });
    }

    [Fact]
    public async Task Can_patch_resource_setting_compound_attribute_to_null()
    {
        // Arrange
        AddressBook existingAddressBook = _fakers.AddressBook.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(existingAddressBook);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "addressBooks",
                id = existingAddressBook.StringId,
                attributes = new Dictionary<string, object?>
                {
                    ["emergencyContact"] = null
                }
            }
        };

        string route = $"/addressBooks/{existingAddressBook.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

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
    public async Task Can_patch_resource_replacing_primitive_collection()
    {
        // Arrange
        AddressBook existingAddressBook = _fakers.AddressBook.Generate();

        existingAddressBook.SyncUrls =
        [
            "https://old1.com",
            "https://old2.com"
        ];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(existingAddressBook);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "addressBooks",
                id = existingAddressBook.StringId,
                attributes = new
                {
                    syncUrls = new[]
                    {
                        "https://new1.com"
                    }
                }
            }
        };

        string route = $"/addressBooks/{existingAddressBook.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            AddressBook addressBookInDatabase = await dbContext.AddressBooks.FirstWithIdAsync(existingAddressBook.Id);

            addressBookInDatabase.SyncUrls.Should().BeEquivalentTo("https://new1.com");
        });
    }

    [Fact]
    public async Task Can_patch_resource_clearing_compound_collection()
    {
        // Arrange
        Contact existingContact = _fakers.Contact.Generate();

        existingContact.Content.PhoneNumbers =
        [
            new ContactPhoneNumber
            {
                Number = "111-222"
            }
        ];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(existingContact);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                id = existingContact.StringId,
                attributes = new
                {
                    content = new
                    {
                        name = new
                        {
                            firstName = existingContact.Content.Name.FirstName,
                            lastName = existingContact.Content.Name.LastName,
                            displayName = existingContact.Content.Name.DisplayName
                        },
                        phoneNumbers = Array.Empty<object>()
                    }
                }
            }
        };

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.Content.PhoneNumbers.Should().BeEmpty();
        });
    }
}
