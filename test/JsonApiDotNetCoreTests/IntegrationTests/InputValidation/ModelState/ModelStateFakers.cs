using System.Globalization;
using Bogus;
using TestBuildingBlocks;

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

    public Faker<SystemVolume> SystemVolume => _lazySystemVolumeFaker.Value;
    public Faker<SystemFile> SystemFile => _lazySystemFileFaker.Value;
    public Faker<SystemDirectory> SystemDirectory => _lazySystemDirectoryFaker.Value;
}
