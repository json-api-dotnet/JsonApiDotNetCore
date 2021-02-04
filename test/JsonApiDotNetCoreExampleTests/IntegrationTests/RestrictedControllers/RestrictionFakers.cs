using System;
using Bogus;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RestrictedControllers
{
    internal sealed class RestrictionFakers : FakerContainer
    {
        private readonly Lazy<Faker<Table>> _lazyTableFaker = new Lazy<Faker<Table>>(() =>
            new Faker<Table>()
                .UseSeed(GetFakerSeed()));

        private readonly Lazy<Faker<Chair>> _lazyChairFaker = new Lazy<Faker<Chair>>(() =>
            new Faker<Chair>()
                .UseSeed(GetFakerSeed()));

        private readonly Lazy<Faker<Sofa>> _lazySofaFaker = new Lazy<Faker<Sofa>>(() =>
            new Faker<Sofa>()
                .UseSeed(GetFakerSeed()));

        private readonly Lazy<Faker<Bed>> _lazyBedFaker = new Lazy<Faker<Bed>>(() =>
            new Faker<Bed>()
                .UseSeed(GetFakerSeed()));

        public Faker<Table> Table => _lazyTableFaker.Value;
        public Faker<Chair> Chair => _lazyChairFaker.Value;
        public Faker<Sofa> Sofa => _lazySofaFaker.Value;
        public Faker<Bed> Bed => _lazyBedFaker.Value;
    }
}
