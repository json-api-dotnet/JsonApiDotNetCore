using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.Blobs;

internal sealed class BlobFakers : FakerContainer
{
    private readonly Lazy<Faker<ImageContainer>> _lazyImageContainerFaker = new(() => new Faker<ImageContainer>()
        .UseSeed(GetFakerSeed())
        .RuleFor(imageContainer => imageContainer.FileName, faker => faker.System.FileName())
        .RuleFor(imageContainer => imageContainer.Data, faker => faker.Random.Bytes(128))
        .RuleFor(imageContainer => imageContainer.Thumbnail, faker => faker.Random.Bytes(64)));

    public Faker<ImageContainer> ImageContainer => _lazyImageContainerFaker.Value;
}
