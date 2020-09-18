using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class ResourceInheritanceDbContext : DbContext
    {
        public ResourceInheritanceDbContext(DbContextOptions<ResourceInheritanceDbContext> options)
            : base(options) { }
        
        public DbSet<Placeholder> Placeholders { get; set; }
        public DbSet<Person> Persons { get; set; }
        
        public DbSet<Male> Males { get; set; }
        
        public DbSet<Female> Females { get; set; }
        
        public DbSet<PlaceholderPerson> RelatedBaseResources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>()
                .ToTable("Persons")
                .HasDiscriminator<int>("Type")
                .HasValue<Male>(1)
                .HasValue<Female>(2);
            
            modelBuilder.Entity<PlaceholderPerson>()
                .HasKey(pp => new { pp.PlaceHolderId, pp.PersonId });
        }
    }
}
