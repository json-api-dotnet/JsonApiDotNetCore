using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

public sealed class CompoundAttributesCreateTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;
    private readonly CompoundAttributesFakers _fakers = new();

    public CompoundAttributesCreateTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
        testContext.UseController<ContactNotesController>();
    }

    [Fact]
    public async Task Can_create_resource_with_all_shapes()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreatePostContent();
        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        // Everything that was sent is returned, except for members that are blocked from viewing. Additionally, the read-only member is calculated.
        JsonObject expected = CompoundAttributesSamples.ParseObject(CompoundAttributesSamples.ExpectedContentJson);
        ((JsonObject)expected["name"]!)["nickName"] = null;
        ((JsonObject)expected["name"]!)["maidenName"] = "Byron";

        responseDocument.Data.SingleValue.GetAttribute("content").ShouldBeJson(expected);

        long newContactId = long.Parse(responseDocument.Data.SingleValue.Id!);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(newContactId);

            contactInDatabase.Content.Name.MaidenName.Should().Be("Byron");
            contactInDatabase.Content.Name.NickName.Should().BeNull();
            contactInDatabase.Content.Name.SecretAlias.Should().Be("secret");
            contactInDatabase.Content.SecurityPin.Should().Be("1234");
            contactInDatabase.Content.Company.Should().NotBeNull();
            contactInDatabase.Content.Company.RegistrationNumber.Should().Be("REG-1");
            contactInDatabase.Content.Company.BusinessHours.Should().Equal(TimeSpan.FromHours(9), TimeSpan.FromHours(17));
            contactInDatabase.Content.PhoneNumbers.Should().HaveCount(2);
            contactInDatabase.Content.PhoneNumbers[0].Kind.Should().Be(PhoneKind.Mobile);
            contactInDatabase.Content.PhoneNumbers[1].Kind.Should().BeNull();
            contactInDatabase.Content.Addresses.Should().HaveCount(1);
            contactInDatabase.Content.Addresses[0].IsoCountryCode.Should().Be("NL");
            contactInDatabase.Content.Addresses[0].Line2.Should().Be("Apt 2");
            contactInDatabase.Content.Ratings.Should().Equal(5, null, 3);
            contactInDatabase.Content.ImportantDates.Should().HaveCount(1);
            contactInDatabase.Content.ImportantDates[0].Value.Should().Be(new DateOnly(1843, 7, 10));
            contactInDatabase.Content.PreferredContactMethods.Should().Equal(ContactMethod.Email, ContactMethod.Phone);
            contactInDatabase.Content.CreditLimits.Should().Equal(100.5m, 2000m);
            contactInDatabase.Content.Anniversaries.Should().Equal(new DateTime(2020, 1, 2, 0, 0, 0, DateTimeKind.Utc));
            contactInDatabase.Content.Audit.Should().NotBeNull();
            contactInDatabase.Content.Audit.CreatedBy.Should().Be("admin");
        });
    }

    [Fact]
    public async Task Can_create_resource_with_only_required_members()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        responseDocument.Data.SingleValue.GetAttribute("content").ShouldBeJson("""
            {
              "name": {
                "firstName": null,
                "middleName": null,
                "lastName": "Doe",
                "displayName": "Doe",
                "nickName": null,
                "maidenName": null,
                "phoneticName": null
              },
              "company": null,
              "phoneNumbers": [],
              "addresses": [],
              "birthDate": null,
              "ratings": null,
              "emailAddresses": [],
              "relations": null,
              "importantDates": [],
              "preferredContactMethods": [],
              "creditLimits": [],
              "anniversaries": [],
              "internalNotes": null,
              "audit": null
            }
            """);

        long newContactId = long.Parse(responseDocument.Data.SingleValue.Id!);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(newContactId);

            contactInDatabase.Content.Name.LastName.Should().Be("Doe");
            contactInDatabase.Content.Company.Should().BeNull();
            contactInDatabase.Content.PhoneNumbers.Should().BeEmpty();
            contactInDatabase.Content.Relations.Should().BeNull();
            contactInDatabase.Content.Ratings.Should().BeNull();
            contactInDatabase.Content.Audit.Should().BeNull();
        });
    }

    [Theory]
    [InlineData("""{ "ratings": [ 1, null, 3 ] }""", "ratings")]
    [InlineData("""{ "company": { "name": "X", "businessHours": [ "01:02:03", "23:59:59" ] } }""", "company.businessHours")]
    [InlineData("""{ "company": { "name": "X", "departments": [ "A", "B" ] } }""", "company.departments")]
    [InlineData("""{ "importantDates": [ { "value": "1999-12-31", "label": "x" } ] }""", "importantDates")]
    [InlineData("""{ "preferredContactMethods": [ 2, 0, 1 ] }""", "preferredContactMethods")]
    [InlineData("""{ "creditLimits": [ 0.01, 99999.99 ] }""", "creditLimits")]
    [InlineData("""{ "anniversaries": [ "2001-02-03T04:05:06Z" ] }""", "anniversaries")]
    public async Task Can_create_scalar_collections_per_element_type(string contentFragment, string path)
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();
        JsonObject fragment = CompoundAttributesSamples.ParseObject(contentFragment);

        foreach ((string key, JsonNode? value) in fragment.ToArray())
        {
            fragment.Remove(key);
            content[key] = value;
        }

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        JsonNode? expected = GetNodeAtPath(content, path);
        responseDocument.Data.SingleValue.GetAttribute("content", path).ShouldBeJson(expected);

        (HttpResponseMessage getResponse, Document getDocument) =
            await _testContext.ExecuteGetAsync<Document>($"/contacts/{responseDocument.Data.SingleValue.Id}");

        getResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        getDocument.Data.SingleValue.Should().NotBeNull();
        getDocument.Data.SingleValue.GetAttribute("content", path).ShouldBeJson(expected);
    }

    [Fact]
    public async Task Can_create_top_level_scalar_collections()
    {
        // Arrange
        Guid[] syncDeviceIds =
        [
            Guid.NewGuid(),
            Guid.NewGuid()
        ];

        var requestBody = new
        {
            data = new
            {
                type = "addressBooks",
                attributes = new
                {
                    name = "Work",
                    categories = new[]
                    {
                        "family",
                        "work"
                    },
                    syncDeviceIds
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/addressBooks", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.GetAttribute("categories").ShouldBeJson("""["family", "work"]""");
        responseDocument.Data.SingleValue.GetAttribute("syncDeviceIds").ShouldBeJson(JsonSerializer.SerializeToNode(syncDeviceIds));

        long newAddressBookId = long.Parse(responseDocument.Data.SingleValue.Id!);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            AddressBook addressBookInDatabase = await dbContext.AddressBooks.FirstWithIdAsync(newAddressBookId);

            addressBookInDatabase.Categories.Should().Equal("family", "work");
            addressBookInDatabase.SyncDeviceIds.Should().Equal(syncDeviceIds);
        });
    }

    [Fact]
    public async Task Can_create_generic_compounds()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();

        content["emailAddresses"] = JsonNode.Parse("""[ { "value": "a@b.c", "label": "one" }, { "value": "d@e.f", "label": null } ]""");
        content["importantDates"] = JsonNode.Parse("""[ { "value": "2024-02-29", "label": "leap" } ]""");

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.GetAttribute("content", "emailAddresses").ShouldBeJson(content["emailAddresses"]);
        responseDocument.Data.SingleValue.GetAttribute("content", "importantDates").ShouldBeJson(content["importantDates"]);

        long newContactId = long.Parse(responseDocument.Data.SingleValue.Id!);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(newContactId);

            contactInDatabase.Content.EmailAddresses.Should().HaveCount(2);
            contactInDatabase.Content.EmailAddresses[1].Value.Should().Be("d@e.f");
            contactInDatabase.Content.EmailAddresses[1].Label.Should().BeNull();
            contactInDatabase.Content.ImportantDates.Should().ContainSingle().Which.Value.Should().Be(new DateOnly(2024, 2, 29));
        });
    }

    [Fact]
    public async Task Can_create_with_custom_public_names()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();

        content["addresses"] = JsonNode.Parse("""
            [ { "isoCountryCode": "US", "city": "Metropolis", "zipCode": "12345", "Address-Line2": "Suite 5" } ]
            """);

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        long newContactId = long.Parse(responseDocument.Data.SingleValue.Id!);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(newContactId);

            contactInDatabase.Content.Addresses.Should().ContainSingle();
            contactInDatabase.Content.Addresses[0].PostalCode.Should().Be("12345");
            contactInDatabase.Content.Addresses[0].Line2.Should().Be("Suite 5");
        });
    }

    [Fact]
    public async Task Can_create_with_init_only_and_private_setters()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();

        content["addresses"] = JsonNode.Parse("""[ { "isoCountryCode": "BE", "city": "Brussels" } ]""");
        content["company"] = JsonNode.Parse("""{ "name": "Acme", "registrationNumber": "REG-42" }""");

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        long newContactId = long.Parse(responseDocument.Data.SingleValue.Id!);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(newContactId);

            contactInDatabase.Content.Addresses.Should().ContainSingle().Which.IsoCountryCode.Should().Be("BE");
            contactInDatabase.Content.Company.Should().NotBeNull();
            contactInDatabase.Content.Company.RegistrationNumber.Should().Be("REG-42");
        });
    }

    [Fact]
    public async Task Can_create_at_depth_four()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "addressBooks",
                attributes = new
                {
                    name = "Deep",
                    favorites = new[]
                    {
                        new
                        {
                            name = new
                            {
                                lastName = "Doe"
                            },
                            phoneNumbers = new[]
                            {
                                new
                                {
                                    number = "555-0001"
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/addressBooks", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.GetAttribute("favorites", "0.phoneNumbers.0.number").ShouldBeJson("\"555-0001\"");

        long newAddressBookId = long.Parse(responseDocument.Data.SingleValue.Id!);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            AddressBook addressBookInDatabase = await dbContext.AddressBooks.FirstWithIdAsync(newAddressBookId);

            addressBookInDatabase.Favorites.Should().ContainSingle();
            addressBookInDatabase.Favorites[0].PhoneNumbers.Should().ContainSingle().Which.Number.Should().Be("555-0001");
        });
    }

    [Fact]
    public async Task Can_create_with_explicit_null()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();

        content["company"] = null;
        content["birthDate"] = null;
        content["ratings"] = null;
        content["relations"] = null;
        content["audit"] = null;

        object contactRequestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        var addressBookRequestBody = new
        {
            data = new
            {
                type = "addressBooks",
                attributes = new Dictionary<string, object?>
                {
                    ["name"] = "Nulls",
                    ["emergencyContact"] = null,
                    ["categories"] = null,
                    ["syncDeviceIds"] = null
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse1, Document responseDocument1) = await _testContext.ExecutePostAsync<Document>("/contacts", contactRequestBody);

        (HttpResponseMessage httpResponse2, Document responseDocument2) =
            await _testContext.ExecutePostAsync<Document>("/addressBooks", addressBookRequestBody);

        // Assert
        httpResponse1.ShouldHaveStatusCode(HttpStatusCode.Created);
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument1.Data.SingleValue.Should().NotBeNull();
        responseDocument1.Data.SingleValue.GetAttribute("content", "company").Should().BeNull();
        responseDocument1.Data.SingleValue.GetAttribute("content", "birthDate").Should().BeNull();
        responseDocument1.Data.SingleValue.GetAttribute("content", "ratings").Should().BeNull();
        responseDocument1.Data.SingleValue.GetAttribute("content", "relations").Should().BeNull();
        responseDocument1.Data.SingleValue.GetAttribute("content", "audit").Should().BeNull();

        responseDocument2.Data.SingleValue.Should().NotBeNull();
        responseDocument2.Data.SingleValue.Attributes.Should().ContainKey("emergencyContact");
        responseDocument2.Data.SingleValue.GetAttribute("emergencyContact").Should().BeNull();
        responseDocument2.Data.SingleValue.GetAttribute("categories").Should().BeNull();
        responseDocument2.Data.SingleValue.GetAttribute("syncDeviceIds").Should().BeNull();
    }

    [Fact]
    public async Task Can_create_with_empty_collections()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();

        content["phoneNumbers"] = new JsonArray();
        content["relations"] = new JsonArray();
        content["ratings"] = new JsonArray();
        content["creditLimits"] = new JsonArray();

        object contactRequestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        var addressBookRequestBody = new
        {
            data = new
            {
                type = "addressBooks",
                attributes = new
                {
                    name = "Empty",
                    categories = Array.Empty<string>(),
                    favorites = Array.Empty<object>()
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse1, Document responseDocument1) = await _testContext.ExecutePostAsync<Document>("/contacts", contactRequestBody);

        (HttpResponseMessage httpResponse2, Document responseDocument2) =
            await _testContext.ExecutePostAsync<Document>("/addressBooks", addressBookRequestBody);

        // Assert
        httpResponse1.ShouldHaveStatusCode(HttpStatusCode.Created);
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument1.Data.SingleValue.Should().NotBeNull();
        responseDocument1.Data.SingleValue.GetAttribute("content", "phoneNumbers").ShouldBeJson("[]");
        responseDocument1.Data.SingleValue.GetAttribute("content", "relations").ShouldBeJson("[]");
        responseDocument1.Data.SingleValue.GetAttribute("content", "ratings").ShouldBeJson("[]");
        responseDocument1.Data.SingleValue.GetAttribute("content", "creditLimits").ShouldBeJson("[]");

        responseDocument2.Data.SingleValue.Should().NotBeNull();
        responseDocument2.Data.SingleValue.GetAttribute("categories").ShouldBeJson("[]");
        responseDocument2.Data.SingleValue.GetAttribute("favorites").ShouldBeJson("[]");

        long newContactId = long.Parse(responseDocument1.Data.SingleValue.Id!);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(newContactId);

            contactInDatabase.Content.Relations.Should().NotBeNull();
            contactInDatabase.Content.Relations.Should().BeEmpty();
            contactInDatabase.Content.Ratings.Should().NotBeNull();
            contactInDatabase.Content.Ratings.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_create_with_relationships_and_compounds()
    {
        // Arrange
        AddressBook existingAddressBook = _fakers.AddressBook.Generate();

        ContactNote existingNote = new()
        {
            Text = "Remember",
            Priority = 1
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            await dbContext.ClearTableAsync<ContactNote>();
            dbContext.AddressBooks.Add(existingAddressBook);
            dbContext.Notes.Add(existingNote);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                attributes = new
                {
                    content = CompoundAttributesSamples.CreateMinimalContent(),
                    remarks = "With relationships"
                },
                relationships = new
                {
                    addressBook = new
                    {
                        data = new
                        {
                            type = "addressBooks",
                            id = existingAddressBook.StringId
                        }
                    },
                    notes = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "contactNotes",
                                id = existingNote.StringId
                            }
                        }
                    }
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        long newContactId = long.Parse(responseDocument.Data.SingleValue.Id!);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.Include(contact => contact.AddressBook).Include(contact => contact.Notes)
                .FirstWithIdAsync(newContactId);

            contactInDatabase.Remarks.Should().Be("With relationships");
            contactInDatabase.Content.Name.LastName.Should().Be("Doe");
            contactInDatabase.AddressBook.Should().NotBeNull();
            contactInDatabase.AddressBook.Id.Should().Be(existingAddressBook.Id);
            contactInDatabase.Notes.Should().ContainSingle().Which.Id.Should().Be(existingNote.Id);
        });
    }

    [Fact]
    public async Task Can_create_with_view_blocked_member()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();

        content["securityPin"] = "4321";
        ((JsonObject)content["name"]!)["secretAlias"] = "hidden";

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(content);

        // Act
        (HttpResponseMessage httpResponse, string responseText) = await _testContext.ExecutePostAsync<string>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseText.Should().NotContain("securityPin");
        responseText.Should().NotContain("secretAlias");
        responseText.Should().NotContain("4321");
        responseText.Should().NotContain("hidden");

        long newContactId = long.Parse(JsonNode.Parse(responseText)!["data"]!["id"]!.GetValue<string>());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(newContactId);

            contactInDatabase.Content.SecurityPin.Should().Be("4321");
            contactInDatabase.Content.Name.SecretAlias.Should().Be("hidden");
        });
    }

    [Fact(Skip = "KNOWN-ISSUE D5: Nullable<struct> compound fails with 500 'Failed to instantiate compound type Nullable`1' on write.")]
    public async Task Can_create_with_struct_compound()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "contacts",
                attributes = new
                {
                    content = CompoundAttributesSamples.CreateMinimalContent(),
                    lastKnownLocation = new
                    {
                        latitude = 52.09,
                        longitude = 5.12
                    }
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.GetAttribute("lastKnownLocation").ShouldBeJson("""{ "latitude": 52.09, "longitude": 5.12 }""");

        long newContactId = long.Parse(responseDocument.Data.SingleValue.Id!);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(newContactId);

            contactInDatabase.LastKnownLocation.Should().NotBeNull();
            contactInDatabase.LastKnownLocation.Value.Latitude.Should().Be(52.09);
            contactInDatabase.LastKnownLocation.Value.Longitude.Should().Be(5.12);
        });
    }

    [Fact]
    public async Task Returns_Location_and_self_link()
    {
        // Arrange
        object requestBody = CompoundAttributesSamples.CreateContactPostBody(CompoundAttributesSamples.CreateMinimalContent());

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        string newContactId = responseDocument.Data.SingleValue.Id!;

        httpResponse.Headers.Location.Should().Be($"http://localhost/contacts/{newContactId}");
        responseDocument.Data.SingleValue.Links.Should().NotBeNull();
        responseDocument.Data.SingleValue.Links.Self.Should().Be($"http://localhost/contacts/{newContactId}");
    }

    private static JsonNode? GetNodeAtPath(JsonNode root, string path)
    {
        JsonNode? node = root;

        foreach (string segment in path.Split('.'))
        {
            node = node![segment];
        }

        return node;
    }
}
