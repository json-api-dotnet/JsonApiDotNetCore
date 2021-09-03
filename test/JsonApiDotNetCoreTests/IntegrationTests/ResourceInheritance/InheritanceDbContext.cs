using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class InheritanceDbContext : DbContext
    {
        public DbSet<Human> Humans { get; set; }
        public DbSet<Man> Men { get; set; }
        public DbSet<CompanyHealthInsurance> CompanyHealthInsurances { get; set; }
        public DbSet<ContentItem> ContentItems { get; set; }

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
        }
    }
}
