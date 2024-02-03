using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.RestrictedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class RestrictionDbContext(DbContextOptions<RestrictionDbContext> options) : TestableDbContext(options)
{
    public DbSet<DataStream> DataStreams => Set<DataStream>();
    public DbSet<ReadOnlyChannel> ReadOnlyChannels => Set<ReadOnlyChannel>();
    public DbSet<WriteOnlyChannel> WriteOnlyChannels => Set<WriteOnlyChannel>();
    public DbSet<RelationshipChannel> RelationshipChannels => Set<RelationshipChannel>();
    public DbSet<ReadOnlyResourceChannel> ReadOnlyResourceChannels => Set<ReadOnlyResourceChannel>();
}
