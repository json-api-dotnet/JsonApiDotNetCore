using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

internal sealed class CompoundAttributesFakers
{
    private readonly Lazy<Faker<AddressBook>> _lazyAddressBookFaker;
    private readonly Lazy<Faker<Contact>> _lazyContactFaker;
    private readonly Lazy<Faker<ContactRoot>> _lazyContactRootFaker;
    private readonly Lazy<Faker<ContactName>> _lazyContactNameFaker;
    private readonly Lazy<Faker<ContactCompany>> _lazyContactCompanyFaker;
    private readonly Lazy<Faker<ContactAddress>> _lazyContactAddressFaker;
    private readonly Lazy<Faker<ContactPhoneNumber>> _lazyContactPhoneNumberFaker;
    private readonly Lazy<Faker<ContactDate>> _lazyContactDateFaker;

    private Faker<ContactName> ContactName => _lazyContactNameFaker.Value;
    private Faker<ContactAddress> ContactAddress => _lazyContactAddressFaker.Value;
    private Faker<ContactDate> ContactDate => _lazyContactDateFaker.Value;

    public Faker<ContactPhoneNumber> ContactPhoneNumber => _lazyContactPhoneNumberFaker.Value;
    public Faker<AddressBook> AddressBook => _lazyAddressBookFaker.Value;
    public Faker<Contact> Contact => _lazyContactFaker.Value;
    public Faker<ContactRoot> ContactRoot => _lazyContactRootFaker.Value;
    public Faker<ContactCompany> ContactCompany => _lazyContactCompanyFaker.Value;

    public CompoundAttributesFakers()
    {
        _lazyContactNameFaker = new Lazy<Faker<ContactName>>(() => new Faker<ContactName>()
            .MakeDeterministic()
            .RuleFor(name => name.FirstName, faker => faker.Name.FirstName())
            .RuleFor(name => name.MiddleName, faker => faker.Name.FirstName())
            .RuleFor(name => name.LastName, faker => faker.Name.LastName())
            .RuleFor(name => name.NickName, faker => faker.Internet.UserName())
            .RuleFor(name => name.MaidenName, faker => faker.Name.LastName()));

        _lazyContactCompanyFaker = new Lazy<Faker<ContactCompany>>(() => new Faker<ContactCompany>()
            .MakeDeterministic()
            .RuleFor(company => company.Name, faker => faker.Company.CompanyName())
            .RuleFor(company => company.Department, faker => faker.Commerce.Department())
            .RuleFor(company => company.BusinessHours, _ => [TimeSpan.FromHours(9), TimeSpan.FromHours(17)]));

        _lazyContactAddressFaker = new Lazy<Faker<ContactAddress>>(() => new Faker<ContactAddress>()
            .MakeDeterministic()
            .RuleFor(address => address.IsoCountryCode, faker => faker.Address.CountryCode())
            .RuleFor(address => address.City, faker => faker.Address.City())
            .RuleFor(address => address.Line1, faker => faker.Address.StreetAddress())
            .RuleFor(address => address.PostalCode, faker => faker.Address.ZipCode())
            .RuleFor(address => address.Label, faker => faker.PickRandom("home", "work", "other")));

        _lazyContactPhoneNumberFaker = new Lazy<Faker<ContactPhoneNumber>>(() => new Faker<ContactPhoneNumber>()
            .MakeDeterministic()
            .RuleFor(phone => phone.CountryPrefix, faker => $"+{faker.Random.Number(1, 99)}")
            .RuleFor(phone => phone.Number, faker => faker.Phone.PhoneNumber("###-###-####"))
            .RuleFor(phone => phone.Label, faker => faker.PickRandom("mobile", "home", "work")));

        _lazyContactDateFaker = new Lazy<Faker<ContactDate>>(() => new Faker<ContactDate>()
            .MakeDeterministic()
            .RuleFor(date => date.Year, faker => (ushort)faker.Random.Number(1950, 2020))
            .RuleFor(date => date.Month, faker => (byte)faker.Random.Number(1, 12))
            .RuleFor(date => date.Day, faker => (byte)faker.Random.Number(1, 28)));

        _lazyContactRootFaker = new Lazy<Faker<ContactRoot>>(() => new Faker<ContactRoot>()
            .MakeDeterministic()
            .RuleFor(root => root.Name, _ => ContactName.Generate())
            .RuleFor(root => root.Company, _ => ContactCompany.Generate())
            .RuleFor(root => root.BirthDate, _ => ContactDate.Generate())
            .RuleFor(root => root.SecurityPin, faker => faker.Random.Replace("####"))
            .RuleFor(root => root.Ratings, faker => [faker.Random.Number(1, 5), null, faker.Random.Number(1, 5)])
            .RuleFor(root => root.PhoneNumbers, _ => ContactPhoneNumber.GenerateList(2))
            .RuleFor(root => root.Addresses, _ => ContactAddress.GenerateList(1))
            .RuleFor(root => root.EmailAddresses, faker =>
            [
                new LabeledValue<string>
                {
                    Value = faker.Internet.Email(),
                    Label = "primary"
                }
            ])
            .RuleFor(root => root.Relations, faker =>
            [
                new LabeledValue<string>
                {
                    Value = faker.Name.FullName(),
                    Label = "friend"
                }
            ])
            .RuleFor(root => root.ImportantDates, faker =>
            [
                new LabeledValue<DateOnly>
                {
                    Value = DateOnly.FromDateTime(faker.Date.Past()),
                    Label = "anniversary"
                }
            ]));

        _lazyContactFaker = new Lazy<Faker<Contact>>(() => new Faker<Contact>()
            .MakeDeterministic()
            .RuleFor(contact => contact.Content, _ => ContactRoot.Generate())
            .RuleFor(contact => contact.Remarks, faker => faker.Lorem.Paragraph()));

        _lazyAddressBookFaker = new Lazy<Faker<AddressBook>>(() => new Faker<AddressBook>()
            .MakeDeterministic()
            .RuleFor(book => book.Name, faker => faker.Commerce.Department())
            .RuleFor(book => book.EmergencyContact, _ => ContactRoot.Generate())
            .RuleFor(book => book.Favorites, _ => ContactRoot.GenerateList(2))
            .RuleFor(book => book.Categories, faker =>
            [
                faker.Commerce.Categories(1)[0],
                faker.Commerce.Categories(1)[0]
            ])
            .RuleFor(book => book.SyncDeviceIds, faker =>
            [
                faker.Random.Guid(),
                faker.Random.Guid()
            ]));
    }
}
