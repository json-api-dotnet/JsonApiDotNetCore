using GettingStarted.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GettingStarted.Data
{
    public class SampleDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        public SampleDbContext(DbContextOptions<SampleDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>();

            modelBuilder.Entity<UserRole>()
                .HasKey(userRole => new {userRole.UserProfileId, userRole.RoleId});

            modelBuilder.Entity<UserProfile>()
                .HasIndex(userProfile => userProfile.SubjectId).IsUnique();

            modelBuilder.Entity<UserProfile>()
                .HasIndex(userProfile => userProfile.Email).IsUnique();
        }
    }
}
