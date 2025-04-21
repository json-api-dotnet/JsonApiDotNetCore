using Bogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace OpenApiTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class QueryStringFakers
{
    private readonly Lazy<Faker<Node>> _lazyNodeFaker = new(() => new Faker<Node>()
        .MakeDeterministic()
        .RuleFor(node => node.Name, faker => faker.Lorem.Word())
        .RuleFor(node => node.Comment, faker => faker.Lorem.Sentence()));

    private readonly Lazy<Faker<NameValuePair>> _lazyNameValuePairFaker = new(() => new Faker<NameValuePair>()
        .MakeDeterministic()
        .RuleFor(node => node.Name, faker => faker.Lorem.Word())
        .RuleFor(node => node.Value, faker => faker.Lorem.Sentence()));

    public Faker<Node> Node => _lazyNodeFaker.Value;
    public Faker<NameValuePair> NameValuePair => _lazyNameValuePairFaker.Value;
}
