using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

/// <summary>
/// Fixed (non-random) sample data that exercises every shape of the address-book model, plus JSON helpers for asserting on responses.
/// </summary>
internal static class CompoundAttributesSamples
{
    /// <summary>
    /// The value of "content" as it is returned by the API for the contact produced by <see cref="CreateContact" />.
    /// </summary>
    public const string ExpectedContentJson = """
        {
          "name": {
            "firstName": "Ada",
            "middleName": "M",
            "lastName": "Lovelace",
            "displayName": "Ada M Lovelace",
            "nickName": "Countess",
            "maidenName": null,
            "phoneticName": "LUV-lace"
          },
          "company": {
            "name": "Contoso",
            "department": "R&D",
            "businessHours": [ "09:00:00", "17:00:00" ],
            "departments": [ "Sales", "Support" ],
            "registrationNumber": "REG-1"
          },
          "phoneNumbers": [
            { "countryPrefix": "+1", "number": "555-1", "label": "mobile", "kind": 0 },
            { "countryPrefix": "+31", "number": "555-2", "label": "home", "kind": null }
          ],
          "addresses": [
            {
              "isoCountryCode": "NL",
              "city": "Utrecht",
              "line1": "Main 1",
              "address-Line2": "Apt 2",
              "zipCode": "1234 AB",
              "label": "home"
            }
          ],
          "birthDate": { "year": 1815, "month": 12, "day": 10 },
          "ratings": [ 5, null, 3 ],
          "emailAddresses": [ { "value": "ada@example.com", "label": "primary" } ],
          "relations": [ { "value": "Charles", "label": "friend" } ],
          "importantDates": [ { "value": "1843-07-10", "label": "notes" } ],
          "preferredContactMethods": [ 1, 0 ],
          "creditLimits": [ 100.5, 2000 ],
          "anniversaries": [ "2020-01-02T00:00:00Z" ],
          "internalNotes": "internal",
          "audit": { "createdBy": "admin" }
        }
        """;

    /// <summary>
    /// Creates the contact that is rendered as <see cref="ExpectedContentJson" />. Members that are blocked for viewing are set, but never rendered.
    /// </summary>
    public static Contact CreateContact()
    {
        var company = new ContactCompany
        {
            Name = "Contoso",
            Department = "R&D",
            BusinessHours =
            [
                TimeSpan.FromHours(9),
                TimeSpan.FromHours(17)
            ],
            Departments =
            [
                "Sales",
                "Support"
            ]
        };

        company.SetRegistrationNumber("REG-1");

        return new Contact
        {
            Remarks = "remarks",
            Content = new ContactRoot
            {
                Name = new ContactName
                {
                    FirstName = "Ada",
                    MiddleName = "M",
                    LastName = "Lovelace",
                    NickName = "Countess",
                    SecretAlias = "secret",
                    PhoneticName = "LUV-lace"
                },
                Company = company,
                PhoneNumbers =
                [
                    new ContactPhoneNumber
                    {
                        CountryPrefix = "+1",
                        Number = "555-1",
                        Label = "mobile",
                        Kind = PhoneKind.Mobile
                    },
                    new ContactPhoneNumber
                    {
                        CountryPrefix = "+31",
                        Number = "555-2",
                        Label = "home"
                    }
                ],
                Addresses =
                [
                    new ContactAddress
                    {
                        IsoCountryCode = "NL",
                        City = "Utrecht",
                        Line1 = "Main 1",
                        Line2 = "Apt 2",
                        PostalCode = "1234 AB",
                        Label = "home"
                    }
                ],
                BirthDate = new ContactDate
                {
                    Year = 1815,
                    Month = 12,
                    Day = 10
                },
                SecurityPin = "1234",
                Ratings =
                [
                    5,
                    null,
                    3
                ],
                EmailAddresses =
                [
                    new LabeledValue<string>
                    {
                        Value = "ada@example.com",
                        Label = "primary"
                    }
                ],
                Relations =
                [
                    new LabeledValue<string>
                    {
                        Value = "Charles",
                        Label = "friend"
                    }
                ],
                ImportantDates =
                [
                    new LabeledValue<DateOnly>
                    {
                        Value = new DateOnly(1843, 7, 10),
                        Label = "notes"
                    }
                ],
                PreferredContactMethods =
                [
                    ContactMethod.Email,
                    ContactMethod.Phone
                ],
                CreditLimits =
                [
                    100.5m,
                    2000m
                ],
                Anniversaries = [new DateTime(2020, 1, 2, 0, 0, 0, DateTimeKind.Utc)],
                InternalNotes = "internal",
                Audit = new ContactAudit
                {
                    CreatedBy = "admin"
                }
            }
        };
    }

    /// <summary>
    /// Creates the request body value for "content" that, when sent in a PATCH request, is identical to the stored data of <see cref="CreateContact" />.
    /// This requires adding the members that are not rendered and removing the read-only and change-blocked members.
    /// </summary>
    public static JsonObject CreateIdenticalPatchContent()
    {
        JsonObject content = ParseObject(ExpectedContentJson);
        var name = (JsonObject)content["name"]!;

        name.Remove("displayName");
        name.Remove("maidenName");
        name["secretAlias"] = "secret";
        content["securityPin"] = "1234";

        // Output keys are camel-cased by DictionaryKeyPolicy (see D9), but input requires the exact public name.
        var address = (JsonObject)content["addresses"]![0]!;
        address.Remove("address-Line2");
        address["Address-Line2"] = "Apt 2";

        return content;
    }

    /// <summary>
    /// Creates a request body value for "content" that is valid for POST: it assigns the change-blocked member 'maidenName' and does not assign the
    /// create-blocked member 'nickName'.
    /// </summary>
    public static JsonObject CreatePostContent()
    {
        JsonObject content = CreateIdenticalPatchContent();
        var name = (JsonObject)content["name"]!;

        name.Remove("nickName");
        name["maidenName"] = "Byron";

        return content;
    }

    /// <summary>
    /// Creates a request body value for "content" that contains the least possible: only the last name.
    /// </summary>
    public static JsonObject CreateMinimalContent()
    {
        return ParseObject("""
            { "name": { "lastName": "Doe" } }
            """);
    }

    public static object CreateContactPostBody(JsonNode? content)
    {
        return new
        {
            data = new
            {
                type = "contacts",
                attributes = new
                {
                    content
                }
            }
        };
    }

    public static object CreateContactPatchBody(string id, JsonNode? content)
    {
        return new
        {
            data = new
            {
                type = "contacts",
                id,
                attributes = new
                {
                    content
                }
            }
        };
    }

    public static JsonObject ParseObject(string json)
    {
        return (JsonObject)JsonNode.Parse(json)!;
    }

    public static JsonNode? GetAttribute(this ResourceObject resource, string name)
    {
        resource.Attributes.Should().ContainKey(name);
        return JsonSerializer.SerializeToNode(resource.Attributes[name]);
    }

    public static JsonNode? GetAttribute(this ResourceObject resource, string name, string path)
    {
        JsonNode? node = resource.GetAttribute(name);

        foreach (string segment in path.Split('.'))
        {
            node.Should().NotBeNull($"segment '{segment}' of path '{path}' should exist");

            node = node is JsonArray array ? array[int.Parse(segment)] : node[segment];
        }

        return node;
    }

    public static void ShouldBeJson(this JsonNode? actual, string expectedJson)
    {
        JsonNode? expected = JsonNode.Parse(expectedJson);
        AssertJsonEqual(actual, expected);
    }

    public static void ShouldBeJson(this JsonNode? actual, JsonNode? expected)
    {
        AssertJsonEqual(actual, expected?.DeepClone());
    }

    private static void AssertJsonEqual(JsonNode? actual, JsonNode? expected)
    {
        bool isEqual = JsonNode.DeepEquals(actual, expected);
        isEqual.Should().BeTrue($"expected JSON {expected?.ToJsonString() ?? "null"} but found {actual?.ToJsonString() ?? "null"}");
    }
}
