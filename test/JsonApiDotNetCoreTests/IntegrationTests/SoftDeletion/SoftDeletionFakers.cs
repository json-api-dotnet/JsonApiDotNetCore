using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion;

internal sealed class SoftDeletionFakers
{
    private readonly Lazy<Faker<Company>> _lazyCompanyFaker = new(() => new Faker<Company>()
        .MakeDeterministic()
        .RuleFor(company => company.Name, faker => faker.Company.CompanyName()));

    private readonly Lazy<Faker<Department>> _lazyDepartmentFaker = new(() => new Faker<Department>()
        .MakeDeterministic()
        .RuleFor(department => department.Name, faker => faker.Commerce.Department()));

    public Faker<Company> Company => _lazyCompanyFaker.Value;
    public Faker<Department> Department => _lazyDepartmentFaker.Value;
}
