using System;
using Bogus;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Links
{
    internal sealed class LinksFakers : FakerContainer
    {
        private readonly Lazy<Faker<PhotoAlbum>> _lazyPhotoAlbumFaker = new Lazy<Faker<PhotoAlbum>>(() =>
            new Faker<PhotoAlbum>()
                .UseSeed(GetFakerSeed())
                .RuleFor(photoAlbum => photoAlbum.Name, f => f.Lorem.Sentence()));

        private readonly Lazy<Faker<Photo>> _lazyPhotoFaker = new Lazy<Faker<Photo>>(() =>
            new Faker<Photo>()
                .UseSeed(GetFakerSeed())
                .RuleFor(photo => photo.Url, f => f.Image.PlaceImgUrl()));

        private readonly Lazy<Faker<PhotoLocation>> _lazyPhotoLocationFaker = new Lazy<Faker<PhotoLocation>>(() =>
            new Faker<PhotoLocation>()
                .UseSeed(GetFakerSeed())
                .RuleFor(photoLocation => photoLocation.PlaceName, f => f.Address.FullAddress())
                .RuleFor(photoLocation => photoLocation.Latitude, f => f.Address.Latitude())
                .RuleFor(photoLocation => photoLocation.Longitude, f => f.Address.Longitude()));

        public Faker<PhotoAlbum> PhotoAlbum => _lazyPhotoAlbumFaker.Value;
        public Faker<Photo> Photo => _lazyPhotoFaker.Value;
        public Faker<PhotoLocation> PhotoLocation => _lazyPhotoLocationFaker.Value;
    }
}
