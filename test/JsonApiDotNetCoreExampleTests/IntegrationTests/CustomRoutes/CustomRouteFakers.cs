using System;
using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CustomRoutes
{
    internal sealed class CustomRouteFakers : FakerContainer
    {
        private readonly Lazy<Faker<Town>> _lazyTownFaker = new Lazy<Faker<Town>>(() =>
            new Faker<Town>()
                .UseSeed(GetFakerSeed())
                .RuleFor(town => town.Name, f => f.Address.City())
                .RuleFor(town => town.Latitude, f => f.Address.Latitude())
                .RuleFor(town => town.Longitude, f => f.Address.Longitude()));

        private readonly Lazy<Faker<Civilian>> _lazyCivilianFaker = new Lazy<Faker<Civilian>>(() =>
            new Faker<Civilian>()
                .UseSeed(GetFakerSeed())
                .RuleFor(civilian => civilian.Name, f => f.Person.FullName));

        public Faker<Town> Town => _lazyTownFaker.Value;
        public Faker<Civilian> Civilian => _lazyCivilianFaker.Value;
    }
}
