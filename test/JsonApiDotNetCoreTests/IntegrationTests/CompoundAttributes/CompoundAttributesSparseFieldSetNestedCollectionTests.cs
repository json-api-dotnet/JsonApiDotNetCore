using System.Net;
using System.Text.Json.Nodes;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

public sealed class CompoundAttributesSparseFieldSetNestedCollectionTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;
    private readonly CompoundAttributesFakers _fakers = new();

    public CompoundAttributesSparseFieldSetNestedCollectionTests(
        IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ContactsController>();
    }

    [Fact]
    public async Task Can_select_leaf_inside_collection_of_compound_attributes_nested_in_compound_attribute()
    {
        // Arrange
        Contact contact = _fakers.Contact.GenerateOne();
        contact.Content = _fakers.ContactRoot.GenerateOne();
        contact.Content.PhoneNumbers = _fakers.ContactPhoneNumber.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}?fields[contacts]=content.phoneNumbers.label";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);

        var content = (JsonObject)responseDocument.Data.SingleValue.GetAttribute("content")!;
        content.Select(property => property.Key).Should().Equal("phoneNumbers");

        var phoneNumbers = (JsonArray)content["phoneNumbers"]!;
        phoneNumbers.Should().HaveCount(2);

        for (int index = 0; index < 2; index++)
        {
            var phoneNumber = (JsonObject)phoneNumbers[index]!;
            phoneNumber["label"]!.GetValue<string>().Should().Be(contact.Content.PhoneNumbers[index].Label);
        }
    }
}
