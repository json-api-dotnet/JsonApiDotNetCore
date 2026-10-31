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
    public async Task PATCH_with_identical_compound_sends_no_update()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        object requestBody =
            CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, CompoundAttributesSamples.CreateIdenticalPatchContent());

        string route = $"/contacts/{existingContact.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        CommandCounter.UpdateCount.Should().Be(0);
    }

    [Fact]
    public async Task PATCH_with_changed_nested_leaf_updates()
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
    public async Task PATCH_with_same_members_in_different_json_order_sends_no_update()
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
    public async Task PATCH_with_reordered_collection_updates()
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
    public async Task PATCH_changing_null_to_empty_collection_updates()
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
    public async Task PATCH_with_identical_scalar_collections_sends_no_update()
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
    public async Task PATCH_with_identical_time_span_collection_sends_no_update()
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
    public async Task PATCH_with_identical_generic_compound_collection_sends_no_update()
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
    public async Task Atomic_update_with_identical_compound_returns_empty_result()
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
