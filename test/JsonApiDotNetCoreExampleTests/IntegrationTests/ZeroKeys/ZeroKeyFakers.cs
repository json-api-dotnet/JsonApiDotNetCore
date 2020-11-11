using System;
using Bogus;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ZeroKeys
{
    internal sealed class ZeroKeyFakers : FakerContainer
    {
        private readonly Lazy<Faker<Game>> _lazyGameFaker = new Lazy<Faker<Game>>(() =>
            new Faker<Game>()
                .UseSeed(GetFakerSeed())
                .RuleFor(game => game.Title, f => f.Random.Words()));

        private readonly Lazy<Faker<Player>> _lazyPlayerFaker = new Lazy<Faker<Player>>(() =>
            new Faker<Player>()
                .UseSeed(GetFakerSeed())
                .RuleFor(player => player.Id, f => f.Person.UserName)
                .RuleFor(player => player.EmailAddress, f => f.Person.Email));

        private readonly Lazy<Faker<Map>> _lazyMapFaker = new Lazy<Faker<Map>>(() =>
            new Faker<Map>()
                .UseSeed(GetFakerSeed())
                .RuleFor(map => map.Id, f => f.Random.Guid())
                .RuleFor(map => map.Name, f => f.Random.Words()));

        public Faker<Game> Game => _lazyGameFaker.Value;
        public Faker<Player> Player => _lazyPlayerFaker.Value;
        public Faker<Map> Map => _lazyMapFaker.Value;
    }
}
