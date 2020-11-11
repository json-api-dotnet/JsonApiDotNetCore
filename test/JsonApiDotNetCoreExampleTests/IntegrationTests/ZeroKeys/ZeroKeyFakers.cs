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

        public Faker<Game> Game => _lazyGameFaker.Value;
        public Faker<Player> Player => _lazyPlayerFaker.Value;
    }
}
