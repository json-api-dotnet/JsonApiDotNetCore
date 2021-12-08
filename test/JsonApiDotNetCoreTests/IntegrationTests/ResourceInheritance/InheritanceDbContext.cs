using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class InheritanceDbContext : DbContext
{
    public DbSet<Human> Humans => Set<Human>();
    public DbSet<Man> Men => Set<Man>();
    public DbSet<CompanyHealthInsurance> CompanyHealthInsurances => Set<CompanyHealthInsurance>();
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();

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
