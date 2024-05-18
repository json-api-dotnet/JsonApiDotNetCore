using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS;

internal sealed class HostingFakers
{
    private readonly Lazy<Faker<ArtGallery>> _lazyArtGalleryFaker = new(() => new Faker<ArtGallery>()
        .MakeDeterministic()
        .RuleFor(artGallery => artGallery.Theme, faker => faker.Lorem.Word()));

    private readonly Lazy<Faker<Painting>> _lazyPaintingFaker = new(() => new Faker<Painting>()
        .MakeDeterministic()
        .RuleFor(painting => painting.Title, faker => faker.Lorem.Sentence()));

    public Faker<ArtGallery> ArtGallery => _lazyArtGalleryFaker.Value;
    public Faker<Painting> Painting => _lazyPaintingFaker.Value;
}
