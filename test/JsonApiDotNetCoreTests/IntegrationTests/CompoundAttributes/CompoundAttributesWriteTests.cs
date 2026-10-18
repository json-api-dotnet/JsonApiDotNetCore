using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
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
                            lastName = "Doe"
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
                    categories = new[]
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
            addressBookInDatabase.Categories.Should().NotBeNull();
            addressBookInDatabase.Categories.Should().BeEquivalentTo("https://example.com/sync", "https://backup.example.com");
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
                            lastName = "Smith"
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

        existingAddressBook.Categories =
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
                    categories = new[]
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

            addressBookInDatabase.Categories.Should().BeEquivalentTo("https://new1.com");
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
                            lastName = existingContact.Content.Name.LastName
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

    [Fact]
    public async Task Can_patch_compound_property_with_blocked_AllowCreate()
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
                            lastName = existingContact.Content.Name.LastName,
                            nickName = "NewNickName"
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
            contactInDatabase.Content.Name.NickName.Should().Be("NewNickName");
        });
    }

    [Fact]
    public async Task Can_create_resource_with_blocked_AllowChange_compound_property()
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
                            firstName = "Alice",
                            lastName = "Wonderland",
                            maidenName = "OriginalFamily"
                        }
                    }
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
            contactInDatabase.Content.Name.MaidenName.Should().Be("OriginalFamily");
        });
    }

    [Fact]
    public async Task Can_create_and_patch_resource_with_custom_public_name_zipCode()
    {
        // Arrange: Create with zipCode
        var createRequestBody = new
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
                            firstName = "Bob",
                            lastName = "Builder"
                        },
                        addresses = new[]
                        {
                            new
                            {
                                isoCountryCode = "US",
                                city = "Metropolis",
                                zipCode = "12345"
                            }
                        }
                    }
                }
            }
        };

        const string postRoute = "/contacts";

        // Act: Create
        (HttpResponseMessage postResponse, Document postDocument) = await _testContext.ExecutePostAsync<Document>(postRoute, createRequestBody);

        // Assert: Create
        postResponse.ShouldHaveStatusCode(HttpStatusCode.Created);
        postDocument.Data.SingleValue.Should().NotBeNull();
        string newContactId = postDocument.Data.SingleValue!.Id!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(long.Parse(newContactId));
            contactInDatabase.Content.Addresses.Should().HaveCount(1);
            contactInDatabase.Content.Addresses[0].PostalCode.Should().Be("12345");
        });

        // Act: Patch with new zipCode
        var patchRequestBody = new
        {
            data = new
            {
                type = "contacts",
                id = newContactId,
                attributes = new
                {
                    content = new
                    {
                        name = new
                        {
                            lastName = "Builder"
                        },
                        addresses = new[]
                        {
                            new
                            {
                                isoCountryCode = "US",
                                city = "Metropolis",
                                zipCode = "99999"
                            }
                        }
                    }
                }
            }
        };

        string patchRoute = $"/contacts/{newContactId}";
        (HttpResponseMessage patchResponse, string patchDocument) = await _testContext.ExecutePatchAsync<string>(patchRoute, patchRequestBody);

        // Assert: Patch
        patchResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        patchDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(long.Parse(newContactId));
            contactInDatabase.Content.Addresses[0].PostalCode.Should().Be("99999");
        });

        // Act: Read with sparse fieldset on zipCode
        string getRoute = $"/contacts/{newContactId}?fields[contacts]=content.addresses.zipCode";
        (HttpResponseMessage getResponse, string getRaw) = await _testContext.ExecuteGetAsync<string>(getRoute);

        // Assert: Read
        getResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        getRaw.Should().Contain("zipCode");
        getRaw.Should().Contain("99999");
    }

    [Fact]
    public async Task Can_create_resource_with_unknown_compound_property_when_allowed()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.App.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownFieldsInRequestBody = true;

        try
        {
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
                                firstName = "Extra",
                                lastName = "Tester",
                                unknownNestedField = "Ignored"
                            }
                        }
                    }
                }
            };

            const string route = "/contacts";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);
            responseDocument.Data.SingleValue.Should().NotBeNull();
        }
        finally
        {
            options.AllowUnknownFieldsInRequestBody = false;
        }
    }

    [Fact]
    public async Task Patching_resource_with_identical_compound_attributes_is_idempotent_with_no_sql_update()
    {
        // Arrange
        Contact contact = _fakers.Contact.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                id = contact.StringId,
                attributes = new
                {
                    content = new
                    {
                        name = new
                        {
                            firstName = contact.Content.Name.FirstName,
                            middleName = contact.Content.Name.MiddleName,
                            lastName = contact.Content.Name.LastName,
                            nickName = contact.Content.Name.NickName
                        },
                        company = contact.Content.Company == null ? null : new
                        {
                            name = contact.Content.Company.Name,
                            department = contact.Content.Company.Department,
                            businessHours = contact.Content.Company.BusinessHours.Select(b => b.ToString()).ToArray()
                        },
                        birthDate = contact.Content.BirthDate == null ? null : new
                        {
                            year = contact.Content.BirthDate.Year,
                            month = contact.Content.BirthDate.Month,
                            day = contact.Content.BirthDate.Day
                        },
                        phoneNumbers = contact.Content.PhoneNumbers.Select(p => new
                        {
                            countryPrefix = p.CountryPrefix,
                            number = p.Number,
                            label = p.Label
                        }).ToArray(),
                        addresses = contact.Content.Addresses.Select(a => new
                        {
                            isoCountryCode = a.IsoCountryCode,
                            city = a.City,
                            line1 = a.Line1,
                            zipCode = a.PostalCode,
                            label = a.Label
                        }).ToArray()
                    }
                }
            }
        };

        string route = $"/contacts/{contact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        responseDocument.Should().BeEmpty();
    }
}
