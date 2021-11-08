using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ConcurrencyTokens
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Disk : Identifiable<long>
    {
        [Attr]
        public string Manufacturer { get; set; } = null!;

        [Attr]
        public string SerialCode { get; set; } = null!;

        [ConcurrencyCheck]
        [Timestamp]
        [Attr(PublicName = "concurrencyToken")]
        // ReSharper disable once InconsistentNaming
        public uint xmin { get; set; }

        [HasMany]
        public IList<Partition> Partitions { get; set; } = new List<Partition>();
    }
}
