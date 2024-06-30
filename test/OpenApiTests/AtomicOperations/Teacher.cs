using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.AtomicOperations")]
public sealed class Teacher : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public string? EmailAddress { get; set; }

    [HasMany]
    public ISet<Course> Teaches { get; set; } = new HashSet<Course>();

    [HasMany]
    public ISet<Student> Mentors { get; set; } = new HashSet<Student>();
}
