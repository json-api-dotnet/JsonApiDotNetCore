using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class InheritanceDbContext : DbContext
    {
        public InheritanceDbContext(DbContextOptions<InheritanceDbContext> options) : base(options) { }
        
        public DbSet<Human> Humans { get; set; }
        public DbSet<Man> Men { get; set; }
        public DbSet<CompanyHealthInsurance> CompanyHealthInsurances { get; set; }
        public DbSet<ContentItem> ContentItems { get; set; }
        public DbSet<HumanFavoriteContentItem> HumanFavoriteContentItems { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Human>()
                .HasDiscriminator<int>("Type")
                .HasValue<Man>(1)
                .HasValue<Woman>(2);
            
            modelBuilder.Entity<HealthInsurance>()
                .HasDiscriminator<int>("Type")
                .HasValue<CompanyHealthInsurance>(1)
                .HasValue<FamilyHealthInsurance>(2);
            
            modelBuilder.Entity<ContentItem>()
                .HasDiscriminator<int>("Type")
                .HasValue<Video>(1)
                .HasValue<Book>(2);
            
            modelBuilder.Entity<HumanFavoriteContentItem>()
                .HasKey(item => new { ContentPersonId = item.ContentItemId, PersonId = item.HumanId });
        }
    }
}
