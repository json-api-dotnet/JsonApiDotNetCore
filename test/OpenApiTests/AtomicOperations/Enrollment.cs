using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using TestBuildingBlocks;

namespace OpenApiTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.AtomicOperations")]
public sealed class Enrollment(OperationsDbContext dbContext) : Identifiable<long>
{
    private readonly ISystemClock _systemClock = dbContext.SystemClock;

    private DateOnly Today => DateOnly.FromDateTime(_systemClock.UtcNow.Date);

    [Attr]
    public DateOnly EnrolledAt { get; set; }

    [Attr]
    public DateOnly? GraduatedAt { get; set; }

    [Attr]
    public bool HasGraduated => GraduatedAt != null && GraduatedAt <= Today;

    [HasOne]
    public Student Student { get; set; } = null!;

    [HasOne]
    public Course Course { get; set; } = null!;
}
