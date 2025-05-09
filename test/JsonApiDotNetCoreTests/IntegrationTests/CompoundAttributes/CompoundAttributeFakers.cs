using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

internal sealed class CompoundAttributeFakers
{
    private readonly Lazy<Faker<CloudAccount>> _lazyCloudAccountFaker;
    private readonly Lazy<Faker<Contact>> _lazyContactFaker;

    private readonly Lazy<Faker<Address>> _lazyAddressFaker = new(() => new Faker<Address>()
        .MakeDeterministic()
        .RuleFor(address => address.Line1, faker => faker.Address.StreetAddress())
        .RuleFor(address => address.Line2, faker => faker.Address.Direction())
        .RuleFor(address => address.City, faker => faker.Address.City())
        .RuleFor(address => address.Country, faker => faker.Address.Country())
        .RuleFor(address => address.PostalCode, faker => faker.Address.ZipCode()));

    private readonly Lazy<Faker<PhoneNumber>> _lazyPhoneNumberFaker = new(() => new Faker<PhoneNumber>()
        .MakeDeterministic()
        .RuleFor(phoneNumber => phoneNumber.Type, faker => faker.PickRandom<PhoneNumberType>())
        .RuleFor(phoneNumber => phoneNumber.CountryCode, faker => faker.Random.Int(1, 999))
        .RuleFor(phoneNumber => phoneNumber.Number, faker => faker.Phone.PhoneNumber()));

    public Faker<CloudAccount> CloudAccount => _lazyCloudAccountFaker.Value;
    public Faker<Contact> Contact => _lazyContactFaker.Value;
    public Faker<Address> Address => _lazyAddressFaker.Value;
    public Faker<PhoneNumber> PhoneNumber => _lazyPhoneNumberFaker.Value;

    public CompoundAttributeFakers()
    {
        _lazyCloudAccountFaker = new Lazy<Faker<CloudAccount>>(() => new Faker<CloudAccount>()
            .MakeDeterministic()
            .RuleFor(account => account.EmergencyContact, _ => Contact.GenerateOne())
            .RuleFor(account => account.BackupEmergencyContact, _ => Contact.GenerateOne())
            .RuleFor(account => account.Contacts, _ => Contact.GenerateSet(2)));

        _lazyContactFaker = new Lazy<Faker<Contact>>(() => new Faker<Contact>()
            .MakeDeterministic()
            .RuleFor(contact => contact.DisplayName, faker => faker.Person.FullName)
            .RuleFor(contact => contact.LivingAddress, _ => Address.GenerateOne())
            .RuleFor(contact => contact.PreviousLivingAddresses, _ => Address.GenerateList(2))
            .RuleFor(contact => contact.PrimaryPhoneNumber, _ => PhoneNumber.GenerateOne())
            .RuleFor(contact => contact.SecondaryPhoneNumbers, _ => PhoneNumber.GenerateList(2))
            .RuleFor(contact => contact.EmailAddresses, faker => faker.Make(2, () => faker.Person.Email).ToArray())
            .RuleFor(contact => contact.Websites, faker => faker.Make(2, () => faker.Person.Website).ToArray()));
    }
}
