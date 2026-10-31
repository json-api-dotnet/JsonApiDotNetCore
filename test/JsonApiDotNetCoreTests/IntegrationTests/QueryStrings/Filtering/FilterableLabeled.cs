using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class FilterableLabeled<T>
{
    [Attr]
    public T Value { get; set; } = default!;

    [Attr]
    public string? Label { get; set; }
}
