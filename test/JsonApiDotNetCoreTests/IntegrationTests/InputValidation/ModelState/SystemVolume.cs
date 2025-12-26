using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState")]
public sealed class SystemVolume : Identifiable<long>
{
    [Attr]
    public string? Name { get; set; }

    [HasOne]
    public required SystemDirectory RootDirectory { get; set; }
}
