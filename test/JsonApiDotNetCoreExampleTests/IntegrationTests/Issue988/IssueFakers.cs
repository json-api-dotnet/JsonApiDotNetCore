using System;
using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Issue988
{
    internal sealed class IssueFakers : FakerContainer
    {
        private readonly Lazy<Faker<Engagement>> _lazyEngagementFaker = new Lazy<Faker<Engagement>>(() =>
            new Faker<Engagement>()
                .UseSeed(GetFakerSeed())
                .RuleFor(engagement => engagement.Name, faker => faker.Lorem.Word()));

        private readonly Lazy<Faker<EngagementParty>> _lazyEngagementPartyFaker = new Lazy<Faker<EngagementParty>>(() =>
            new Faker<EngagementParty>()
                .UseSeed(GetFakerSeed())
                .RuleFor(party => party.Role, faker => faker.Lorem.Word())
                .RuleFor(party => party.ShortName, faker => faker.Lorem.Word()));

        public Faker<Engagement> Engagement => _lazyEngagementFaker.Value;
        public Faker<EngagementParty> EngagementParty => _lazyEngagementPartyFaker.Value;
    }
}
