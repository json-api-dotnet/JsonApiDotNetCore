using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.AtomicOperations", GenerateControllerEndpoints = JsonApiEndpoints.All & ~JsonApiEndpoints.Delete)]
public sealed class Student : Identifiable<long>
{
    [Attr]
    public required string Name { get; set; }

    [Attr]
    public string? EmailAddress { get; set; }

    [HasOne]
    public Teacher? Mentor { get; set; }

    [HasMany]
    public ISet<Enrollment> Enrollments { get; set; } = new HashSet<Enrollment>();
}
