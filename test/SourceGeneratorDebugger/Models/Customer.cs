using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace SourceGeneratorDebugger.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Customer : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;
}
