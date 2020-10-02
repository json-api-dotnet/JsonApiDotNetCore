using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class InheritanceDbContext : DbContext
    {
        public InheritanceDbContext(DbContextOptions<InheritanceDbContext> options) : base(options) { }
        
        public DbSet<Man> Males { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Human>()
                .ToTable("Persons")
                .HasDiscriminator<int>("Type")
                .HasValue<Man>(1)
                .HasValue<Woman>(2);
            
            modelBuilder.Entity<Animal>()
                .ToTable("Animals")
                .HasDiscriminator<int>("Type")
                .HasValue<Cat>(1)
                .HasValue<Dog>(2);
            
            modelBuilder.Entity<ContentItem>()
                .ToTable("Content")
                .HasDiscriminator<int>("Type")
                .HasValue<Video>(1)
                .HasValue<Book>(2);
            
            modelBuilder.Entity<HumanFavoriteContentItem>()
                .HasKey(pp => new { ContentPersonId = pp.ContentItemId, PersonId = pp.HumanId });
        }
    }
}
