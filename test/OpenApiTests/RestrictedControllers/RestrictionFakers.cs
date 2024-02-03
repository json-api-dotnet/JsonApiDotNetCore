using Bogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace OpenApiTests.RestrictedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class RestrictionFakers : FakerContainer
{
    private readonly Lazy<Faker<DataStream>> _lazyDataStreamFaker = new(() => new Faker<DataStream>()
        .UseSeed(GetFakerSeed())
        .RuleFor(stream => stream.BytesTransmitted, faker => faker.Random.ULong()));

    private readonly Lazy<Faker<ReadOnlyChannel>> _lazyReadOnlyChannelFaker = new(() => new Faker<ReadOnlyChannel>()
        .UseSeed(GetFakerSeed())
        .RuleFor(channel => channel.Name, faker => faker.Lorem.Word()));

    private readonly Lazy<Faker<WriteOnlyChannel>> _lazyWriteOnlyChannelFaker = new(() => new Faker<WriteOnlyChannel>()
        .UseSeed(GetFakerSeed())
        .RuleFor(channel => channel.Name, faker => faker.Lorem.Word()));

    private readonly Lazy<Faker<RelationshipChannel>> _lazyRelationshipChannelFaker = new(() => new Faker<RelationshipChannel>()
        .UseSeed(GetFakerSeed())
        .RuleFor(channel => channel.Name, faker => faker.Lorem.Word()));

    private readonly Lazy<Faker<ReadOnlyResourceChannel>> _lazyReadOnlyResourceChannelFaker = new(() => new Faker<ReadOnlyResourceChannel>()
        .UseSeed(GetFakerSeed())
        .RuleFor(channel => channel.Name, faker => faker.Lorem.Word()));

    public Faker<DataStream> DataStream => _lazyDataStreamFaker.Value;
    public Faker<ReadOnlyChannel> ReadOnlyChannel => _lazyReadOnlyChannelFaker.Value;
    public Faker<WriteOnlyChannel> WriteOnlyChannel => _lazyWriteOnlyChannelFaker.Value;
    public Faker<RelationshipChannel> RelationshipChannel => _lazyRelationshipChannelFaker.Value;
    public Faker<ReadOnlyResourceChannel> ReadOnlyResourceChannel => _lazyReadOnlyResourceChannelFaker.Value;
}
