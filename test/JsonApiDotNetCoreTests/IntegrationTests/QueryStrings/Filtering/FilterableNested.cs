using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class FilterableNested
{
    [Attr]
    public string SomeString { get; set; } = string.Empty;

    [Attr]
    public string? SomeNullableString { get; set; }

    [Attr]
    public int SomeInt32 { get; set; }

    [Attr]
    public int? SomeNullableInt32 { get; set; }

    [Attr]
    public bool SomeBoolean { get; set; }

    [Attr]
    public DateTime SomeDateTime { get; set; }

    [Attr]
    public Guid SomeGuid { get; set; }

    [Attr]
    public TimeSpan SomeTimeSpan { get; set; }

    [Attr]
    public DayOfWeek SomeEnum { get; set; }

    [Attr]
    public List<string> SomeStrings { get; set; } = [];

    [Attr]
    public List<int?> SomeNullableInt32s { get; set; } = [];

    [Attr(IsCompound = true)]
    public FilterableLeaf? Child { get; set; }
}
