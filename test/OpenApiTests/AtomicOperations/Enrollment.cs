using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.AtomicOperations")]
public sealed class Enrollment(OperationsDbContext dbContext) : Identifiable<long>
{
    private readonly TimeProvider _timeProvider = dbContext.TimeProvider;

    private DateOnly Today => DateOnly.FromDateTime(_timeProvider.GetUtcNow().Date);

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
