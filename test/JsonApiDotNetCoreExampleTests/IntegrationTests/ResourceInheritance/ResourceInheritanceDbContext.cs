using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class ResourceInheritanceDbContext : DbContext
    {
        public ResourceInheritanceDbContext(DbContextOptions<ResourceInheritanceDbContext> options) : base(options) { }
        
        public DbSet<Male> Males { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Human>()
                .ToTable("Persons")
                .HasDiscriminator<int>("Type")
                .HasValue<Male>(1)
                .HasValue<Female>(2);
            
            modelBuilder.Entity<Animal>()
                .ToTable("Animals")
                .HasDiscriminator<int>("Type")
                .HasValue<Cat>(1)
                .HasValue<Dog>(2);
            
            modelBuilder.Entity<Content>()
                .ToTable("Content")
                .HasDiscriminator<int>("Type")
                .HasValue<Video>(1)
                .HasValue<Book>(2);
            
            modelBuilder.Entity<HumanContentItem>()
                .HasKey(pp => new { ContentPersonId = pp.ContentId, PersonId = pp.HumanId });
        }
    }
}
