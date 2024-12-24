using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion;

internal sealed class SoftDeletionFakers
{
    private readonly Lazy<Faker<Company>> _lazyCompanyFaker;

    private readonly Lazy<Faker<Department>> _lazyDepartmentFaker;

    public Faker<Company> Company => _lazyCompanyFaker.Value;
    public Faker<Department> Department => _lazyDepartmentFaker.Value;

    public SoftDeletionFakers(DateTimeOffset systemTime)
    {
        DateTime systemTimeUtc = systemTime.UtcDateTime;

        _lazyCompanyFaker = new Lazy<Faker<Company>>(() => new Faker<Company>()
            .MakeDeterministic(systemTimeUtc)
            .RuleFor(company => company.Name, faker => faker.Company.CompanyName()));

        _lazyDepartmentFaker = new Lazy<Faker<Department>>(() => new Faker<Department>()
            .MakeDeterministic(systemTimeUtc)
            .RuleFor(department => department.Name, faker => faker.Commerce.Department()));
    }
}
