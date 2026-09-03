using System.Globalization;
using Bogus;
using TestBuildingBlocks;
using SocialMediaAccountType = JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState.SocialMediaAccount;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState;

internal sealed class ModelStateFakers
{
    private static readonly DateOnly MinCreatedOn = DateOnly.Parse("2000-01-01", CultureInfo.InvariantCulture);
    private static readonly DateOnly MaxCreatedOn = DateOnly.Parse("2050-01-01", CultureInfo.InvariantCulture);

    private static readonly TimeOnly MinCreatedAt = TimeOnly.Parse("09:00:00", CultureInfo.InvariantCulture);
    private static readonly TimeOnly MaxCreatedAt = TimeOnly.Parse("17:30:00", CultureInfo.InvariantCulture);

    private readonly Lazy<Faker<SystemVolume>> _lazySystemVolumeFaker = new(() => new Faker<SystemVolume>()
        .MakeDeterministic()
        .RuleFor(systemVolume => systemVolume.Name, faker => faker.Lorem.Word()));

    private readonly Lazy<Faker<SystemFile>> _lazySystemFileFaker = new(() => new Faker<SystemFile>()
        .MakeDeterministic()
        .RuleFor(systemFile => systemFile.FileName, faker => faker.System.FileName())
        .RuleFor(systemFile => systemFile.Attributes, faker => faker.Random.Enum(FileAttributes.Normal, FileAttributes.Hidden, FileAttributes.ReadOnly))
        .RuleFor(systemFile => systemFile.SizeInBytes, faker => faker.Random.Long(0, 1_000_000))
        .RuleFor(systemFile => systemFile.CreatedOn, faker => faker.Date.BetweenDateOnly(MinCreatedOn, MaxCreatedOn))
        .RuleFor(systemFile => systemFile.CreatedAt, faker => faker.Date.BetweenTimeOnly(MinCreatedAt, MaxCreatedAt)));

    private readonly Lazy<Faker<SystemDirectory>> _lazySystemDirectoryFaker = new(() => new Faker<SystemDirectory>()
        .MakeDeterministic()
        .RuleFor(systemDirectory => systemDirectory.Name, faker => Path.GetFileNameWithoutExtension(faker.System.FileName()))
        .RuleFor(systemDirectory => systemDirectory.IsCaseSensitive, faker => faker.Random.Bool()));

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
            int byteCount = faker.Random.Number(SocialMediaAccountType.MinPasswordChars, SocialMediaAccountType.MaxPasswordChars);
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
        .RuleFor(socialMediaAccount => socialMediaAccount.ValidatedAtDate, faker => faker.Date.RecentDateOnly())
        .RuleFor(socialMediaAccount => socialMediaAccount.ValidatedAtTime, faker => faker.Date.RecentTimeOnly().TruncateToWholeMilliseconds()));

    public Faker<SystemVolume> SystemVolume => _lazySystemVolumeFaker.Value;
    public Faker<SystemFile> SystemFile => _lazySystemFileFaker.Value;
    public Faker<SystemDirectory> SystemDirectory => _lazySystemDirectoryFaker.Value;
    public Faker<SocialMediaAccount> SocialMediaAccount => _lazySocialMediaAccountFaker.Value;
}
