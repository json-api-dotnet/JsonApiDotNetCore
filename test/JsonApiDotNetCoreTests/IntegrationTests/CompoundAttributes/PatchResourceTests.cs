using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

public sealed class PatchResourceTests : IClassFixture<IntegrationTestContext<TestableStartup<CompoundAttributeDbContext>, CompoundAttributeDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundAttributeDbContext>, CompoundAttributeDbContext> _testContext;
    private readonly CompoundAttributeFakers _fakers = new();

    public PatchResourceTests(IntegrationTestContext<TestableStartup<CompoundAttributeDbContext>, CompoundAttributeDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CloudAccountsController>();
    }

    // TODO: Split up into individual tests. Make collection elements nullable. Add a non-exposed property, ensuring it is preserved.
    [Fact]
    public async Task Can_update_compound_attribute()
    {
        // Arrange
        CloudAccount existingAccount = _fakers.CloudAccount.GenerateOne();
        existingAccount.EmergencyContact.PrimaryPhoneNumber = null;
        existingAccount.EmergencyContact.PreviousLivingAddresses = null;
        existingAccount.EmergencyContact.Websites = null;

        Contact newContact1 = _fakers.Contact.GenerateOne();
        Contact newContact2 = _fakers.Contact.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Accounts.Add(existingAccount);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "cloudAccounts",
                id = existingAccount.StringId,
                attributes = new
                {
                    emergencyContact = new
                    {
                        displayName = newContact1.DisplayName,
                        livingAddress = new
                        {
                            line1 = newContact1.LivingAddress.Line1,
                            line2 = (object?)null
                        },
                        primaryPhoneNumber = new
                        {
                            type = newContact1.PrimaryPhoneNumber!.Type,
                            number = newContact1.PrimaryPhoneNumber!.Number
                        },
                        secondaryPhoneNumbers = Array.Empty<object>(),
                        emailAddresses = Array.Empty<string>()
                    },
                    backupEmergencyContact = (object?)null,
                    contacts = new[]
                    {
                        new
                        {
                            displayName = newContact2.DisplayName,
                            livingAddress = new
                            {
                                line1 = newContact2.LivingAddress.Line1,
                                city = newContact2.LivingAddress.City,
                                country = newContact2.LivingAddress.Country,
                                postalCode = newContact2.LivingAddress.PostalCode
                            },
                            previousLivingAddresses = Array.Empty<object>(),
                            emailAddresses = new[]
                            {
                                newContact2.EmailAddresses.ElementAt(0)
                            },
                            websites = new[]
                            {
                                newContact2.Websites!.ElementAt(0)
                            }
                        }
                    }
                }
            }
        };

        string route = $"/cloudAccounts/{existingAccount.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Id.Should().Be(existingAccount.StringId);
            resource.Type.Should().Be("cloudAccounts");
            resource.Attributes.Should().HaveCount(3);

            resource.Attributes.Should().ContainKey("emergencyContact").WhoseValue.Should().BeOfType<Dictionary<string, object?>>().Subject.With(contact =>
            {
                contact.Should().HaveCount(7);
                contact.Should().ContainKey("displayName").WhoseValue.Should().Be(newContact1.DisplayName);

                contact.Should().ContainKey("livingAddress").WhoseValue.Should().BeOfType<Dictionary<string, object?>>().Subject.With(address =>
                {
                    address.Should().HaveCount(5);
                    address.Should().ContainKey("line1").WhoseValue.Should().Be(newContact1.LivingAddress.Line1);
                    address.Should().ContainKey("line2").WhoseValue.Should().BeNull();
                    address.Should().ContainKey("city").WhoseValue.Should().Be(existingAccount.EmergencyContact.LivingAddress.City);
                    address.Should().ContainKey("country").WhoseValue.Should().Be(existingAccount.EmergencyContact.LivingAddress.Country);
                    address.Should().ContainKey("postalCode").WhoseValue.Should().Be(existingAccount.EmergencyContact.LivingAddress.PostalCode);
                });

                contact.Should().ContainKey("previousLivingAddresses").WhoseValue.Should().BeNull();

                contact.Should().ContainKey("primaryPhoneNumber").WhoseValue.Should().BeOfType<Dictionary<string, object?>>().Subject.With(phoneNumber =>
                {
                    phoneNumber.Should().HaveCount(3);
                    phoneNumber.Should().ContainKey("type").WhoseValue.Should().Be(newContact1.PrimaryPhoneNumber!.Type);
                    phoneNumber.Should().ContainKey("countryCode").WhoseValue.Should().BeNull();
                    phoneNumber.Should().ContainKey("number").WhoseValue.Should().Be(newContact1.PrimaryPhoneNumber!.Number);
                });

                contact.Should().ContainKey("secondaryPhoneNumbers").WhoseValue.Should().BeOfType<List<object?>>().Subject.Should().BeEmpty();
                contact.Should().ContainKey("emailAddresses").WhoseValue.Should().BeOfType<List<object?>>().Subject.Should().BeEmpty();
                contact.Should().ContainKey("websites").WhoseValue.Should().BeNull();
            });

            resource.Attributes.Should().ContainKey("backupEmergencyContact").WhoseValue.Should().BeNull();

            resource.Attributes.Should().ContainKey("contacts").WhoseValue.Should().BeOfType<List<object?>>().Subject.With(contacts =>
            {
                contacts.Should().HaveCount(1);

                contacts[0].Should().BeOfType<Dictionary<string, object?>>().Subject.With(contact =>
                {
                    contact.Should().HaveCount(7);
                    contact.Should().ContainKey("displayName").WhoseValue.Should().Be(newContact2.DisplayName);

                    contact.Should().ContainKey("livingAddress").WhoseValue.Should().BeOfType<Dictionary<string, object?>>().Subject.With(address =>
                    {
                        address.Should().HaveCount(5);
                        address.Should().ContainKey("line1").WhoseValue.Should().Be(newContact2.LivingAddress.Line1);
                        address.Should().ContainKey("line2").WhoseValue.Should().BeNull();
                        address.Should().ContainKey("city").WhoseValue.Should().Be(newContact2.LivingAddress.City);
                        address.Should().ContainKey("country").WhoseValue.Should().Be(newContact2.LivingAddress.Country);
                        address.Should().ContainKey("postalCode").WhoseValue.Should().Be(newContact2.LivingAddress.PostalCode);
                    });

                    contact.Should().ContainKey("previousLivingAddresses").WhoseValue.Should().BeOfType<List<object?>>().Subject.Should().BeEmpty();
                    contact.Should().ContainKey("primaryPhoneNumber").WhoseValue.Should().BeNull();
                    contact.Should().ContainKey("secondaryPhoneNumbers").WhoseValue.Should().BeOfType<List<object?>>().Subject.Should().BeEmpty();

                    contact.Should().ContainKey("emailAddresses").WhoseValue.Should().BeOfType<List<object?>>().Subject.With(emails =>
                    {
                        emails.Should().HaveCount(1);
                        emails[0].Should().Be(newContact2.EmailAddresses.ElementAt(0));
                    });

                    contact.Should().ContainKey("websites").WhoseValue.Should().BeOfType<List<object?>>().Subject.With(websites =>
                    {
                        websites.Should().HaveCount(1);
                        websites[0].Should().Be(newContact2.Websites!.ElementAt(0));
                    });
                });
            });
        });

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            CloudAccount accountInDatabase = await dbContext.Accounts.FirstWithIdAsync(existingAccount.Id);

            accountInDatabase.EmergencyContact.DisplayName.Should().Be(newContact1.DisplayName);
            accountInDatabase.EmergencyContact.LivingAddress.Line1.Should().Be(newContact1.LivingAddress.Line1);
            accountInDatabase.EmergencyContact.LivingAddress.Line2.Should().BeNull();
            accountInDatabase.EmergencyContact.LivingAddress.City.Should().Be(existingAccount.EmergencyContact.LivingAddress.City);
            accountInDatabase.EmergencyContact.LivingAddress.Country.Should().Be(existingAccount.EmergencyContact.LivingAddress.Country);
            accountInDatabase.EmergencyContact.LivingAddress.PostalCode.Should().Be(existingAccount.EmergencyContact.LivingAddress.PostalCode);
            accountInDatabase.EmergencyContact.PrimaryPhoneNumber.Should().NotBeNull();
            accountInDatabase.EmergencyContact.PrimaryPhoneNumber.Type.Should().Be(newContact1.PrimaryPhoneNumber!.Type);
            accountInDatabase.EmergencyContact.PrimaryPhoneNumber.CountryCode.Should().BeNull();
            accountInDatabase.EmergencyContact.PrimaryPhoneNumber.Number.Should().Be(newContact1.PrimaryPhoneNumber!.Number);
            accountInDatabase.EmergencyContact.SecondaryPhoneNumbers.Should().BeEmpty();

            accountInDatabase.BackupEmergencyContact.Should().BeNull();

            accountInDatabase.Contacts.Should().HaveCount(1);
            accountInDatabase.Contacts.ElementAt(0).DisplayName.Should().Be(newContact2.DisplayName);
            accountInDatabase.Contacts.ElementAt(0).LivingAddress.Line1.Should().Be(newContact2.LivingAddress.Line1);
            accountInDatabase.Contacts.ElementAt(0).LivingAddress.Line2.Should().BeNull();
            accountInDatabase.Contacts.ElementAt(0).LivingAddress.City.Should().Be(newContact2.LivingAddress.City);
            accountInDatabase.Contacts.ElementAt(0).LivingAddress.Country.Should().Be(newContact2.LivingAddress.Country);
            accountInDatabase.Contacts.ElementAt(0).LivingAddress.PostalCode.Should().Be(newContact2.LivingAddress.PostalCode);
            accountInDatabase.Contacts.ElementAt(0).PreviousLivingAddresses.Should().BeEmpty();
            accountInDatabase.Contacts.ElementAt(0).PrimaryPhoneNumber.Should().BeNull();
            accountInDatabase.Contacts.ElementAt(0).SecondaryPhoneNumbers.Should().BeEmpty();
            accountInDatabase.Contacts.ElementAt(0).EmailAddresses.Should().HaveCount(1);
            accountInDatabase.Contacts.ElementAt(0).EmailAddresses[0].Should().Be(newContact2.EmailAddresses.ElementAt(0));
            accountInDatabase.Contacts.ElementAt(0).Websites.Should().HaveCount(1);
            accountInDatabase.Contacts.ElementAt(0).Websites![0].Should().Be(newContact2.Websites!.ElementAt(0));
        });
    }
}
