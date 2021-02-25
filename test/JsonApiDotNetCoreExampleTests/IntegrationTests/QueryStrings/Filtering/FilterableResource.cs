using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings.Filtering
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class FilterableResource : Identifiable
    {
        [Attr]
        public string SomeString { get; set; }

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
        public DateTime SomeDateTime { get; set; }

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
        public ICollection<FilterableResource> Children { get; set; }
    }
}
