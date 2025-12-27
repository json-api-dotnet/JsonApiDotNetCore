using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.OwnedTypes;

public sealed class OwnedAttributesWriteTests : IClassFixture<IntegrationTestContext<TestableStartup<OwnedAttributesDbContext>, OwnedAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<OwnedAttributesDbContext>, OwnedAttributesDbContext> _testContext;
    private readonly OwnedAttributesFakers _fakers = new();

    public OwnedAttributesWriteTests(IntegrationTestContext<TestableStartup<OwnedAttributesDbContext>, OwnedAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
    }

    [Fact]
    public async Task Can_replace_primitive_collection()
    {
        // Arrange
        AddressBook existingAddressBook = _fakers.AddressBook.GenerateOne();
        existingAddressBook.SyncUrls = [existingAddressBook.SyncUrls![0]];

        string?[] newSyncUrls =
        [
            _fakers.AddressBook.GenerateOne().SyncUrls![0],
            null,
            _fakers.AddressBook.GenerateOne().SyncUrls![0]
        ];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
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
                    syncUrls = newSyncUrls
                }
            }
        };

        string route = $"/addressBooks/{existingAddressBook.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            AddressBook addressBookInDatabase = await dbContext.AddressBooks.FirstWithIdAsync(existingAddressBook.Id);

            addressBookInDatabase.Favorites.Should().NotBeEmpty();

            addressBookInDatabase.SyncUrls.Should().BeEquivalentTo(newSyncUrls, options => options.WithStrictOrdering());
        });
    }

    [Fact]
    public async Task Can_set_primitive_collection_to_null()
    {
        // Arrange
        AddressBook existingAddressBook = _fakers.AddressBook.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
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
                    syncUrls = (object?)null
                }
            }
        };

        string route = $"/addressBooks/{existingAddressBook.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            AddressBook addressBookInDatabase = await dbContext.AddressBooks.FirstWithIdAsync(existingAddressBook.Id);

            addressBookInDatabase.SyncUrls.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_set_primitive_collection_to_empty()
    {
        // Arrange
        AddressBook existingAddressBook = _fakers.AddressBook.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
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
                    syncUrls = Array.Empty<string>()
                }
            }
        };

        string route = $"/addressBooks/{existingAddressBook.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            AddressBook addressBookInDatabase = await dbContext.AddressBooks.FirstWithIdAsync(existingAddressBook.Id);

            addressBookInDatabase.SyncUrls.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_replace_members_of_compound_attribute()
    {
        // Arrange
        Contact existingContact = _fakers.Contact.GenerateOne();

        ContactName newName = _fakers.Name.GenerateOne();
        ushort? newBirthYear = _fakers.Date.GenerateOne().Year;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
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
                            firstName = newName.FirstName,
                            lastName = newName.LastName,
                            displayName = newName.DisplayName
                        },
                        emailAddresses = Array.Empty<object>(),
                        emergencyPhoneNumber = (object?)null,
                        addresses = (object?)null,
                        birthDate = new
                        {
                            year = newBirthYear
                        }
                    }
                }
            }
        };

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.Content.Name.FirstName.Should().Be(newName.FirstName);
            contactInDatabase.Content.Name.LastName.Should().Be(newName.LastName);
            contactInDatabase.Content.Name.DisplayName.Should().Be(newName.DisplayName);

            contactInDatabase.Content.Company.Should().NotBeNull();
            contactInDatabase.Content.Company.Name.Should().Be(existingContact.Content.Company!.Name);
            contactInDatabase.Content.Company.JobTitle.Should().Be(existingContact.Content.Company!.JobTitle);
            contactInDatabase.Content.Company.Department.Should().Be(existingContact.Content.Company!.Department);

            contactInDatabase.Content.EmailAddresses.Should().BeEmpty();

            contactInDatabase.Content.EmergencyPhoneNumber.Should().BeNull();

            contactInDatabase.Content.Addresses.Should().BeNull();

            contactInDatabase.Content.BirthDate.Should().NotBeNull();
            contactInDatabase.Content.BirthDate.Year.Should().Be(newBirthYear);
            contactInDatabase.Content.BirthDate.Month.Should().Be(existingContact.Content.BirthDate!.Month);
            contactInDatabase.Content.BirthDate.Day.Should().Be(existingContact.Content.BirthDate!.Day);
        });
    }

    [Fact]
    public async Task Can_set_compound_member_to_empty_object()
    {
        // Arrange
        Contact existingContact = _fakers.Contact.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
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
                        name = new object()
                    }
                }
            }
        };

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.Content.Name.FirstName.Should().Be(existingContact.Content.Name.FirstName);
            contactInDatabase.Content.Name.LastName.Should().Be(existingContact.Content.Name.LastName);
            contactInDatabase.Content.Name.DisplayName.Should().Be(existingContact.Content.Name.DisplayName);
        });
    }

    [Fact]
    public async Task Can_replace_compound_collection()
    {
        // Arrange
        Contact existingContact = _fakers.Contact.GenerateOne();
        existingContact.Content.PhoneNumbers = null;

        ContactPhoneNumber newPhoneNumber = _fakers.PhoneNumber.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
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
                        phoneNumbers = new[]
                        {
                            newPhoneNumber,
                            // TODO: EF Core crashes on NULL when using owned entities in a collection
                            new object()
                        }
                    }
                }
            }
        };

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);
            var emptyPhoneNumber = Activator.CreateInstance<ContactPhoneNumber>();

            contactInDatabase.Content.PhoneNumbers.Should().HaveCount(2);
            contactInDatabase.Content.PhoneNumbers[0].Should().BeEquivalentTo(newPhoneNumber);
            contactInDatabase.Content.PhoneNumbers[1].Should().BeEquivalentTo(emptyPhoneNumber);
        });
    }
}
