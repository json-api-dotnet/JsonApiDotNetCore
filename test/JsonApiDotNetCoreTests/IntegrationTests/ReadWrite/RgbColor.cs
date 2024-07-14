using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ReadWrite")]
public sealed class RgbColor : Identifiable<string?>
{
#if NET6_0
    // Workaround for bug in .NET 6, see https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1153.
    public override string? Id { get; set; }
#endif

    [Attr]
    public string DisplayName { get; set; } = null!;

    [HasOne]
    public WorkItemGroup? Group { get; set; }
}
