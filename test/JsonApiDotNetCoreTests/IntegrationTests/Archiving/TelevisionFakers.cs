using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving;

internal sealed class TelevisionFakers
{
    private readonly Lazy<Faker<TelevisionNetwork>> _lazyTelevisionNetworkFaker = new(() => new Faker<TelevisionNetwork>()
        .MakeDeterministic()
        .RuleFor(network => network.Name, faker => faker.Company.CompanyName()));

    private readonly Lazy<Faker<TelevisionStation>> _lazyTelevisionStationFaker = new(() => new Faker<TelevisionStation>()
        .MakeDeterministic()
        .RuleFor(station => station.Name, faker => faker.Company.CompanyName()));

    private readonly Lazy<Faker<TelevisionBroadcast>> _lazyTelevisionBroadcastFaker = new(() => new Faker<TelevisionBroadcast>()
        .MakeDeterministic()
        .RuleFor(broadcast => broadcast.Title, faker => faker.Lorem.Sentence())
        .RuleFor(broadcast => broadcast.AiredAt, faker => faker.Date.PastOffset().TruncateToWholeMilliseconds())
        .RuleFor(broadcast => broadcast.ArchivedAt, faker => faker.Date.RecentOffset().TruncateToWholeMilliseconds()));

    private readonly Lazy<Faker<BroadcastComment>> _lazyBroadcastCommentFaker = new(() => new Faker<BroadcastComment>()
        .MakeDeterministic()
        .RuleFor(comment => comment.Text, faker => faker.Lorem.Paragraph())
        .RuleFor(comment => comment.CreatedAt, faker => faker.Date.PastOffset().TruncateToWholeMilliseconds()));

    public Faker<TelevisionNetwork> TelevisionNetwork => _lazyTelevisionNetworkFaker.Value;
    public Faker<TelevisionStation> TelevisionStation => _lazyTelevisionStationFaker.Value;
    public Faker<TelevisionBroadcast> TelevisionBroadcast => _lazyTelevisionBroadcastFaker.Value;
    public Faker<BroadcastComment> BroadcastComment => _lazyBroadcastCommentFaker.Value;
}
