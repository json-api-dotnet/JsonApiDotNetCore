using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion")]
public sealed class Company : Identifiable<long>, ISoftDeletable
{
    [Attr]
    public required string Name { get; set; }

    public DateTimeOffset? SoftDeletedAt { get; set; }

    [HasMany]
    public ICollection<Department> Departments { get; set; } = new List<Department>();
}
