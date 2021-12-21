using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

internal sealed class IdempotencyFakers : FakerContainer
{
    private readonly Lazy<Faker<Tree>> _lazyTreeFaker = new(() =>
        new Faker<Tree>()
            .UseSeed(GetFakerSeed())
            .RuleFor(tree => tree.HeightInMeters, faker => faker.Random.Decimal(0.1m, 100)));

    private readonly Lazy<Faker<Branch>> _lazyBranchFaker = new(() =>
        new Faker<Branch>()
            .UseSeed(GetFakerSeed())
            .RuleFor(branch => branch.LengthInMeters, faker => faker.Random.Decimal(0.1m, 20)));

    private readonly Lazy<Faker<Leaf>> _lazyLeafFaker = new(() =>
        new Faker<Leaf>()
            .UseSeed(GetFakerSeed())
            .RuleFor(leaf => leaf.Color, faker => faker.Commerce.Color()));

    public Faker<Tree> Tree => _lazyTreeFaker.Value;
    public Faker<Branch> Branch => _lazyBranchFaker.Value;
    public Faker<Leaf> Leaf => _lazyLeafFaker.Value;
}
