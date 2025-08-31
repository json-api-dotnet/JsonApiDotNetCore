using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.IdCompaction;

internal sealed class IdCompationFakers
{
    private readonly Lazy<Faker<Grant>> _lazyGrantFaker = new(() => new Faker<Grant>()
        .MakeDeterministic()
        .RuleFor(grant => grant.Name, faker => faker.Company.CompanyName()));

    public Faker<Grant> Grants => _lazyGrantFaker.Value;
}
