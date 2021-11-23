using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SerializationDbContext : DbContext
    {
        public DbSet<Meeting> Meetings => Set<Meeting>();
        public DbSet<MeetingAttendee> Attendees => Set<MeetingAttendee>();

        public SerializationDbContext(DbContextOptions<SerializationDbContext> options)
            : base(options)
        {
        }
    }
}
