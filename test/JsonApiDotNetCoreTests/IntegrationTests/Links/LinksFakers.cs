using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.Links;

internal sealed class LinksFakers
{
    private readonly Lazy<Faker<PhotoAlbum>> _lazyPhotoAlbumFaker = new(() => new Faker<PhotoAlbum>()
        .MakeDeterministic()
        .RuleFor(photoAlbum => photoAlbum.Name, faker => faker.Lorem.Sentence()));

    private readonly Lazy<Faker<Photo>> _lazyPhotoFaker = new(() => new Faker<Photo>()
        .MakeDeterministic()
        .RuleFor(photo => photo.Url, faker => faker.Image.PlaceImgUrl()));

    private readonly Lazy<Faker<PhotoLocation>> _lazyPhotoLocationFaker = new(() => new Faker<PhotoLocation>()
        .MakeDeterministic()
        .RuleFor(photoLocation => photoLocation.PlaceName, faker => faker.Address.FullAddress())
        .RuleFor(photoLocation => photoLocation.Latitude, faker => faker.Address.Latitude())
        .RuleFor(photoLocation => photoLocation.Longitude, faker => faker.Address.Longitude()));

    public Faker<PhotoAlbum> PhotoAlbum => _lazyPhotoAlbumFaker.Value;
    public Faker<Photo> Photo => _lazyPhotoFaker.Value;
    public Faker<PhotoLocation> PhotoLocation => _lazyPhotoLocationFaker.Value;
}
