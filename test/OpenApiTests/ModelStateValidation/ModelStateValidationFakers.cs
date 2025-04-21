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
        .RuleFor(socialMediaAccount => socialMediaAccount.LastName, faker => faker.Person.LastName)
        .RuleFor(socialMediaAccount => socialMediaAccount.UserName, faker => faker.Random.String2(3, 18))
        .RuleFor(socialMediaAccount => socialMediaAccount.CreditCard, faker => faker.Finance.CreditCardNumber())
        .RuleFor(socialMediaAccount => socialMediaAccount.Email, faker => faker.Person.Email)
        .RuleFor(socialMediaAccount => socialMediaAccount.Password, faker =>
        {
            int byteCount = faker.Random.Number(ModelStateValidation.SocialMediaAccount.MinPasswordChars,
                ModelStateValidation.SocialMediaAccount.MaxPasswordChars);

            return Convert.ToBase64String(faker.Random.Bytes(byteCount));
        })
        .RuleFor(socialMediaAccount => socialMediaAccount.Phone, faker => faker.Person.Phone)
        .RuleFor(socialMediaAccount => socialMediaAccount.Age, faker => faker.Random.Double(0.1, 122.9))
        .RuleFor(socialMediaAccount => socialMediaAccount.ProfilePicture, faker => new Uri(faker.Image.LoremFlickrUrl()))
        .RuleFor(socialMediaAccount => socialMediaAccount.BackgroundPicture, faker => faker.Image.LoremFlickrUrl())
        .RuleFor(socialMediaAccount => socialMediaAccount.Tags, faker => faker.Make(faker.Random.Number(1, 10), () => faker.Random.Word()))
        .RuleFor(socialMediaAccount => socialMediaAccount.CountryCode, faker => faker.Random.ListItem([
            "NL",
            "FR"
        ]))
        .RuleFor(socialMediaAccount => socialMediaAccount.Planet, faker => faker.Random.Word())
        .RuleFor(socialMediaAccount => socialMediaAccount.NextRevalidation, faker => TimeSpan.FromHours(faker.Random.Number(1, 5)))
        .RuleFor(socialMediaAccount => socialMediaAccount.ValidatedAt, faker => faker.Date.Recent().ToUniversalTime().TruncateToWholeMilliseconds())
        .RuleFor(socialMediaAccount => socialMediaAccount.ValidatedAtDate, faker => DateOnly.FromDateTime(faker.Date.Recent()))
        .RuleFor(socialMediaAccount => socialMediaAccount.ValidatedAtTime, faker => TimeOnly.FromDateTime(faker.Date.Recent().TruncateToWholeMilliseconds())));

    public Faker<SocialMediaAccount> SocialMediaAccount => _lazySocialMediaAccountFaker.Value;
}
