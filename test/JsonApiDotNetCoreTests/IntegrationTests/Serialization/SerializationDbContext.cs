using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class SerializationDbContext(DbContextOptions<SerializationDbContext> options) : TestableDbContext(options)
{
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<MeetingAttendee> Attendees => Set<MeetingAttendee>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<MeetingAttendee>()
            .OwnsOne(meetingAttendee => meetingAttendee.HomeAddress);

        base.OnModelCreating(builder);
    }
}
