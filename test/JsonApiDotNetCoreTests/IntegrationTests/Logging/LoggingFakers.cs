using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging;

internal sealed class LoggingFakers
{
    private readonly Lazy<Faker<AuditEntry>> _lazyAuditEntryFaker = new(() => new Faker<AuditEntry>()
        .MakeDeterministic()
        .RuleFor(auditEntry => auditEntry.UserName, faker => faker.Internet.UserName())
        .RuleFor(auditEntry => auditEntry.CreatedAt, faker => faker.Date.PastOffset().TruncateToWholeMilliseconds()));

    private readonly Lazy<Faker<Banana>> _lazyBananaFaker = new(() => new Faker<Banana>()
        .MakeDeterministic()
        .RuleFor(banana => banana.WeightInKilograms, faker => faker.Random.Double(.2, .3))
        .RuleFor(banana => banana.LengthInCentimeters, faker => faker.Random.Double(10, 25)));

    private readonly Lazy<Faker<Peach>> _lazyPeachFaker = new(() => new Faker<Peach>()
        .MakeDeterministic()
        .RuleFor(peach => peach.WeightInKilograms, faker => faker.Random.Double(.2, .3))
        .RuleFor(peach => peach.DiameterInCentimeters, faker => faker.Random.Double(6, 7.5)));

    public Faker<AuditEntry> AuditEntry => _lazyAuditEntryFaker.Value;
    public Faker<Banana> Banana => _lazyBananaFaker.Value;
    public Faker<Peach> Peach => _lazyPeachFaker.Value;
}
