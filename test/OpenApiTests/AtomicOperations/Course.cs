using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.AtomicOperations", GenerateControllerEndpoints = JsonApiEndpoints.All & ~JsonApiEndpoints.DeleteRelationship,
    ClientIdGeneration = ClientIdGenerationMode.Required)]
public sealed class Course : Identifiable<Guid>
{
    [Attr]
    public string Subject { get; set; } = null!;

    [Attr]
    public string? Description { get; set; }

    [HasMany(Capabilities = HasManyCapabilities.All & ~HasManyCapabilities.AllowSet)]
    public ISet<Teacher> TaughtBy { get; set; } = new HashSet<Teacher>();

    [HasMany]
    public ISet<Enrollment> Enrollments { get; set; } = new HashSet<Enrollment>();
}
