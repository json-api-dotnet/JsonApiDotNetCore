using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection")]
public sealed class Postcard : Identifiable<long>
{
    [Attr]
    public string Message { get; set; } = null!;

    // Not mapped, because EF Core cannot map a type that needs a constructor dependency. The request fails before anything is stored.
    [Attr(IsCompound = true)]
    [NotMapped]
    public Stamp? Stamp { get; set; }
}
