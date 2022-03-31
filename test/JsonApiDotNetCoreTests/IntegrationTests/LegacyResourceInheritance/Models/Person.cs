using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.LegacyResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.LegacyResourceInheritance")]
public abstract class Person : Identifiable<long>
{
    [Attr]
    public bool Retired { get; set; }

    [HasOne]
    public Animal? Pet { get; set; }

    [HasMany]
    public List<Person> Parents { get; set; } = new();

    [HasMany]
    public List<Content> FavoriteContent { get; set; } = new();
}
