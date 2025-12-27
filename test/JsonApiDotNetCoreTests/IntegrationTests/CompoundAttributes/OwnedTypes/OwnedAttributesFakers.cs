using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.OwnedTypes;

internal sealed class OwnedAttributesFakers
{
    private static readonly List<string> GenericLabels =
    [
        "home",
        "work",
        "other"
    ];

    private static readonly List<string> PhoneLabels =
    [
        "home",
        "work",
        "mobile",
        "other"
    ];

    private static readonly List<string> WebsiteLabels =
    [
        "home",
        "work",
        "blog",
        "profile",
        "other"
    ];

    private static readonly List<string> RelationLabels =
    [
        "spouse",
        "child",
        "mother",
        "father",
        "parent",
        "brother",
        "sister",
        "friend",
        "relative",
        "domestic partner",
        "manager",
        "assistant",
        "referred by",
        "partner"
    ];

    private readonly Lazy<Faker<AddressBook>> _lazyAddressBookFaker;
    private readonly Lazy<Faker<Contact>> _lazyContactFaker;
    private readonly Lazy<Faker<ContactRoot>> _lazyContactRootFaker;

    private readonly Lazy<Faker<ContactName>> _lazyContactNameFaker = new(() => new Faker<ContactName>()
        .MakeDeterministic()
        .RuleFor(name => name.FirstName, faker => faker.Person.FirstName)
        .RuleFor(name => name.LastName, faker => faker.Person.LastName)
        .RuleFor(name => name.DisplayName, faker => faker.Person.FullName));

    private readonly Lazy<Faker<ContactCompany>> _lazyContactCompanyFaker = new(() => new Faker<ContactCompany>()
        .MakeDeterministic()
        .RuleFor(company => company.Name, faker => faker.Company.CompanyName())
        .RuleFor(company => company.JobTitle, faker => faker.Name.JobTitle())
        .RuleFor(company => company.Department, faker => faker.Commerce.Department()));

    private readonly Lazy<Faker<LabeledValue<string>>> _lazyEmailAddressFaker = new(() => new Faker<LabeledValue<string>>()
        .MakeDeterministic()
        .RuleFor(labeledValue => labeledValue.Value, faker => faker.Person.Email)
        .RuleFor(labeledValue => labeledValue.Label, faker => faker.PickRandom(GenericLabels)));

    private readonly Lazy<Faker<ContactPhoneNumber>> _lazyContactPhoneNumberFaker = new(() => new Faker<ContactPhoneNumber>()
        .MakeDeterministic()
        .RuleFor(phoneNumber => phoneNumber.CountryPrefix, faker => $"+{faker.Random.Number(1, 999)}")
        .RuleFor(phoneNumber => phoneNumber.Digits, faker => faker.Random.Digits(10).Select(digit => (sbyte)digit).ToArray())
        .RuleFor(phoneNumber => phoneNumber.Label, faker => faker.PickRandom(PhoneLabels)));

    private readonly Lazy<Faker<ContactAddress>> _lazyContactAddressFaker = new(() => new Faker<ContactAddress>()
        .MakeDeterministic()
        .RuleFor(address => address.IsoCountryCode, faker => faker.Address.CountryCode())
        .RuleFor(address => address.Line1, faker => faker.Address.StreetAddress())
        .RuleFor(address => address.Line2, faker => faker.Address.SecondaryAddress())
        .RuleFor(address => address.PostalCode, faker => faker.Address.ZipCode())
        .RuleFor(address => address.City, faker => faker.Address.City())
        .RuleFor(address => address.Label, faker => faker.PickRandom(GenericLabels)));

    private readonly Lazy<Faker<ContactDate>> _lazyContactDateFaker = new(() => new Faker<ContactDate>()
        .MakeDeterministic()
        .RuleFor(date => date.Year, faker =>
        {
            DateTime recentTime = faker.Date.Recent();
            return (ushort)faker.Random.Int(recentTime.AddYears(-75).Year, recentTime.AddYears(-1).Year);
        })
        .RuleFor(date => date.Month, faker => faker.Random.Byte(1, 12))
        .RuleFor(date => date.Day, faker => faker.Random.Byte(1, 28)));

    private readonly Lazy<Faker<LabeledValue<Uri>>> _lazyWebsiteFaker = new(() => new Faker<LabeledValue<Uri>>()
        .MakeDeterministic()
        .RuleFor(labeledValue => labeledValue.Value, faker => new Uri(faker.Internet.Url()))
        .RuleFor(labeledValue => labeledValue.Label, faker => faker.PickRandom(WebsiteLabels)));

    private readonly Lazy<Faker<LabeledValue<string>>> _lazyRelationsFaker = new(() => new Faker<LabeledValue<string>>()
        .MakeDeterministic()
        .RuleFor(labeledValue => labeledValue.Value, faker => faker.Person.FullName)
        .RuleFor(labeledValue => labeledValue.Label, faker => faker.PickRandom(RelationLabels)));

    public Faker<AddressBook> AddressBook => _lazyAddressBookFaker.Value;
    public Faker<Contact> Contact => _lazyContactFaker.Value;
    public Faker<ContactRoot> ContactRoot => _lazyContactRootFaker.Value;
    public Faker<ContactName> Name => _lazyContactNameFaker.Value;
    public Faker<LabeledValue<string>> EmailAddress => _lazyEmailAddressFaker.Value;
    public Faker<ContactCompany> Company => _lazyContactCompanyFaker.Value;
    public Faker<ContactPhoneNumber> PhoneNumber => _lazyContactPhoneNumberFaker.Value;
    public Faker<ContactAddress> Address => _lazyContactAddressFaker.Value;
    public Faker<ContactDate> Date => _lazyContactDateFaker.Value;
    public Faker<LabeledValue<Uri>> Website => _lazyWebsiteFaker.Value;
    public Faker<LabeledValue<string>> Relation => _lazyRelationsFaker.Value;

    public OwnedAttributesFakers()
    {
        _lazyContactFaker = new Lazy<Faker<Contact>>(() => new Faker<Contact>()
            .MakeDeterministic()
            .RuleFor(contact => contact.Content, _ => ContactRoot.GenerateOne()));

        _lazyContactRootFaker = new Lazy<Faker<ContactRoot>>(() => new Faker<ContactRoot>()
            .MakeDeterministic()
            .RuleFor(contact => contact.Name, _ => Name.GenerateOne())
            .RuleFor(contact => contact.Company, _ => Company.GenerateOne())
            .RuleFor(contact => contact.EmailAddresses, faker => faker.Make(faker.Random.Number(2, 5), () => EmailAddress.GenerateOne()).ToList())
            .RuleFor(contact => contact.EmergencyPhoneNumber, _ => PhoneNumber.GenerateOne())
            .RuleFor(contact => contact.PhoneNumbers, faker => faker.Make(faker.Random.Number(2, 4), () => PhoneNumber.GenerateOne()).ToList())
            .RuleFor(contact => contact.Addresses, faker => faker.Make(faker.Random.Number(2, 3), () => Address.GenerateOne()))
            .RuleFor(contact => contact.BirthDate, _ => Date.GenerateOne())
            .RuleFor(contact => contact.Websites, faker => faker.Make(faker.Random.Number(1, 5), () => Website.GenerateOne()).ToList())
            .RuleFor(contact => contact.Relations, faker => faker.Make(faker.Random.Number(2, 5), () => Relation.GenerateOne()).ToList())
            .RuleFor(contact => contact.Remarks, faker => faker.Lorem.Paragraph()));

        _lazyAddressBookFaker = new Lazy<Faker<AddressBook>>(() => new Faker<AddressBook>()
            .MakeDeterministic()
            .RuleFor(addressBook => addressBook.EmergencyContact, _ => ContactRoot.GenerateOne())
            .RuleFor(addressBook => addressBook.Favorites, _ => ContactRoot.GenerateList(1))
            .RuleFor(addressBook => addressBook.SyncUrls, faker => faker.Make(2, () => faker.Internet.Url()).ToList<string?>()));
    }
}
