using System;
using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.HostingInIIS
{
    internal sealed class HostingFakers : FakerContainer
    {
        private readonly Lazy<Faker<ArtGallery>> _lazyArtGalleryFaker = new Lazy<Faker<ArtGallery>>(() =>
            new Faker<ArtGallery>()
                .UseSeed(GetFakerSeed())
                .RuleFor(artGallery => artGallery.Theme, f => f.Lorem.Word()));

        private readonly Lazy<Faker<Painting>> _lazyPaintingFaker = new Lazy<Faker<Painting>>(() =>
            new Faker<Painting>()
                .UseSeed(GetFakerSeed())
                .RuleFor(painting => painting.Title, f => f.Lorem.Sentence()));

        public Faker<ArtGallery> ArtGallery => _lazyArtGalleryFaker.Value;
        public Faker<Painting> Painting => _lazyPaintingFaker.Value;
    }
}
