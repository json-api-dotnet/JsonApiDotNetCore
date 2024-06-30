using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class OperationsDbContext(ISystemClock systemClock, DbContextOptions<OperationsDbContext> options) : TestableDbContext(options)
{
    internal ISystemClock SystemClock { get; } = systemClock;

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
}
