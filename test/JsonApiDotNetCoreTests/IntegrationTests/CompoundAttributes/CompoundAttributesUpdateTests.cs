using System.Net;
using System.Text.Json.Nodes;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

public sealed class CompoundAttributesUpdateTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;
    private readonly CompoundAttributesFakers _fakers = new();

    public CompoundAttributesUpdateTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
        testContext.UseController<ContactNotesController>();
    }

    [Fact]
    public async Task PATCH_replaces_compound_wholesale()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(existingContact);
            await dbContext.SaveChangesAsync();
        });

        // The request contains only the last name. Because the whole compound is replaced, all other members are reset.
        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, CompoundAttributesSamples.CreateMinimalContent());

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.Remarks.Should().Be("remarks");
            contactInDatabase.Content.Name.LastName.Should().Be("Doe");
            contactInDatabase.Content.Name.FirstName.Should().BeNull();
            contactInDatabase.Content.Name.NickName.Should().BeNull();
            contactInDatabase.Content.Name.SecretAlias.Should().BeNull();
            contactInDatabase.Content.Company.Should().BeNull();
            contactInDatabase.Content.BirthDate.Should().BeNull();
            contactInDatabase.Content.SecurityPin.Should().BeNull();
            contactInDatabase.Content.Ratings.Should().BeNull();
            contactInDatabase.Content.Relations.Should().BeNull();
            contactInDatabase.Content.Audit.Should().BeNull();
            contactInDatabase.Content.PhoneNumbers.Should().BeEmpty();
            contactInDatabase.Content.Addresses.Should().BeEmpty();
            contactInDatabase.Content.EmailAddresses.Should().BeEmpty();
            contactInDatabase.Content.PreferredContactMethods.Should().BeEmpty();
            contactInDatabase.Content.CreditLimits.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task PATCH_leaves_omitted_attributes_untouched()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();

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
                    remarks = "Only the remarks changed"
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

            contactInDatabase.Remarks.Should().Be("Only the remarks changed");
            contactInDatabase.Content.Name.LastName.Should().Be("Lovelace");
            contactInDatabase.Content.Name.SecretAlias.Should().Be("secret");
            contactInDatabase.Content.SecurityPin.Should().Be("1234");
            contactInDatabase.Content.PhoneNumbers.Should().HaveCount(2);
        });
    }

    [Fact]
    public async Task PATCH_sets_nullable_compound_to_null()
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
            addressBookInDatabase.Favorites.Should().HaveCount(2);
        });

        // Act
        (HttpResponseMessage getResponse, Document getDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        getResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        getDocument.Data.SingleValue.Should().NotBeNull();
        getDocument.Data.SingleValue.Attributes.Should().ContainKey("emergencyContact");
        getDocument.Data.SingleValue.GetAttribute("emergencyContact").Should().BeNull();
    }

    [Fact]
    public async Task PATCH_replaces_compound_collection()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(existingContact);
            await dbContext.SaveChangesAsync();
        });

        JsonObject content = CompoundAttributesSamples.CreateIdenticalPatchContent();

        content["phoneNumbers"] = JsonNode.Parse("""
            [
              { "number": "3", "label": "c" },
              { "number": "1", "label": "a" },
              { "number": "2", "label": "b", "kind": 2 }
            ]
            """);

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, content);

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.Content.PhoneNumbers.Select(phone => phone.Number).Should().Equal("3", "1", "2");
            contactInDatabase.Content.PhoneNumbers[2].Kind.Should().Be(PhoneKind.Fax);
            contactInDatabase.Content.PhoneNumbers[0].CountryPrefix.Should().BeNull();
            contactInDatabase.Content.Name.LastName.Should().Be("Lovelace");
        });
    }

    [Fact]
    public async Task PATCH_clears_compound_collection()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(existingContact);
            await dbContext.SaveChangesAsync();
        });

        JsonObject content = CompoundAttributesSamples.CreateIdenticalPatchContent();
        content["phoneNumbers"] = new JsonArray();

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, content);

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
            contactInDatabase.Content.Addresses.Should().HaveCount(1);
        });
    }

    [Fact]
    public async Task PATCH_replaces_scalar_collection()
    {
        // Arrange
        AddressBook existingAddressBook = _fakers.AddressBook.Generate();

        var newDeviceId = Guid.NewGuid();

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
                        "new1",
                        "new2",
                        "new3"
                    },
                    syncDeviceIds = new[]
                    {
                        newDeviceId
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

            addressBookInDatabase.Categories.Should().Equal("new1", "new2", "new3");
            addressBookInDatabase.SyncDeviceIds.Should().Equal(newDeviceId);
        });
    }

    [Fact]
    public async Task PATCH_changes_compound_and_scalar_together()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(existingContact);
            await dbContext.SaveChangesAsync();
        });

        JsonObject content = CompoundAttributesSamples.CreateIdenticalPatchContent();
        ((JsonObject)content["name"]!)["lastName"] = "King";

        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                id = existingContact.StringId,
                attributes = new
                {
                    remarks = "Changed remarks",
                    content
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

            contactInDatabase.Remarks.Should().Be("Changed remarks");
            contactInDatabase.Content.Name.LastName.Should().Be("King");
            contactInDatabase.Content.Name.DisplayName.Should().Be("Ada M King");
        });
    }

    [Fact]
    public async Task PATCH_changes_relationship_and_leaves_compound_intact()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        AddressBook existingAddressBook = _fakers.AddressBook.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.Contacts.Add(existingContact);
            dbContext.AddressBooks.Add(existingAddressBook);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                id = existingContact.StringId,
                relationships = new
                {
                    addressBook = new
                    {
                        data = new
                        {
                            type = "addressBooks",
                            id = existingAddressBook.StringId
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
            Contact contactInDatabase = await dbContext.Contacts.Include(contact => contact.AddressBook).FirstWithIdAsync(existingContact.Id);

            contactInDatabase.AddressBook.Should().NotBeNull();
            contactInDatabase.AddressBook.Id.Should().Be(existingAddressBook.Id);
            contactInDatabase.Content.Name.LastName.Should().Be("Lovelace");
            contactInDatabase.Content.Name.SecretAlias.Should().Be("secret");
            contactInDatabase.Content.PhoneNumbers.Should().HaveCount(2);
            contactInDatabase.Content.Company.Should().NotBeNull();
        });
    }

    [Fact(Skip = "KNOWN-ISSUE D5: Nullable<struct> compound fails with 500 'Failed to instantiate compound type Nullable`1' on write.")]
    public async Task PATCH_with_struct_compound()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();

        existingContact.LastKnownLocation = new GeoPoint
        {
            Latitude = 1,
            Longitude = 2
        };

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
                    lastKnownLocation = new
                    {
                        latitude = 3.5,
                        longitude = 4.5
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

            contactInDatabase.LastKnownLocation.Should().NotBeNull();
            contactInDatabase.LastKnownLocation.Value.Latitude.Should().Be(3.5);
            contactInDatabase.LastKnownLocation.Value.Longitude.Should().Be(4.5);
        });
    }

    [Fact]
    public async Task PATCH_setting_non_nullable_compound_to_null_fails()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(existingContact);
            await dbContext.SaveChangesAsync();
        });

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, null);

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The Content field is required.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/content");

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.Content.Should().NotBeNull();
            contactInDatabase.Content.Name.LastName.Should().Be("Lovelace");
        });
    }
}
