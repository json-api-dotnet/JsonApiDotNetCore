using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState;

internal sealed class ModelStateFakers : FakerContainer
{
    private readonly Lazy<Faker<SystemVolume>> _lazySystemVolumeFaker = new(() =>
        new Faker<SystemVolume>()
            .UseSeed(GetFakerSeed())
            .RuleFor(systemVolume => systemVolume.Name, faker => faker.Lorem.Word()));

    private readonly Lazy<Faker<SystemFile>> _lazySystemFileFaker = new(() =>
        new Faker<SystemFile>()
            .UseSeed(GetFakerSeed())
            .RuleFor(systemFile => systemFile.FileName, faker => faker.System.FileName())
            .RuleFor(systemFile => systemFile.Attributes, faker => faker.Random.Enum(FileAttributes.Normal, FileAttributes.Hidden, FileAttributes.ReadOnly))
            .RuleFor(systemFile => systemFile.SizeInBytes, faker => faker.Random.Long(0, 1_000_000)));

    private readonly Lazy<Faker<SystemDirectory>> _lazySystemDirectoryFaker = new(() =>
        new Faker<SystemDirectory>()
            .UseSeed(GetFakerSeed())
            .RuleFor(systemDirectory => systemDirectory.Name, faker => Path.GetFileNameWithoutExtension(faker.System.FileName()))
            .RuleFor(systemDirectory => systemDirectory.IsCaseSensitive, faker => faker.Random.Bool()));

    public Faker<SystemVolume> SystemVolume => _lazySystemVolumeFaker.Value;
    public Faker<SystemFile> SystemFile => _lazySystemFileFaker.Value;
    public Faker<SystemDirectory> SystemDirectory => _lazySystemDirectoryFaker.Value;
}
