using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance")]
public sealed class GenericFeature : Identifiable<long>
{
    [Attr]
    public string Description { get; set; } = null!;

    [HasMany]
    public ISet<GenericProperty> Properties { get; set; } = new HashSet<GenericProperty>();
}
