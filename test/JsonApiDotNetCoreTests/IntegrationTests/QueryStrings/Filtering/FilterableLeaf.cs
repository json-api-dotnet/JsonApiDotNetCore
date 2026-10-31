using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class FilterableLeaf
{
    [Attr]
    public string? SomeString { get; set; }

    [Attr]
    public int? SomeInt32 { get; set; }
}
