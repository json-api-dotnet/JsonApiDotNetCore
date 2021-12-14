using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ReadWrite")]
public sealed class WorkItemGroup : Identifiable<Guid>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public bool IsPublic { get; set; }

    [NotMapped]
    [Attr]
    public bool IsDeprecated => !string.IsNullOrEmpty(Name) && Name.StartsWith("DEPRECATED:", StringComparison.OrdinalIgnoreCase);

    [HasOne]
    public RgbColor? Color { get; set; }

    [HasMany]
    public IList<WorkItem> Items { get; set; } = new List<WorkItem>();
}
