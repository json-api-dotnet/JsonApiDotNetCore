using System;
using Bogus;

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

        public Faker<PhotoAlbum> PhotoAlbum => _lazyPhotoAlbumFaker.Value;
        public Faker<Photo> Photo => _lazyPhotoFaker.Value;
    }
}
