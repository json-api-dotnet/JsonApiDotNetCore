#nullable disable

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SerializationDbContext : DbContext
    {
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<MeetingAttendee> Attendees { get; set; }

        public SerializationDbContext(DbContextOptions<SerializationDbContext> options)
            : base(options)
        {
        }
    }
}
