using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization")]
public sealed class Student : Identifiable<int>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public string SocialSecurityNumber { get; set; } = null!;

    [HasOne]
    public Scholarship? Scholarship { get; set; }
}
