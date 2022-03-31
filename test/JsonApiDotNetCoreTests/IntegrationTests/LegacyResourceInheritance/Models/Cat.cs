using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.LegacyResourceInheritance.Models;

[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.LegacyResourceInheritance")]
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Cat : Animal
{
    [Attr]
    public bool ScaredOfDogs { get; set; }

    public Cat()
    {
        Feline = true;
    }
}
