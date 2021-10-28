using System;
using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState
{
    internal sealed class ModelStateFakers : FakerContainer
    {
        private readonly Lazy<Faker<SystemFile>> _lazySystemFileFaker = new(() =>
            new Faker<SystemFile>()
                .UseSeed(GetFakerSeed())
                .RuleFor(systemFile => systemFile.FileName, faker => faker.System.FileName())
                .RuleFor(systemFile => systemFile.SizeInBytes, faker => faker.Random.Long(0, 1_000_000)));

        private readonly Lazy<Faker<SystemDirectory>> _lazySystemDirectoryFaker = new(() =>
            new Faker<SystemDirectory>()
                .UseSeed(GetFakerSeed())
                .RuleFor(systemDirectory => systemDirectory.Name, faker => faker.Address.City())
                .RuleFor(systemDirectory => systemDirectory.IsCaseSensitive, faker => faker.Random.Bool())
                .RuleFor(systemDirectory => systemDirectory.SizeInBytes, faker => faker.Random.Long(0, 1_000_000)));

        public Faker<SystemFile> SystemFile => _lazySystemFileFaker.Value;
        public Faker<SystemDirectory> SystemDirectory => _lazySystemDirectoryFaker.Value;
    }
}
