using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering")]
    public sealed class FilterableResource : Identifiable<int>
    {
        [Attr]
        public string SomeString { get; set; } = string.Empty;

        [Attr]
        public string? SomeNullableString { get; set; }

        [Attr]
        public bool SomeBoolean { get; set; }

        [Attr]
        public bool? SomeNullableBoolean { get; set; }

        [Attr]
        public int SomeInt32 { get; set; }

        [Attr]
        public int? SomeNullableInt32 { get; set; }

        [Attr]
        public int OtherInt32 { get; set; }

        [Attr]
        public int? OtherNullableInt32 { get; set; }

        [Attr]
        public ulong SomeUnsignedInt64 { get; set; }

        [Attr]
        public ulong? SomeNullableUnsignedInt64 { get; set; }

        [Attr]
        public decimal SomeDecimal { get; set; }

        [Attr]
        public decimal? SomeNullableDecimal { get; set; }

        [Attr]
        public double SomeDouble { get; set; }

        [Attr]
        public double? SomeNullableDouble { get; set; }

        [Attr]
        public Guid SomeGuid { get; set; }

        [Attr]
        public Guid? SomeNullableGuid { get; set; }

        [Attr]
        public DateTime SomeDateTimeInLocalZone { get; set; }

        [Attr]
        public DateTime SomeDateTimeInUtcZone { get; set; }

        [Attr]
        public DateTime? SomeNullableDateTime { get; set; }

        [Attr]
        public DateTimeOffset SomeDateTimeOffset { get; set; }

        [Attr]
        public DateTimeOffset? SomeNullableDateTimeOffset { get; set; }

        [Attr]
        public TimeSpan SomeTimeSpan { get; set; }

        [Attr]
        public TimeSpan? SomeNullableTimeSpan { get; set; }

        [Attr]
        public DayOfWeek SomeEnum { get; set; }

        [Attr]
        public DayOfWeek? SomeNullableEnum { get; set; }

        [HasMany]
        public ICollection<FilterableResource> Children { get; set; } = new List<FilterableResource>();
    }
}
