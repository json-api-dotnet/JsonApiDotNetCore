using Bogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace OpenApiTests.ModelValidation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ModelValidationFakers : FakerContainer
{
    private readonly Lazy<Faker<Fingerprint>> _lazyFingerprintFaker = new(() => new Faker<Fingerprint>()
        .UseSeed(GetFakerSeed())
        .RuleFor(fingerprint => fingerprint.FirstName, faker => faker.Person.FirstName)
        .RuleFor(fingerprint => fingerprint.LastName, faker => faker.Person.LastName)
        .RuleFor(fingerprint => fingerprint.UserName, faker => faker.Random.String2(3, 18))
        .RuleFor(fingerprint => fingerprint.CreditCard, faker => faker.Finance.CreditCardNumber())
        .RuleFor(fingerprint => fingerprint.Email, faker => faker.Person.Email)
        .RuleFor(fingerprint => fingerprint.Phone, faker => faker.Person.Phone)
        .RuleFor(fingerprint => fingerprint.Age, faker => faker.Random.Number(0, 123))
        .RuleFor(fingerprint => fingerprint.Tags, faker => faker.Make(faker.Random.Number(0, 10), () => faker.Random.String2(3)))
        .RuleFor(fingerprint => fingerprint.ProfilePicture, faker => new Uri(faker.Image.LoremFlickrUrl()))
        .RuleFor(fingerprint => fingerprint.NextRevalidation, faker => TimeSpan.FromMinutes(faker.Random.Number(1, 5)))
        .RuleFor(fingerprint => fingerprint.ValidatedAt, faker => faker.Date.Recent())
        .RuleFor(fingerprint => fingerprint.ValidatedDateAt, faker => DateOnly.FromDateTime(faker.Date.Recent()))
        .RuleFor(fingerprint => fingerprint.ValidatedTimeAt, faker => TimeOnly.FromDateTime(faker.Date.Recent()))
        .RuleFor(fingerprint => fingerprint.Signature, faker => Convert.ToBase64String(faker.Random.Bytes(10))));

    public Faker<Fingerprint> Fingerprint => _lazyFingerprintFaker.Value;
}
