using System;
using Bogus;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.NamingConventions
{
    internal sealed class SwimmingFakers : FakerContainer
    {
        private readonly Lazy<Faker<SwimmingPool>> _lazySwimmingPoolFaker = new Lazy<Faker<SwimmingPool>>(() =>
            new Faker<SwimmingPool>()
                .UseSeed(GetFakerSeed())
                .RuleFor(swimmingPool => swimmingPool.IsIndoor, f => f.Random.Bool()));

        private readonly Lazy<Faker<WaterSlide>> _lazyWaterSlideFaker = new Lazy<Faker<WaterSlide>>(() =>
            new Faker<WaterSlide>()
                .UseSeed(GetFakerSeed())
                .RuleFor(waterSlide => waterSlide.LengthInMeters, f => f.Random.Decimal(3, 100)));

        private readonly Lazy<Faker<DivingBoard>> _lazyDivingBoardFaker = new Lazy<Faker<DivingBoard>>(() =>
            new Faker<DivingBoard>()
                .UseSeed(GetFakerSeed())
                .RuleFor(divingBoard => divingBoard.HeightInMeters, f => f.Random.Decimal(1, 15)));

        public Faker<SwimmingPool> SwimmingPool => _lazySwimmingPoolFaker.Value;
        public Faker<WaterSlide> WaterSlide => _lazyWaterSlideFaker.Value;
        public Faker<DivingBoard> DivingBoard => _lazyDivingBoardFaker.Value;
    }
}
