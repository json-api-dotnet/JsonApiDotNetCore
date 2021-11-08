using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ConcurrencyTokens
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Partition : Identifiable<long>
    {
        [Attr]
        public string MountPoint { get; set; } = null!;

        [Attr]
        public string FileSystem { get; set; } = null!;

        [Attr]
        public ulong CapacityInBytes { get; set; }

        [Attr]
        public ulong FreeSpaceInBytes { get; set; }

        [Attr(PublicName = "concurrencyToken")]
        // ReSharper disable once InconsistentNaming
        public uint xmin { get; set; }

        [HasOne]
        public Disk Owner { get; set; } = null!;
    }
}
