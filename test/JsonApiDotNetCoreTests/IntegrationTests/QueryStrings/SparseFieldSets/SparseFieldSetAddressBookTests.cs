using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.SparseFieldSets;

/// <summary>
/// Verifies sparse fieldsets on the address book domain, which has a read-only nested member, a view-blocked nested member and a nullable struct
/// compound.
/// </summary>
public sealed class SparseFieldSetAddressBookTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;
    private readonly CompoundAttributesFakers _fakers = new();

    public SparseFieldSetAddressBookTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
        testContext.UseController<ContactNotesController>();
    }

    [Fact]
    public async Task Cannot_select_view_blocked_nested_member()
    {
        // Arrange
        var parameterValue = new MarkedText("content.name.^secretAlias", '^');
        string route = $"/contacts?fields[contacts]={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified fieldset is invalid.");
        error.Detail.Should().Be($"Retrieving attribute 'secretAlias' is not allowed. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("fields[contacts]");
    }

    [Fact]
    public async Task Can_fetch_all_scalar_properties_when_fieldset_selects_read_only_nested_member()
    {
        // Arrange
        Contact contact = CompoundAttributesSamples.CreateContact();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts/{contact.StringId}?fields[contacts]=content.name.displayName";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);

        JsonElement content = responseDocument.Data.SingleValue.GetAttribute("content");
        content.GetMemberNames().Should().Equal("name");
        content.GetProperty("name").GetMemberNames().Should().Equal("displayName");
        content.GetProperty("name").GetProperty("displayName").GetString().Should().Be("Ada M Lovelace");
    }

    [Theory]
    [InlineData("lastKnownLocation")]
    [InlineData("lastKnownLocation.latitude")]
    public async Task Can_select_nullable_struct_compound_in_collection(string fieldChain)
    {
        // Arrange
        List<Contact> contacts = _fakers.Contact.GenerateList(2);
        contacts[0].LastKnownLocation = null;

        contacts[1].LastKnownLocation = new GeoPoint
        {
            Latitude = 52.09,
            Longitude = 5.12
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.AddRange(contacts);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/contacts?fields[contacts]={fieldChain}&sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].GetAttribute("lastKnownLocation").ValueKind.Should().Be(JsonValueKind.Null);

        JsonElement location = responseDocument.Data.ManyValue[1].GetAttribute("lastKnownLocation");
        location.GetProperty("latitude").GetDouble().Should().Be(52.09);

        if (fieldChain == "lastKnownLocation")
        {
            location.GetProperty("longitude").GetDouble().Should().Be(5.12);
        }
        else
        {
            location.GetMemberNames().Should().Equal("latitude");
        }
    }

    [Fact]
    public async Task Can_include_through_resource_with_nullable_struct_compound()
    {
        // Arrange
        AddressBook addressBook = _fakers.AddressBook.GenerateOne();
        Contact contact = _fakers.Contact.GenerateOne();

        contact.LastKnownLocation = new GeoPoint
        {
            Latitude = 1,
            Longitude = 2
        };

        contact.Notes = new HashSet<ContactNote>
        {
            new()
            {
                Text = "note",
                Priority = 1
            }
        };

        addressBook.Contacts = new[]
        {
            contact
        }.ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<AddressBook>();
            dbContext.AddressBooks.Add(addressBook);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/addressBooks/{addressBook.StringId}?include=contacts.notes";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(2);

        ResourceObject includedContact = responseDocument.Included.Single(resource => resource.Type == "contacts");
        includedContact.GetAttribute("lastKnownLocation").GetProperty("latitude").GetDouble().Should().Be(1);
    }
}
