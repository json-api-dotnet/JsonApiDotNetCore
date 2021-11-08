using System;
using Bogus;
using JsonApiDotNetCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreTests.IntegrationTests.ConcurrencyTokens
{
    internal sealed class ConcurrencyFakers : FakerContainer
    {
        private const ulong OneGigabyte = 1024 * 1024 * 1024;
        private static readonly string[] KnownFileSystems = ArrayFactory.Create("NTFS", "FAT32", "ext4", "XFS", "ZFS", "btrfs");

        private readonly Lazy<Faker<Disk>> _lazyDiskFaker = new(() =>
            new Faker<Disk>().UseSeed(GetFakerSeed())
                .RuleFor(disk => disk.Manufacturer, faker => faker.Company.CompanyName())
                .RuleFor(disk => disk.SerialCode, faker => faker.System.ApplePushToken()));

        private readonly Lazy<Faker<Partition>> _lazyPartitionFaker = new(() =>
            new Faker<Partition>().UseSeed(GetFakerSeed())
                .RuleFor(partition => partition.MountPoint, faker => faker.System.DirectoryPath())
                .RuleFor(partition => partition.FileSystem, faker => faker.PickRandom(KnownFileSystems))
                .RuleFor(partition => partition.CapacityInBytes, faker => faker.Random.ULong(OneGigabyte * 50, OneGigabyte * 100))
                .RuleFor(partition => partition.FreeSpaceInBytes, faker => faker.Random.ULong(OneGigabyte * 10, OneGigabyte * 40)));

        public Faker<Disk> Disk => _lazyDiskFaker.Value;
        public Faker<Partition> Partition => _lazyPartitionFaker.Value;
    }
}
