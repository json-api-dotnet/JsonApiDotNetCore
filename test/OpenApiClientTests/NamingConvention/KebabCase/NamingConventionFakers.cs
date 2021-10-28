using System;
using Bogus;
using OpenApiTests.NamingConvention;
using TestBuildingBlocks;

namespace OpenApiClientTests.NamingConvention.KebabCase
{
    internal sealed class NamingConventionFakers : FakerContainer
    {
        private readonly Lazy<Faker<Supermarket>> _lazySupermarketFaker = new(() => new Faker<Supermarket>().UseSeed(GetFakerSeed())
            .RuleFor(supermarket => supermarket.NameOfCity, faker => faker.Address.City())
            .RuleFor(supermarket => supermarket.Kind, faker => faker.PickRandom<SupermarketType>()));

        public Faker<Supermarket> Supermarket => _lazySupermarketFaker.Value;
    }
}
