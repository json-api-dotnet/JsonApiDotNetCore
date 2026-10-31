using System.Net;
using System.Text.Json.Nodes;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

public sealed class CompoundAttributesChangeDetectionTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private static readonly SqlCommandCounter CommandCounter = new();

    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;
    private readonly CompoundAttributesFakers _fakers = new();

    public CompoundAttributesChangeDetectionTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
        testContext.UseController<ContactNotesController>();
        testContext.UseController<OperationsController>();

        testContext.ConfigureDbContext(options => options.AddInterceptors(CommandCounter));
    }

    [Fact]
    public async Task Can_patch_identical_compound_without_update()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, CompoundAttributesSamples.CreateIdenticalPatchContent());

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        CommandCounter.UpdateCount.Should().Be(0);
    }

    [Fact]
    public async Task Can_patch_changed_nested_leaf()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        JsonObject content = CompoundAttributesSamples.CreateIdenticalPatchContent();
        content["audit"]!["createdBy"] = "someone-else";

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, content);

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        CommandCounter.UpdateCount.Should().Be(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.Content.Audit.Should().NotBeNull();
            contactInDatabase.Content.Audit.CreatedBy.Should().Be("someone-else");
        });
    }

    [Fact]
    public async Task Can_patch_same_members_in_different_json_order_without_update()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        JsonObject content = CompoundAttributesSamples.CreateIdenticalPatchContent();
        JsonObject reorderedContent = Reorder(content);

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, reorderedContent);

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        reorderedContent.First().Key.Should().NotBe(content.First().Key);
        CommandCounter.UpdateCount.Should().Be(0);
    }

    [Fact]
    public async Task Can_patch_reordered_collection()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        JsonObject content = CompoundAttributesSamples.CreateIdenticalPatchContent();
        var phoneNumbers = (JsonArray)content["phoneNumbers"]!;
        JsonNode first = phoneNumbers[0]!.DeepClone();
        JsonNode second = phoneNumbers[1]!.DeepClone();

        content["phoneNumbers"] = new JsonArray(second, first);

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, content);

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        CommandCounter.UpdateCount.Should().Be(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.Content.PhoneNumbers.Select(phone => phone.Number).Should().Equal("555-2", "555-1");
        });
    }

    [Fact]
    public async Task Can_patch_null_to_empty_collection()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        existingContact.Content.Relations = null;
        await SeedAsync(existingContact);

        JsonObject content = CompoundAttributesSamples.CreateIdenticalPatchContent();
        content["relations"] = new JsonArray();

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, content);

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        CommandCounter.UpdateCount.Should().Be(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.Content.Relations.Should().NotBeNull();
            contactInDatabase.Content.Relations.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_patch_identical_scalar_collections_without_update()
    {
        // Arrange
        AddressBook existingAddressBook = _fakers.AddressBook.Generate();
        await SeedAsync(existingAddressBook);

        var requestBody = new
        {
            data = new
            {
                type = "addressBooks",
                id = existingAddressBook.StringId,
                attributes = new
                {
                    categories = existingAddressBook.Categories,
                    syncDeviceIds = existingAddressBook.SyncDeviceIds
                }
            }
        };

        string route = $"/addressBooks/{existingAddressBook.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        CommandCounter.UpdateCount.Should().Be(0);
    }

    [Fact]
    public async Task Can_patch_identical_time_span_collection_without_update()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        JsonObject content = CompoundAttributesSamples.CreateIdenticalPatchContent();
        content["company"]!["businessHours"] = new JsonArray("09:00:00", "17:00:00");

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, content);

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        CommandCounter.UpdateCount.Should().Be(0);
    }

    [Fact]
    public async Task Can_patch_identical_generic_compound_collection_without_update()
    {
        // Arrange
        AddressBook existingAddressBook = _fakers.AddressBook.Generate();
        existingAddressBook.Favorites = [CompoundAttributesSamples.CreateContact().Content];
        await SeedAsync(existingAddressBook);

        var requestBody = new
        {
            data = new
            {
                type = "addressBooks",
                id = existingAddressBook.StringId,
                attributes = new
                {
                    favorites = new[]
                    {
                        CompoundAttributesSamples.CreateIdenticalPatchContent()
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

        CommandCounter.UpdateCount.Should().Be(0);
    }

    [Fact]
    public async Task Can_patch_identical_nullable_struct_compound_without_update()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();

        existingContact.LastKnownLocation = new GeoPoint
        {
            Latitude = 52.09,
            Longitude = 5.12
        };

        await SeedAsync(existingContact);

        object requestBody = CreateLocationPatchBody(existingContact.StringId!, new
        {
            latitude = 52.09,
            longitude = 5.12
        });

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        CommandCounter.UpdateCount.Should().Be(0);
    }

    [Fact]
    public async Task Can_patch_changed_nullable_struct_compound()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();

        existingContact.LastKnownLocation = new GeoPoint
        {
            Latitude = 52.09,
            Longitude = 5.12
        };

        await SeedAsync(existingContact);

        object requestBody = CreateLocationPatchBody(existingContact.StringId!, new
        {
            latitude = 52.09,
            longitude = 6.0
        });

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        CommandCounter.UpdateCount.Should().Be(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.LastKnownLocation.Should().NotBeNull();
            contactInDatabase.LastKnownLocation.Value.Longitude.Should().Be(6.0);
        });
    }

    [Fact]
    public async Task Can_patch_null_to_nullable_struct_compound()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        existingContact.LastKnownLocation = null;
        await SeedAsync(existingContact);

        object requestBody = CreateLocationPatchBody(existingContact.StringId!, new
        {
            latitude = 0.0,
            longitude = 0.0
        });

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        CommandCounter.UpdateCount.Should().Be(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.LastKnownLocation.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task Can_patch_null_to_compound()
    {
        // Arrange
        AddressBook existingAddressBook = _fakers.AddressBook.Generate();
        existingAddressBook.EmergencyContact = null;
        await SeedAsync(existingAddressBook);

        var requestBody = new
        {
            data = new
            {
                type = "addressBooks",
                id = existingAddressBook.StringId,
                attributes = new
                {
                    emergencyContact = CompoundAttributesSamples.CreateIdenticalPatchContent()
                }
            }
        };

        string route = $"/addressBooks/{existingAddressBook.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        CommandCounter.UpdateCount.Should().Be(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            AddressBook addressBookInDatabase = await dbContext.AddressBooks.FirstWithIdAsync(existingAddressBook.Id);

            addressBookInDatabase.EmergencyContact.Should().NotBeNull();
            addressBookInDatabase.EmergencyContact.Name.LastName.Should().Be("Lovelace");
        });
    }

    [Fact]
    public async Task Can_patch_changed_element_in_compound_collection()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        JsonObject content = CompoundAttributesSamples.CreateIdenticalPatchContent();
        content["phoneNumbers"]![1]!["number"] = "555-3";

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, content);

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        CommandCounter.UpdateCount.Should().Be(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.Content.PhoneNumbers.Select(phone => phone.Number).Should().Equal("555-1", "555-3");
        });
    }

    [Fact]
    public async Task Can_update_atomically_with_identical_compound_and_get_empty_result()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "contacts",
                        id = existingContact.StringId,
                        attributes = new
                        {
                            content = CompoundAttributesSamples.CreateIdenticalPatchContent()
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        CommandCounter.UpdateCount.Should().Be(0);
    }

    private async Task SeedAsync(Contact contact)
    {
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        CommandCounter.Reset();
    }

    private async Task SeedAsync(AddressBook addressBook)
    {
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(addressBook);
            await dbContext.SaveChangesAsync();
        });

        CommandCounter.Reset();
    }

    private static object CreateLocationPatchBody(string id, object lastKnownLocation)
    {
        return new
        {
            data = new
            {
                type = "contacts",
                id,
                attributes = new
                {
                    lastKnownLocation
                }
            }
        };
    }

    private static JsonObject Reorder(JsonObject source)
    {
        var result = new JsonObject();

        foreach ((string key, JsonNode? value) in source.Reverse())
        {
            result[key] = value is JsonObject nested ? Reorder(nested) : value?.DeepClone();
        }

        return result;
    }
}
