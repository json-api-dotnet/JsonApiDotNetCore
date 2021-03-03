using JetBrains.Annotations;
using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class InheritanceDbContext : DbContext
    {
        public DbSet<Human> Humans { get; set; }
        public DbSet<Man> Men { get; set; }
        public DbSet<CompanyHealthInsurance> CompanyHealthInsurances { get; set; }
        public DbSet<ContentItem> ContentItems { get; set; }
        public DbSet<HumanFavoriteContentItem> HumanFavoriteContentItems { get; set; }

        public InheritanceDbContext(DbContextOptions<InheritanceDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Human>()
                .HasDiscriminator<int>("Type")
                .HasValue<Man>(1)
                .HasValue<Woman>(2);

            builder.Entity<HealthInsurance>()
                .HasDiscriminator<int>("Type")
                .HasValue<CompanyHealthInsurance>(1)
                .HasValue<FamilyHealthInsurance>(2);

            builder.Entity<ContentItem>()
                .HasDiscriminator<int>("Type")
                .HasValue<Video>(1)
                .HasValue<Book>(2);

            builder.Entity<HumanFavoriteContentItem>()
                .HasKey(item => new
                {
                    ContentPersonId = item.ContentItemId,
                    PersonId = item.HumanId
                });
        }
    }
}
