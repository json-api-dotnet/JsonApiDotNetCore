using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class SerializationDbContext : TestableDbContext
{
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<MeetingAttendee> Attendees => Set<MeetingAttendee>();

    public SerializationDbContext(DbContextOptions<SerializationDbContext> options)
        : base(options)
    {
    }
}
