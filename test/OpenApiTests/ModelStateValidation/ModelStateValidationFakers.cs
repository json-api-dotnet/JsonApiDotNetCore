using Bogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace OpenApiTests.ModelStateValidation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ModelStateValidationFakers
{
    private readonly Lazy<Faker<SocialMediaAccount>> _lazySocialMediaAccountFaker = new(() => new Faker<SocialMediaAccount>()
        .MakeDeterministic()
        .RuleFor(socialMediaAccount => socialMediaAccount.AlternativeId, faker => faker.Random.Guid())
        .RuleFor(socialMediaAccount => socialMediaAccount.FirstName, faker => faker.Person.FirstName)
        .RuleFor(socialMediaAccount => socialMediaAccount.GivenName, (_, socialMediaAccount) => socialMediaAccount.FirstName)
        .RuleFor(socialMediaAccount => socialMediaAccount.LastName, faker => faker.Person.LastName)
        .RuleFor(socialMediaAccount => socialMediaAccount.UserName, faker => faker.Person.UserName)
        .RuleFor(socialMediaAccount => socialMediaAccount.CreditCard, faker => faker.Finance.CreditCardNumber())
        .RuleFor(socialMediaAccount => socialMediaAccount.Email, faker => faker.Person.Email)
        .RuleFor(socialMediaAccount => socialMediaAccount.Password, faker => faker.Random.String2(8, 32))
        .RuleFor(socialMediaAccount => socialMediaAccount.Password, faker => Convert.ToBase64String(faker.Random.Bytes(20)))
        .RuleFor(socialMediaAccount => socialMediaAccount.Phone, faker => faker.Person.Phone)
        .RuleFor(socialMediaAccount => socialMediaAccount.Age, faker => faker.Random.Double(0.1, 122.9))
        .RuleFor(socialMediaAccount => socialMediaAccount.ProfilePicture, faker => new Uri(faker.Image.LoremFlickrUrl()))
        .RuleFor(socialMediaAccount => socialMediaAccount.BackgroundPicture, faker => faker.Image.LoremFlickrUrl())
        .RuleFor(socialMediaAccount => socialMediaAccount.Tags, faker => faker.Make(faker.Random.Number(1, 10), () => faker.Random.String2(2, 10)))
        .RuleFor(socialMediaAccount => socialMediaAccount.CountryCode, faker => faker.Random.ListItem(["NL", "FR"]))
        .RuleFor(socialMediaAccount => socialMediaAccount.Planet, faker => faker.Random.String2(2, 8))
        .RuleFor(socialMediaAccount => socialMediaAccount.NextRevalidation, faker => TimeSpan.FromMinutes(faker.Random.Number(1, 5)))
        .RuleFor(socialMediaAccount => socialMediaAccount.ValidatedAt, faker => faker.Date.Recent().ToUniversalTime())
        .RuleFor(socialMediaAccount => socialMediaAccount.ValidatedAtDate, faker => DateOnly.FromDateTime(faker.Date.Recent()))
        .RuleFor(socialMediaAccount => socialMediaAccount.ValidatedAtTime, faker => TimeOnly.FromDateTime(faker.Date.Recent())));

    public Faker<SocialMediaAccount> SocialMediaAccount => _lazySocialMediaAccountFaker.Value;
}
