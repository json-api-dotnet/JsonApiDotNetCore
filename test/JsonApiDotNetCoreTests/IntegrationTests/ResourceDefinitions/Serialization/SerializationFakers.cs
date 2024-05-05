using Bogus;
using Bogus.Extensions.UnitedStates;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization;

internal sealed class SerializationFakers
{
    private readonly Lazy<Faker<Student>> _lazyStudentFaker = new(() => new Faker<Student>()
        .MakeDeterministic()
        .RuleFor(student => student.Name, faker => faker.Person.FullName)
        .RuleFor(student => student.SocialSecurityNumber, faker => faker.Person.Ssn()));

    private readonly Lazy<Faker<Scholarship>> _lazyScholarshipFaker = new(() => new Faker<Scholarship>()
        .MakeDeterministic()
        .RuleFor(scholarship => scholarship.ProgramName, faker => faker.Commerce.Department())
        .RuleFor(scholarship => scholarship.Amount, faker => faker.Finance.Amount()));

    public Faker<Student> Student => _lazyStudentFaker.Value;
    public Faker<Scholarship> Scholarship => _lazyScholarshipFaker.Value;
}
