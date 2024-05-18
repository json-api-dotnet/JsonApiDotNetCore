using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices;

internal sealed class DomainFakers
{
    private readonly Lazy<Faker<DomainUser>> _lazyDomainUserFaker = new(() => new Faker<DomainUser>()
        .MakeDeterministic()
        .RuleFor(domainUser => domainUser.LoginName, faker => faker.Person.UserName)
        .RuleFor(domainUser => domainUser.DisplayName, faker => faker.Person.FullName));

    private readonly Lazy<Faker<DomainGroup>> _lazyDomainGroupFaker = new(() => new Faker<DomainGroup>()
        .MakeDeterministic()
        .RuleFor(domainGroup => domainGroup.Name, faker => faker.Commerce.Department()));

    public Faker<DomainUser> DomainUser => _lazyDomainUserFaker.Value;
    public Faker<DomainGroup> DomainGroup => _lazyDomainGroupFaker.Value;
}
