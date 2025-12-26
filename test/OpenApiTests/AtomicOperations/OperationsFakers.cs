using Bogus;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace OpenApiTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class OperationsFakers
{
    private readonly Lazy<Faker<Course>> _lazyCourseFaker = new(() => new Faker<Course>()
        .MakeDeterministic()
        .CustomInstantiator(faker => new Course
        {
            Subject = faker.Lorem.Word(),
            Description = faker.Lorem.Sentence()
        }));

    private readonly Lazy<Faker<Teacher>> _lazyTeacherFaker = new(() => new Faker<Teacher>()
        .MakeDeterministic()
        .RuleFor(teacher => teacher.Name, faker => faker.Person.FullName)
        .RuleFor(teacher => teacher.EmailAddress, faker => faker.Person.Email));

    private readonly Lazy<Faker<Student>> _lazyStudentFaker = new(() => new Faker<Student>()
        .MakeDeterministic()
        .CustomInstantiator(faker => new Student
        {
            Name = faker.Person.FullName,
            EmailAddress = faker.Person.Email
        }));

    private readonly Lazy<Faker<Enrollment>> _lazyEnrollmentFaker;

    public Faker<Course> Course => _lazyCourseFaker.Value;
    public Faker<Teacher> Teacher => _lazyTeacherFaker.Value;
    public Faker<Student> Student => _lazyStudentFaker.Value;
    public Faker<Enrollment> Enrollment => _lazyEnrollmentFaker.Value;

    public OperationsFakers(IServiceProvider serviceProvider)
    {
        _lazyEnrollmentFaker = new Lazy<Faker<Enrollment>>(() => new Faker<Enrollment>()
            .MakeDeterministic()
            .CustomInstantiator(faker =>
            {
                var dbContext = ResolveDbContext(serviceProvider);
                return new Enrollment(dbContext)
                {
                    Student = new Student { Name = faker.Name.FullName() },
                    Course = new Course { Subject = faker.Commerce.Department() },
                    EnrolledAt = faker.Date.PastDateOnly(),
                    GraduatedAt = faker.Date.RecentDateOnly()
                };
            }));
    }

    private OperationsDbContext ResolveDbContext(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<OperationsDbContext>();
    }
}
