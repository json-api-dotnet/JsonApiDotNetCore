using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization")]
public sealed class Student : Identifiable<long>
{
    [Attr]
    public required string Name { get; set; }

    [Attr]
    public required string SocialSecurityNumber { get; set; }

    [HasOne]
    public Scholarship? Scholarship { get; set; }
}
