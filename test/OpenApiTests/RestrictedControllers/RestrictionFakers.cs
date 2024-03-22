using Bogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace OpenApiTests.RestrictedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class RestrictionFakers : FakerContainer
{
    private static readonly IList<bool?> NullableBooleanValues =
    [
        true,
        false,
        null
    ];

    private readonly Lazy<Faker<DataStream>> _lazyDataStreamFaker = new(() => new Faker<DataStream>()
        .UseSeed(GetFakerSeed())
        .RuleFor(stream => stream.BytesTransmitted, faker => (ulong)faker.Random.Long(0)));

    private readonly Lazy<Faker<ReadOnlyChannel>> _lazyReadOnlyChannelFaker = new(() => new Faker<ReadOnlyChannel>()
        .UseSeed(GetFakerSeed())
        .RuleFor(channel => channel.Name, faker => faker.Lorem.Word())
        .RuleFor(channel => channel.IsCommercial, faker => faker.PickRandom(NullableBooleanValues))
        .RuleFor(channel => channel.IsAdultOnly, faker => faker.PickRandom(NullableBooleanValues)));

    private readonly Lazy<Faker<WriteOnlyChannel>> _lazyWriteOnlyChannelFaker = new(() => new Faker<WriteOnlyChannel>()
        .UseSeed(GetFakerSeed())
        .RuleFor(channel => channel.Name, faker => faker.Lorem.Word())
        .RuleFor(channel => channel.IsCommercial, faker => faker.PickRandom(NullableBooleanValues))
        .RuleFor(channel => channel.IsAdultOnly, faker => faker.PickRandom(NullableBooleanValues)));

    private readonly Lazy<Faker<RelationshipChannel>> _lazyRelationshipChannelFaker = new(() => new Faker<RelationshipChannel>()
        .UseSeed(GetFakerSeed())
        .RuleFor(channel => channel.Name, faker => faker.Lorem.Word())
        .RuleFor(channel => channel.IsCommercial, faker => faker.PickRandom(NullableBooleanValues))
        .RuleFor(channel => channel.IsAdultOnly, faker => faker.PickRandom(NullableBooleanValues)));

    private readonly Lazy<Faker<ReadOnlyResourceChannel>> _lazyReadOnlyResourceChannelFaker = new(() => new Faker<ReadOnlyResourceChannel>()
        .UseSeed(GetFakerSeed())
        .RuleFor(channel => channel.Name, faker => faker.Lorem.Word())
        .RuleFor(channel => channel.IsCommercial, faker => faker.PickRandom(NullableBooleanValues))
        .RuleFor(channel => channel.IsAdultOnly, faker => faker.PickRandom(NullableBooleanValues)));

    public Faker<DataStream> DataStream => _lazyDataStreamFaker.Value;
    public Faker<ReadOnlyChannel> ReadOnlyChannel => _lazyReadOnlyChannelFaker.Value;
    public Faker<WriteOnlyChannel> WriteOnlyChannel => _lazyWriteOnlyChannelFaker.Value;
    public Faker<RelationshipChannel> RelationshipChannel => _lazyRelationshipChannelFaker.Value;
    public Faker<ReadOnlyResourceChannel> ReadOnlyResourceChannel => _lazyReadOnlyResourceChannelFaker.Value;
}
