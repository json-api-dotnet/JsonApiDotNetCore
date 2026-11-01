using System.Net;
using System.Text.Json.Nodes;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

/// <summary>
/// Public members of a compound type without [Attr] are set by server-side code, are stored, fetched before an update and saved, but are never read from
/// or written to request/response bodies and have no meaning in query strings.
/// </summary>
public sealed class CompoundAttributesNonExposedMemberTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext>>
{
    private static readonly DateTime StampTime = new(2030, 5, 6, 7, 8, 9, DateTimeKind.Utc);

    private readonly IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> _testContext;
    // The same instance is used for all tests of this class, because services can only be configured once per fixture.
    private static readonly StampSettings Settings = new();

    public CompoundAttributesNonExposedMemberTests(IntegrationTestContext<TestableStartup<CompoundAttributesDbContext>, CompoundAttributesDbContext> testContext)
    {
        _testContext = testContext;
        Settings.Reset();

        testContext.UseController<AddressBooksController>();
        testContext.UseController<ContactsController>();
        testContext.UseController<ContactNotesController>();

        testContext.ConfigureServices(RegisterServices);
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(Settings);
        services.AddResourceDefinition<ContactStampDefinition>();
    }

    [Fact]
    public async Task Can_set_non_exposed_member_on_create_without_exposing_it()
    {
        // Arrange
        Settings.StampOnCreate = true;

        object requestBody = CompoundAttributesSamples.CreateContactPostBody(CompoundAttributesSamples.CreatePostContent());

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>("/contacts", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        JsonNode? company = responseDocument.Data.SingleValue.GetAttribute("content", "company");
        AssertNoNonExposedMembers(company);

        long contactId = long.Parse(responseDocument.Data.SingleValue.Id!);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(contactId);

            contactInDatabase.Content.Company.Should().NotBeNull();
            contactInDatabase.Content.Company.InternalVersion.Should().Be(1);
            contactInDatabase.Content.Company.LastUpdatedAt.Should().Be(StampTime);
        });

        (HttpResponseMessage getResponse, Document getDocument) = await _testContext.ExecuteGetAsync<Document>($"/contacts/{contactId}");

        getResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        AssertNoNonExposedMembers(getDocument.Data.SingleValue!.GetAttribute("content", "company"));
    }

    [Theory]
    [InlineData("filter=equals(content.company.internalVersion,'1')", "filter")]
    [InlineData("filter=equals(content.company.lastUpdatedAt,null)", "filter")]
    [InlineData("sort=content.company.internalVersion", "sort")]
    [InlineData("fields[contacts]=content.company.internalVersion", "fields[contacts]")]
    [InlineData("fields[contacts]=content.company.lastUpdatedAt", "fields[contacts]")]
    public async Task Cannot_use_non_exposed_member_in_query_string(string queryString, string parameterName)
    {
        // Arrange
        Contact contact = CompoundAttributesSamples.CreateContact();
        contact.Content.Company!.InternalVersion = 3;
        contact.Content.Company.LastUpdatedAt = StampTime;
        await SeedAsync(contact);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>($"/contacts?{queryString}");

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);
        responseDocument.Errors[0].Source.Should().NotBeNull();
        responseDocument.Errors[0].Source!.Parameter.Should().Be(parameterName);
        responseDocument.Errors[0].Detail.Should().Contain("does not exist");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cannot_send_non_exposed_member_in_request_body(bool isPatch)
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        JsonObject content = isPatch ? CompoundAttributesSamples.CreateIdenticalPatchContent() : CompoundAttributesSamples.CreatePostContent();
        ((JsonObject)content["company"]!)["internalVersion"] = 99;

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = isPatch
            ? await _testContext.ExecutePatchAsync<Document>($"/contacts/{existingContact.StringId}",
                CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, content))
            : await _testContext.ExecutePostAsync<Document>("/contacts", CompoundAttributesSamples.CreateContactPostBody(content));

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);
        responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Unknown attribute found.");
        responseDocument.Errors[0].Detail.Should().Be("Attribute 'internalVersion' does not exist on compound type 'ContactCompany'.");
        responseDocument.Errors[0].Source.Should().NotBeNull();
        responseDocument.Errors[0].Source!.Pointer.Should().Be("/data/attributes/content/company/internalVersion");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Never_stores_non_exposed_member_from_request_body_when_unknown_members_are_allowed(bool isPatch)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.App.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownFieldsInRequestBody = true;

        try
        {
            Contact existingContact = CompoundAttributesSamples.CreateContact();
            await SeedAsync(existingContact);

            JsonObject content = isPatch ? CompoundAttributesSamples.CreateIdenticalPatchContent() : CompoundAttributesSamples.CreatePostContent();
            var company = (JsonObject)content["company"]!;
            company["internalVersion"] = 99;
            company["lastUpdatedAt"] = "2001-01-01T00:00:00Z";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = isPatch
                ? await _testContext.ExecutePatchAsync<Document>($"/contacts/{existingContact.StringId}",
                    CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, content))
                : await _testContext.ExecutePostAsync<Document>("/contacts", CompoundAttributesSamples.CreateContactPostBody(content));

            // Assert
            httpResponse.ShouldHaveStatusCode(isPatch ? HttpStatusCode.NoContent : HttpStatusCode.Created);

            long contactId = isPatch ? existingContact.Id : long.Parse(responseDocument.Data.SingleValue!.Id!);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(contactId);

                contactInDatabase.Content.Company.Should().NotBeNull();
                contactInDatabase.Content.Company.InternalVersion.Should().Be(0);
                contactInDatabase.Content.Company.LastUpdatedAt.Should().BeNull();
            });
        }
        finally
        {
            options.AllowUnknownFieldsInRequestBody = false;
        }
    }

    [Fact]
    public async Task Fetches_non_exposed_member_before_update_and_saves_it()
    {
        // Arrange
        Settings.CarryOverOnUpdate = true;

        Contact existingContact = CompoundAttributesSamples.CreateContact();
        existingContact.Content.Company!.InternalVersion = 7;
        existingContact.Content.Company.LastUpdatedAt = StampTime;
        await SeedAsync(existingContact);

        // The request replaces the whole compound, with a different company name, and cannot contain the non-exposed members.
        JsonObject content = CompoundAttributesSamples.CreateIdenticalPatchContent();
        content["company"]!["name"] = "Fabrikam";

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, content);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePatchAsync<Document>($"/contacts/{existingContact.StringId}", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        // The stored non-exposed values differ from what the request carried, which is a side effect.
        AssertNoNonExposedMembers(responseDocument.Data.SingleValue!.GetAttribute("content", "company"));
        responseDocument.Data.SingleValue!.GetAttribute("content", "company.name").ShouldBeJson("\"Fabrikam\"");

        Settings.SeenStoredVersion.Should().Be(7);
        Settings.SeenStoredUpdatedAt.Should().Be(StampTime);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.Content.Company.Should().NotBeNull();
            contactInDatabase.Content.Company.Name.Should().Be("Fabrikam");
            contactInDatabase.Content.Company.InternalVersion.Should().Be(7);
            contactInDatabase.Content.Company.LastUpdatedAt.Should().Be(StampTime);
        });
    }

    [Fact]
    public async Task Loses_non_exposed_member_on_update_when_definition_does_not_carry_it_over()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        existingContact.Content.Company!.InternalVersion = 7;
        await SeedAsync(existingContact);

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, CompoundAttributesSamples.CreateIdenticalPatchContent());

        // Act
        (HttpResponseMessage httpResponse, string _) = await _testContext.ExecutePatchAsync<string>($"/contacts/{existingContact.StringId}", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.Content.Company.Should().NotBeNull();
            contactInDatabase.Content.Company.InternalVersion.Should().Be(0);
        });
    }

    [Fact]
    public async Task Returns_no_content_when_non_exposed_member_is_unchanged()
    {
        // Arrange
        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, CompoundAttributesSamples.CreateIdenticalPatchContent());

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>($"/contacts/{existingContact.StringId}", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Returns_body_when_only_non_exposed_member_is_changed_by_definition()
    {
        // Arrange
        Settings.StampOnUpdate = true;

        Contact existingContact = CompoundAttributesSamples.CreateContact();
        await SeedAsync(existingContact);

        object requestBody = CompoundAttributesSamples.CreateContactPatchBody(existingContact.StringId!, CompoundAttributesSamples.CreateIdenticalPatchContent());

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePatchAsync<Document>($"/contacts/{existingContact.StringId}", requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.GetAttribute("content").ShouldBeJson(CompoundAttributesSamples.ExpectedContentJson);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Contact contactInDatabase = await dbContext.Contacts.FirstWithIdAsync(existingContact.Id);

            contactInDatabase.Content.Company.Should().NotBeNull();
            contactInDatabase.Content.Company.LastUpdatedAt.Should().Be(StampTime);
        });
    }

    [Theory]
    [InlineData("content")]
    [InlineData("content.company")]
    [InlineData("content.company.name")]
    public async Task Never_renders_non_exposed_member_with_sparse_fieldset(string fields)
    {
        // Arrange
        Contact contact = CompoundAttributesSamples.CreateContact();
        contact.Content.Company!.InternalVersion = 3;
        contact.Content.Company.LastUpdatedAt = StampTime;
        await SeedAsync(contact);

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecuteGetAsync<Document>($"/contacts/{contact.StringId}?fields[contacts]={fields}");

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        JsonNode? company = responseDocument.Data.SingleValue.GetAttribute("content", "company");
        company.Should().NotBeNull();
        AssertNoNonExposedMembers(company);
    }

    private static void AssertNoNonExposedMembers(JsonNode? company)
    {
        company.Should().NotBeNull();

        string json = company.ToJsonString();
        json.Should().NotContainEquivalentOf("internalVersion");
        json.Should().NotContainEquivalentOf("lastUpdatedAt");
    }

    private async Task SeedAsync(Contact contact)
    {
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Contact>();
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();
        });
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class StampSettings
    {
        public bool StampOnCreate { get; set; }
        public bool StampOnUpdate { get; set; }
        public bool CarryOverOnUpdate { get; set; }
        public int? SeenStoredVersion { get; set; }
        public DateTime? SeenStoredUpdatedAt { get; set; }

        public void Reset()
        {
            StampOnCreate = false;
            StampOnUpdate = false;
            CarryOverOnUpdate = false;
            SeenStoredVersion = null;
            SeenStoredUpdatedAt = null;
        }
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private sealed class ContactStampDefinition(IResourceGraph resourceGraph, StampSettings settings) : JsonApiResourceDefinition<Contact, long>(resourceGraph)
    {
        private readonly StampSettings _settings = settings;

        private int _storedVersion;
        private DateTime? _storedUpdatedAt;

        public override Task OnPrepareWriteAsync(Contact resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (writeOperation == WriteOperationKind.UpdateResource)
            {
                // At this point the resource is what was fetched from the database, so this proves the non-exposed members were fetched.
                _storedVersion = resource.Content.Company?.InternalVersion ?? 0;
                _storedUpdatedAt = resource.Content.Company?.LastUpdatedAt;

                _settings.SeenStoredVersion = _storedVersion;
                _settings.SeenStoredUpdatedAt = _storedUpdatedAt;
            }

            return Task.CompletedTask;
        }

        public override Task OnWritingAsync(Contact resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            ContactCompany? company = resource.Content.Company;

            if (company != null)
            {
                if (writeOperation == WriteOperationKind.CreateResource && _settings.StampOnCreate)
                {
                    company.InternalVersion = 1;
                    company.LastUpdatedAt = StampTime;
                }
                else if (writeOperation == WriteOperationKind.UpdateResource)
                {
                    // The request replaced the compound, so the stored values must be copied over to survive.
                    if (_settings.CarryOverOnUpdate)
                    {
                        company.InternalVersion = _storedVersion;
                        company.LastUpdatedAt = _storedUpdatedAt;
                    }

                    if (_settings.StampOnUpdate)
                    {
                        company.LastUpdatedAt = StampTime;
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
