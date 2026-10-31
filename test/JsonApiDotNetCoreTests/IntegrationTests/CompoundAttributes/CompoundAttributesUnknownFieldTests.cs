using System.Net;
using System.Text.Json.Nodes;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

public sealed class CompoundAttributesUnknownFieldTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;

    public CompoundAttributesUnknownFieldTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
        testContext.UseController<ContactNotesController>();

        var options = (JsonApiOptions)testContext.App.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownFieldsInRequestBody = true;
    }

    [Theory]
    [InlineData(false, "content")]
    [InlineData(false, "name")]
    [InlineData(false, "phoneNumbers")]
    [InlineData(false, "company")]
    [InlineData(true, "content")]
    [InlineData(true, "name")]
    [InlineData(true, "phoneNumbers")]
    [InlineData(true, "company")]
    public async Task Ignores_unknown_nested_member_when_allowed(bool isPatch, string depth)
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();

        switch (depth)
        {
            case "content":
            {
                content["nope"] = "x";
                break;
            }
            case "name":
            {
                ((JsonObject)content["name"]!)["nope"] = "x";
                break;
            }
            case "phoneNumbers":
            {
                content["phoneNumbers"] = JsonNode.Parse("""[ { "number": "1", "nope": "x" } ]""");
                break;
            }
            default:
            {
                content["company"] = JsonNode.Parse("""{ "name": "Acme", "nope": "x" }""");
                break;
            }
        }

        Contact? existingContact = null;

        if (isPatch)
        {
            existingContact = CompoundAttributesSamples.CreateContact();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Contact>();
                dbContext.Contacts.Add(existingContact);
                await dbContext.SaveChangesAsync();
            });
        }

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = isPatch
            ? await _testContext.ExecutePatchAsync<Document>($"/contacts/{existingContact!.StringId}",
                CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, content))
            : await _testContext.ExecutePostAsync<Document>("/contacts", CompoundAttributesSamples.CreateContactPostBody(content));

        // Assert
        httpResponse.ShouldHaveStatusCode(isPatch ? HttpStatusCode.NoContent : HttpStatusCode.Created);

        if (!isPatch)
        {
            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.GetAttribute("content", "name.lastName").ShouldBeJson("\"Doe\"");

            responseDocument.Data.SingleValue.GetAttribute("content")!.ToJsonString().Should().NotContain("nope");
        }

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            long id = isPatch ? existingContact!.Id : long.Parse(responseDocument.Data.SingleValue!.Id!);
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(id);

            contactInDatabase.Content.Name.LastName.Should().Be("Doe");
        });
    }

    [Fact]
    public async Task Ignores_unknown_member_inside_compound_collection_element()
    {
        // Arrange
        JsonObject content = CompoundAttributesSamples.CreateMinimalContent();

        content["phoneNumbers"] = JsonNode.Parse("""
            [
              { "number": "1", "label": "a" },
              { "number": "2", "nope": "x", "label": "b" },
              { "number": "3", "label": "c" }
            ]
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

            contactInDatabase.Content.PhoneNumbers.Select(phone => (phone.Number, phone.Label)).Should().Equal(("1", "a"), ("2", "b"), ("3", "c"));
        });
    }
}
